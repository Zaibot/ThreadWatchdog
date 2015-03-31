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
using Zaibot.ThreadWatchdog.Core.Native;

namespace Zaibot.ThreadWatchdog.Core.Internals
{
    /// <summary>
    /// Calculates the CPU time used by the native thread that called the constructor.
    /// </summary>
    internal sealed class ThreadTimes : IDisposable
    {
        private readonly IntPtr _threadHandle;
        private long _startTime;

        internal ThreadTimes()
        {
            this._threadHandle = ThreadConcerns.GetCurrentThreadHandle();
            this.Restart();
        }

        internal void Restart()
        {
            this._startTime = this.GetThreadTimes();
        }

        internal TimeSpan GetElapsedTime()
        {
            return TimeSpan.FromTicks(this.GetThreadTimes() - this._startTime);
        }

        private long GetThreadTimes()
        {
            long unused, kernelTime, userTime;

            var retcode = Kernel32Api.GetThreadTimes(this._threadHandle, out unused, out unused, out kernelTime, out userTime);
            if (retcode == 0)
                return -1;

            var totalTime = kernelTime + userTime;

            return totalTime;
        }

        public void Dispose()
        {
            Kernel32Api.CloseHandle(_threadHandle);
        }
    }
}