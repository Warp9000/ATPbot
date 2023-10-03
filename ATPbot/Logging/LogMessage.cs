using System;

namespace ATPbot.Logging;

public class LogMessage
{
    public LogMessage(string message, string source, Severity severity = Severity.Info)
    {
        Message = message;
        Source = source;
        Severity = severity;
        Timestamp = DateTime.Now;
    }
    public LogMessage(string message, object source, Severity severity = Severity.Info)
    {
        Message = message;
        switch (source)
        {
            case Type type:
                Source = type.FullName ?? type.Name;
                break;
            case string str:
                Source = str;
                break;
            default:
                Source = source.GetType().FullName ?? source.GetType().Name;
                break;
        }
        Severity = severity;
        Timestamp = DateTime.Now;
    }
    public string Message { get; set; }
    public string Source { get; set; }
    public Severity Severity { get; set; }
    public DateTime Timestamp { get; set; }
}