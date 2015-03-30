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
using Zaibot.ThreadWatchdog.Core.Native;

namespace Zaibot.ThreadWatchdog.Core.Internals
{
    internal static class ThreadConcerns
    {
        private static readonly IntPtr ProcessHandle = Process.GetCurrentProcess().Handle;

        internal static IntPtr GetCurrentThreadHandle()
        {
            var pseudoThread = Kernel32Api.GetCurrentThread();

            // Returns a pseudo handle, duplicate to get the actual handle value.
            IntPtr actualThread;

            var duplicateHandle = Kernel32Api.DuplicateHandle(ProcessHandle, pseudoThread, ProcessHandle, out actualThread, 0, true, Kernel32Api.DUPLICATE_SAME_ACCESS);
            if (duplicateHandle == false)
                throw new WatchdogException("Unable to duplicate the native thread handle.");

            return actualThread;
        }
    }
}