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

using System.IO;

namespace Zaibot.ThreadWatchdog.Core.Reporters
{
    /// <summary>
    /// Writes generated report texts to the <see cref="TextWriter"/> specified in the constructor.
    /// The creator is responsible for the lifetime of the <see cref="TextWriter"/>.
    /// </summary>
    public sealed class TextReportToStream : TextReportBase
    {
        /// <summary>
        /// Used for synchronizing access to the writer.
        /// </summary>
        private readonly object _syncLock = new object();

        /// <summary>
        /// The output writer.
        /// </summary>
        public TextWriter Writer { get; private set; }

        /// <summary>
        /// Separator appended after each message.
        /// </summary>
        public string Separator { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer">Output writer for the reports (caller is responsible for the lifetime of the <paramref name="writer"/>).</param>
        public TextReportToStream(TextWriter writer)
        {
            this.Writer = writer;
            this.Separator = new string('*', 100);
            this.DateTimeFormat = DefaultDateTimeFormat;
        }

        /// <summary>
        /// Called when the report text has been created.
        /// </summary>
        /// <param name="reportText"></param>
        protected override void OnReportText(string reportText)
        {
            lock (this._syncLock)
            {
                this.Writer.WriteLine(reportText);
                this.Writer.WriteLine(this.Separator);
            }
        }
    }
}