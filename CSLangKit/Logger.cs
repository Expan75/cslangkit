namespace CSLangKit;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

public enum LogLevel
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3
}

public class Logger
{
    private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Info;
    private const string NAMESPACE = "main";

    private StreamWriter LogFile { get; set; }
    private LogLevel CurrentLevel { get; set; } = DEFAULT_LOG_LEVEL;
    private List<Action<string>> Sinks { get; set; } = new List<Action<string>>();

    private static string FormatLogMessage(string message, LogLevel level) =>
        $"{DateTime.Now} - {NAMESPACE} - {level} - {message}";

    private void WriteLogMessage(string message, LogLevel level)
    {
        var FormattedMessage = FormatLogMessage(message, level);
        if (level < this.CurrentLevel)
        {
            return;
        } // suppress if not valid level
        foreach (Action<string> sink in this.Sinks)
        {
            sink.Invoke(FormattedMessage); // this just calls a function. It has no
            // implications w.r.t. UI thread.
        }
    }

    public Logger(LogLevel level = DEFAULT_LOG_LEVEL)
    {
        this.CurrentLevel = level;
    }

    public void AddSink(Action<string> sink)
    {
        this.Sinks.Add(sink);
    }

    // sets up a new logger with a logfile sink already attached.
    public static Logger ToFile(string filepath, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        Logger logger = new Logger(level);
        logger.LogFile = File.CreateText(filepath);
        logger.Sinks.Add(
            (msg) =>
            {
                logger.LogFile.WriteLine(msg);
                logger.LogFile.Flush();
            }
        );
        return logger;
    }

    // util methods for cleaner downstream code
    public void Debug(string msg)
    {
        this.WriteLogMessage(msg, LogLevel.Debug);
    }

    public void Info(string msg)
    {
        this.WriteLogMessage(msg, LogLevel.Info);
    }

    public void Warning(string msg)
    {
        this.WriteLogMessage(msg, LogLevel.Warning);
    }

    public void Error(string msg)
    {
        this.WriteLogMessage(msg, LogLevel.Error);
    }
}
