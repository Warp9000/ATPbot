using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace ATPbot.Logging;

public class Logger
{
    public Logger(Severity minimumSeverity = Severity.Info, string? logFile = null, string? crashFile = null)
    {
        MinimumSeverity = minimumSeverity;
        LogFile = logFile;
        CrashFile = crashFile;
        if (logFile is not null)
            logFileWriter = File.AppendText(logFile);
        if (crashFile is not null)
        {
            crashFileWriter = File.AppendText(crashFile);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((sender, args) =>
            {
                if (args.ExceptionObject is not Exception exception) return;
                onUnhandledException(exception);
            });
            // handle exceptions in other threads
            TaskScheduler.UnobservedTaskException += new EventHandler<UnobservedTaskExceptionEventArgs>((sender, args) =>
            {
                onUnhandledException(args.Exception);
            });
        }
    }

    /// <summary>
    ///     The minimum severity of messages to log to the console and file.
    /// </summary>
    public Severity MinimumSeverity { get; set; } = Severity.Info;

    /// <summary>
    ///    The file to log to. If null, logging to a file is disabled.
    /// </summary>
    public string? LogFile { get; set; }

    /// <summary>
    ///    The file to flush MessageHistory to on crash. If null, crash logging is disabled.
    /// </summary>
    public string? CrashFile { get; set; }

    /// <summary>
    ///     A queue of the last 1024 log messages regardless of severity.
    /// </summary>
    public Queue<LogMessage> MessageHistory { get; } = new(1024);

    private StreamWriter? logFileWriter;
    private StreamWriter? crashFileWriter;

    public void Log(LogMessage message)
    {
        lock (MessageHistory)
            MessageHistory.Enqueue(message);

        if (message.Severity < MinimumSeverity) return;

        Write(message);
    }

    public void Log(string message, string source, Severity severity = Severity.Info)
    {
        var logMessage = new LogMessage(message, source, severity);
        Log(logMessage);
    }

    public void Log(string message, object source, Severity severity = Severity.Info)
    {
        var logMessage = new LogMessage(message, source, severity);
        Log(logMessage);
    }

    public void Log(Exception exception, string source, Severity severity = Severity.Error)
    {
        var logMessage = new LogMessage(exception.ToString(), source, severity);
        Log(logMessage);
    }

    public void Log(Exception exception, object source, Severity severity = Severity.Error)
    {
        var logMessage = new LogMessage(exception.ToString(), source, severity);
        Log(logMessage);
    }

    public Task DiscordLog(Discord.LogMessage message)
    {
        var logMessage = new LogMessage(message.Message, message.Source, message.Severity.ToSeverity());
        if (message.Exception != null)
            logMessage.Message += $"\n{message.Exception}";
        logMessage.Source = "Discord." + logMessage.Source;
        Log(logMessage);
        return Task.CompletedTask;
    }

    private string Format(LogMessage message)
    {
        return $"[{message.Timestamp:HH:mm:ss}] [{message.Severity}]\t[{message.Source}] {message.Message}";
    }

    private void Write(LogMessage message)
    {
        Console.ForegroundColor = message.Severity switch
        {
            Severity.Debug => ConsoleColor.DarkGray,
            Severity.Verbose => ConsoleColor.Gray,
            Severity.Info => ConsoleColor.White,
            Severity.Warning => ConsoleColor.Yellow,
            Severity.Error => ConsoleColor.Red,
            _ => ConsoleColor.White
        };
        Console.WriteLine(Format(message));
        Console.ResetColor();

        if (logFileWriter is not null)
            WriteToFile(message, logFileWriter);

    }

    private void WriteToFile(LogMessage message, StreamWriter writer)
    {
        lock (writer)
        {
            var directory = Path.GetDirectoryName(LogFile);
            if (directory is not null && !Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            writer.WriteLine(Format(message));
            writer.Flush();
        }
    }

    private void onUnhandledException(Exception exception)
    {
        Log(exception.ToString(), "Unhandled Exception", Severity.Error);
        lock (MessageHistory)
        {
            foreach (var message in MessageHistory)
                WriteToFile(message, crashFileWriter!);
        }
    }

    public static string GetUniqueFileName(string prefix, string extension = "log", bool includeDate = true)
    {
        var fileName = string.IsNullOrEmpty(prefix) ? "" : $"{prefix}-";
        if (includeDate)
            fileName += $"{DateTime.Now:yyyy-MM-dd}";
        if (File.Exists($"{fileName}.{extension}"))
        {
            var i = 1;
            while (File.Exists($"{fileName}-{i}.{extension}"))
                i++;
            fileName += $"-{i}";
        }
        return $"{fileName}.{extension}";
    }
}