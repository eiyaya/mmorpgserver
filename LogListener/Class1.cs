#region using

using NLog;
using NLog.Targets;

#endregion

[Target("LogListener")]
public sealed class LogListener : TargetWithLayout
{
    public static int ErrorCount;
    public static int FatalCount;
    public static int WarningCount;

    protected override void Write(LogEventInfo logEvent)
    {
        if (logEvent.Level == LogLevel.Fatal)
        {
            FatalCount++;
        }
        else if (logEvent.Level == LogLevel.Error)
        {
            ErrorCount++;
        }
        else if (logEvent.Level == LogLevel.Warn)
        {
            WarningCount++;
        }
    }
}