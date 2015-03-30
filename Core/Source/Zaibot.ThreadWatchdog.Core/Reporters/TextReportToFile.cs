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
using System.IO;
using System.Threading;

namespace Zaibot.ThreadWatchdog.Core.Reporters
{
    /// <summary>
    /// Writes generated report texts to the specified file in the constructor.
    /// When a write attempt fails it will wait 10ms and retry a maximum 10 times, then the exception is rethrown.
    /// </summary>
    public sealed class TextReportToFile : TextReportBase
    {
        /// <summary>
        /// Maximum retry count for writing to the file.
        /// </summary>
        private const int MaxTryCount = 10;

        /// <summary>
        /// Used for synchronizing access within this process to the file.
        /// </summary>
        private readonly object _syncLock = new object();

        /// <summary>
        /// The output filename.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Separator appended after each message.
        /// </summary>
        public string Separator { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        public TextReportToFile(string filePath)
        {
            this.FilePath = filePath;
            this.Separator = new string('*', 100);
            this.DateTimeFormat = DefaultDateTimeFormat;
        }

        /// <summary>
        /// Called when the report text has been created.
        /// </summary>
        /// <param name="reportText"></param>
        protected override void OnReportText(string reportText)
        {
            var triesLeft = MaxTryCount;
            while (true)
            {
                try
                {
                    lock (this._syncLock)
                    {
                        File.AppendAllText(this.FilePath, string.Concat(reportText, this.Separator, Environment.NewLine));
                        break;
                    }
                }
                catch (IOException)
                {
                    if (triesLeft-- == 0)
                        throw;

                    Thread.Sleep(10);
                }
            }
        }
    }
}