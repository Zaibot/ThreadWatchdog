# ThreadWatchdog
Generate traces on threads occupying the CPU above a certain threshold.

Allowing to debug infinite loops in production environments without the need to be connected until it happens.

## Example use case
Used to debug a website application that was in production, it took text input and detected arbitrary links using a regular expression. This had caused a infinite loop on certain inputs due to an error in regular expression. There was no information of what went wrong and more importantly where, this is where the watchdog came in.

## Usage
```csharp
void ApplicationStartup()
{
    Watchdog.Instance.Subscribe(new TextReportToFile(@"C:\Temp\ThreadWatchdog.txt"));
    Watchdog.Instance.Start();
}

void SuspiciousCode()
{
    Watchdog.Instance.MonitorCurrentThread();
}
```

## ASP.NET Example
Add the following calls to the Global application class.

```csharp
void Application_Start()
{
    Watchdog.Instance.Subscribe(new TextReportToFile(@"C:\Temp\ThreadWatchdog.txt"));
    Watchdog.Instance.Start();
}

void Application_BeginRequest()
{
    Watchdog.Instance.MonitorCurrentThread();
}
```

## Example Output
```
2015-03-30 22:40:59 +02:00
Zaibot.ThreadWatchdog.DemoConsole [1] @ 99.97 % CPU usage

Thread exceeded threshold CPU usage.
   at Zaibot.ThreadWatchdog.DemoConsole.Program.Main(String[] args) in D:\...\Zaibot.ThreadWatchdog.DemoConsole\Program.cs:line 36
```
