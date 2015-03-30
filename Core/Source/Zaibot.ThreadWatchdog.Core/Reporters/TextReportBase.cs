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
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;

// Thread.Suspend, Thread.Resume and new StackTrace(Thread...) have been marked as obsolete.
#pragma warning disable 618

namespace Zaibot.ThreadWatchdog.Core.Reporters
{
    public abstract class TextReportBase : IWatchdogSubscriber
    {
        protected const string DefaultDateTimeFormat = "yyyy-MM-dd HH:mm:ss zzz";

        /// <summary>
        /// The date time format strings, by default <c>yyyy-MM-dd HH:mm:ss zzz</c>.
        /// </summary>
        public string DateTimeFormat { get; set; }

        void IWatchdogSubscriber.OnReport(WatchdogReport report)
        {
            var thread = report.Thread;
            if (thread != Thread.CurrentThread)
            {
                StackTrace trace = null;
                Exception error = null;

                Thread.BeginThreadAffinity();
                Thread.BeginCriticalRegion();
                try
                {
                    thread.Suspend();
                    try
                    {
                        trace = new StackTrace(thread, true);
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                    }
                    finally
                    {
                        thread.Resume();
                    }
                }
                finally
                {
                    Thread.EndCriticalRegion();
                    Thread.EndThreadAffinity();
                }

                if (error != null)
                {
                    var cpuUsage = report.Usage;

                    var errorMessage = new StringBuilder(1024);
                    errorMessage.AppendLine(DateTime.Now.ToString(this.DateTimeFormat, CultureInfo.InvariantCulture));
                    errorMessage.AppendLine(string.Format("{0} [{1}] @ {2:P} CPU usage", Process.GetCurrentProcess().ProcessName, report.Thread.ManagedThreadId, cpuUsage));
                    errorMessage.AppendLine("Unable to read the stack trace due to an error: " + error.Message);
                    errorMessage.AppendLine();
                    errorMessage.AppendLine(error.StackTrace);

                    this.OnReportText(errorMessage.ToString());
                }
                else
                {
                    var cpuUsage = report.Usage;

                    var reportText = new StringBuilder(1024);
                    reportText.AppendLine(DateTime.Now.ToString(this.DateTimeFormat));
                    reportText.AppendLine(string.Format("{0} [{1}] @ {2:P} CPU usage", Process.GetCurrentProcess().ProcessName, report.Thread.ManagedThreadId, cpuUsage));
                    reportText.AppendLine();
                    reportText.AppendLine("Thread exceeded threshold CPU usage.");
                    reportText.AppendLine(trace.ToString());

                    this.OnReportText(reportText.ToString());
                }
            }
        }

        /// <summary>
        /// Called when the report text has been created.
        /// </summary>
        /// <param name="reportText"></param>
        protected abstract void OnReportText(string reportText);
    }
}