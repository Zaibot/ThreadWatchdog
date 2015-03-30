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
using System.Runtime.InteropServices;

namespace Zaibot.ThreadWatchdog.Core.Native
{
    internal static class Kernel32Api
    {
        internal const uint DUPLICATE_SAME_ACCESS = 0x00000002;

        [DllImport("kernel32.dll", EntryPoint = "GetCurrentThread", ExactSpelling = true)]
        internal static extern IntPtr GetCurrentThread();

        [DllImport("kernel32.dll", EntryPoint = "GetThreadTimes", ExactSpelling = true, SetLastError = true)]
        internal static extern long GetThreadTimes(IntPtr threadHandle, out long createionTime, out long exitTime, out long kernelTime, out long userTime);

        [DllImport("kernel32.dll", EntryPoint = "DuplicateHandle", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, IntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle, uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwOptions);
    }
}