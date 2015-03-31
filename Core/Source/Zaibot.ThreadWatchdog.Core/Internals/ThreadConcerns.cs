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
using System.Runtime.InteropServices;
using Zaibot.ThreadWatchdog.Core.Native;

namespace Zaibot.ThreadWatchdog.Core.Internals
{
    internal static class ThreadConcerns
    {
        internal static IntPtr GetCurrentThreadHandle()
        {
            // Returns a pseudo handle, duplicate to get the actual handle value.
            IntPtr actualThread;

            var processHandle = Kernel32Api.GetCurrentProcess();
            var threadHandle = Kernel32Api.GetCurrentThread();

            var duplicateHandle = Kernel32Api.DuplicateHandle(processHandle, threadHandle, processHandle, out actualThread, 0, false, Kernel32Api.DUPLICATE_SAME_ACCESS);
            if (duplicateHandle == false)
                throw new WatchdogException("Unable to duplicate the native thread handle. " + Marshal.GetLastWin32Error() + " :: " + threadHandle.ToInt64().ToString("X16") + " :: " + processHandle.ToInt64().ToString("X16"));

            return actualThread;
        }
    }
}