// 
// The MIT License (MIT)
// 
// Copyright (c) 2015 Zaibot Programs
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Zaibot.ThreadWatchdog.Core.Internals;

namespace Zaibot.ThreadWatchdog.Core
{
    public sealed class Watchdog
    {
        /// <summary>
        /// Singleton.
        /// </summary>
        public static readonly Watchdog Instance = new Watchdog();

        /// <summary>
        /// Cached.
        /// </summary>
        private static readonly int ProcessorCount = Environment.ProcessorCount;

        private static readonly TimeSpan MinimumInterval = TimeSpan.FromMilliseconds(1);

        /// <summary>
        /// Constructor, starts up the watchdog thread.
        /// </summary>
        public Watchdog()
        {
            this.Interval = TimeSpan.FromSeconds(1);
            this.ReportThreshold = 0.75d;
        }

        #region Error Handling
        /// <summary>
        /// Called when an error occurred.
        /// </summary>
        public Action<Exception> Error { get; set; }

        /// <summary>
        /// Ignores thrown exceptions when set to <c>false</c> (default <c>false</c>).
        /// </summary>
        public bool ThrowErrors { get; set; }

        /// <summary>
        /// Called when an exception occurred, the exception will be forwarded to <see cref="Error"/>.
        /// </summary>
        /// <param name="ex"></param>
        /// <remarks>
        /// Errors thrown by code in the <see cref="Error"/> will be ignored if <see cref="ThrowErrors"/> is <c>false</c>.
        /// </remarks>
        private void OnError(Exception ex)
        {
            var action = this.Error;
            if (action != null)
            {
                try
                {
                    action(ex);
                }
                catch
                {
                    if (this.ThrowErrors)
                        throw;
                }
            }
        }
        #endregion

        #region Life Time Methods
        public void Start()
        {
            lock (this._workerLock)
            {
                if (this._workerThread == null)
                {
                    this._workerStopEvent.Reset();

                    this._workerThread = this.CreateWorkerThread();
                    this._workerThread.Start();
                }
            }
        }

        public void Stop()
        {
            lock (this._workerLock)
            {
                if (this._workerThread != null)
                {
                    this._workerStopEvent.Set();
                    if (this._workerThread.Join(TimeSpan.FromSeconds(5)) == false)
                    {
                        this._workerThread.Abort();
                    }
                    this._workerThread = null;
                }
            }
        }
        #endregion

        #region Worker
        private readonly object _workerLock = new object();
        private readonly ManualResetEvent _workerStopEvent = new ManualResetEvent(false);
        private Thread _workerThread;

        /// <summary>
        /// Determines whether the watchdog worker thread is running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                var thread = this._workerThread;

