namespace ATPbot.Logging;

public static class Extensions
{
    public static Discord.LogSeverity ToDiscordSeverity(this Severity severity)
    {
        return severity switch
        {
            Severity.Debug => Discord.LogSeverity.Debug,
            Severity.Verbose => Discord.LogSeverity.Verbose,
            Severity.Info => Discord.LogSeverity.Info,
            Severity.Warning => Discord.LogSeverity.Warning,
            Severity.Error => Discord.LogSeverity.Error,
            _ => Discord.LogSeverity.Info
        };
    }

    public static Severity ToSeverity(this Discord.LogSeverity severity)
    {
        return severity switch
        {
            Discord.LogSeverity.Debug => Severity.Debug,
            Discord.LogSeverity.Verbose => Severity.Verbose,
            Discord.LogSeverity.Info => Severity.Info,
            Discord.LogSeverity.Warning => Severity.Warning,
            Discord.LogSeverity.Error => Severity.Error,
            _ => Severity.Info
        };
    }
}