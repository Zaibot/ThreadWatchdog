# ThreadWatchdog
Generate traces on threads occupying the CPU above a certain threshold.

Allowing to debug infinite loops in production environments.

# Example use case
Used to debug a website application that was in production, it took text input and detected arbitrary links using a regular expression. This had caused a infinite loop on certain inputs due to an error in regular expression. There was no information of what went wrong and more importantly where, this is where the watchdog came in.

# Usage
```csharp
void ApplicationStartup()
{
    ThreadWatchdog.Instance.Subscribe(new WatchdogToFile(@"D:\ThreadWatchdog.txt"));
}
```
