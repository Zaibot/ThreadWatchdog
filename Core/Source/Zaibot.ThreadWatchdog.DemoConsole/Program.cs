using System;
using Zaibot.ThreadWatchdog.Core;
using Zaibot.ThreadWatchdog.Core.Reporters;

namespace Zaibot.ThreadWatchdog.DemoConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Subscribe to the reports using the console output.
            Watchdog.Instance.Subscribe(new TextReportToStream(Console.Out));

            // Start the watchdog thread.
            Watchdog.Instance.Start();

            // Start monitoring the current thread.
            Watchdog.Instance.MonitorCurrentThread();
            for (;;)
            {
            }
        }
    }
}