                return thread != null && thread.IsAlive;
            }
        }

        /// <summary>
        /// Construct a new worker thread.
        /// </summary>
        /// <returns></returns>
        private Thread CreateWorkerThread()
        {
            return new Thread(this.WorkerProc) {IsBackground = true, Priority = ThreadPriority.Highest, Name = "ThreadWatchdog"};
        }

        /// <summary>
        /// The watchdog process that evaluates the time spent by tracked threads.
        /// </summary>
        private void WorkerProc()
        {
            while (this._workerStopEvent.WaitOne(this.Interval, true) == false)
            {
                try
                {
                    var trackers = this._trackers;

                    foreach (var tracker in trackers)
                    {
                        if (tracker.Thread.IsAlive == false)
                        {
                            this.Remove(tracker);
                            continue;
                        }

                        // Extract the time.
                        var alive = tracker.AliveTimer.Elapsed;
                        var execution = tracker.ExecutionTimer.GetElapsedTime();

                        // Restart our counters.
#if NET35
                        tracker.AliveTimer.Reset();
                        tracker.AliveTimer.Start();
#else
                        tracker.AliveTimer.Restart();
#endif
                        tracker.ExecutionTimer.Restart();

                        // Calculate if exceeded the threshold, then report if necessary.
                        var cpuTime = execution.TotalMilliseconds;
                        var triggerTime = alive.TotalMilliseconds*this.ReportThreshold;
                        if (cpuTime > triggerTime)
                        {
                            var report = new WatchdogReport();
                            report.Thread = tracker.Thread;
                            report.TimeElapsed = alive;
                            report.CpuTimeElapsed = execution;
                            report.Usage = cpuTime/alive.TotalMilliseconds;
                            this.Report(report);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.OnError(ex);

                    if (this.ThrowErrors)
                        throw;
                }
            }
        }
        #endregion

        #region Configuration
        private double _reportThreshold;

        /// <summary>
        /// The ratio from 0 to 1, specifying the maximum amount of time a thread can use per processor. When exceed the <see cref="IWatchdogSubscriber"/>s will be invoked. Default: <c>0.75</c>.
        /// </summary>
        public double ReportThreshold
        {
            get { return this._reportThreshold; }
            set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException("value", value, "Report threshold should be between 0.0 and 1.0.");
                }

                this._reportThreshold = value;
            }
        }

        private TimeSpan _interval;

        /// <summary>
        /// Specified the time in between check iterations on the watched threads.
        /// </summary>
        public TimeSpan Interval
        {
            get { return this._interval; }
            set
            {
                if (value < MinimumInterval)
                {
                    throw new ArgumentOutOfRangeException("value", value, "Interval should be more then 1ms.");
                }

                this._interval = value;
            }
        }
        #endregion

        #region Tracing
        private class Tracker
        {
            internal Thread Thread { get; set; }
            internal Stopwatch AliveTimer { get; set; }
            internal ThreadTimes ExecutionTimer { get; set; }
        }

        private readonly object _trackersLock = new object();
        private List<Tracker> _trackers = new List<Tracker>();

        /// <summary>
        /// Removes the specified tracker.
        /// </summary>
        /// <param name="tracker"></param>
        private void Remove(Tracker tracker)
        {
            lock (this)
            {
                tracker.ExecutionTimer.Dispose();

                var threadDebugs = new List<Tracker>(this._trackers);
                threadDebugs.Remove(tracker);
                this._trackers = threadDebugs;
            }
        }

        /// <summary>
        /// Determines whether the specified thread is being monitored.
        /// </summary>
        /// <param name="thread"></param>
        /// <returns></returns>
        private bool IsMonitored(Thread thread)
        {
            var trackers = this._trackers;

            foreach (var tracker in trackers)
            {
                if (tracker.Thread == thread)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Start monitoring the current thread.
        /// </summary>
        public void MonitorCurrentThread()
        {
            var thread = Thread.CurrentThread;

            var isRegistered = this.IsMonitored(thread);
            if (isRegistered)
            {
                // Nothing todo.
                return;
            }

            // Create a new registration.
            var item = new Tracker();
            item.Thread = thread;
            item.AliveTimer = Stopwatch.StartNew();
            item.ExecutionTimer = new ThreadTimes();

            lock (this)
            {
                var trackers = new List<Tracker>(this._trackers);

                // Remove old trackers.
                trackers.Add(item);
                this._trackers = trackers;
            }
        }
        #endregion

        #region Subscribers
        private List<IWatchdogSubscriber> _subscribers = new List<IWatchdogSubscriber>();

        /// <summary>
        /// Dispatches the specified <paramref name="report"/> to the subscribers.
        /// </summary>
        /// <param name="report"></param>
        private void Report(WatchdogReport report)
        {
            var subscribers = this._subscribers;

            foreach (var subscriber in subscribers)
            {
                try
                {
                    subscriber.OnReport(report);
                }
                catch (Exception ex)
                {
                    this.OnError(ex);

                    if (this.ThrowErrors)
                        throw;
                }
            }
        }

        /// <summary>
        /// Unsubscribe the specified listener.
        /// </summary>
        /// <param name="subscriber"></param>
        public void Unsubscribe(IWatchdogSubscriber subscriber)
        {
            lock (this._trackersLock)
            {
                var list = new List<IWatchdogSubscriber>(this._subscribers);
                list.Remove(subscriber);

                this._subscribers = list;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriber"></param>
        public void Subscribe(IWatchdogSubscriber subscriber)
        {
            lock (this._trackersLock)
            {
                var list = new List<IWatchdogSubscriber>(this._subscribers);
                list.Add(subscriber);

                this._subscribers = list;
            }
        }
        #endregion
    }
}