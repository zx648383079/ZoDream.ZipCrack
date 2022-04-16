using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Loggers
{
    public class EventLogger : ILogger
    {
        public EventLogger(): this(LogLevel.Debug)
        {

        }

        public EventLogger(LogLevel level)
        {
            Level = level;
        }

        private bool isLoading = false;

        public LogLevel Level { get; private set; }

        public event LogEventHandler? OnLog;
        public event ProgressEventHandler? OnProgress;

        public void Error(string message)
        {
            Log(LogLevel.Error, message);
        }

        public void Debug(string message)
        {
            Log(LogLevel.Debug, message);
        }
        public void Info(string message)
        {
            Log(LogLevel.Info, message);
        }

        public void Log(string message)
        {
            Log(Level, message);
        }

        public void Log(LogLevel level, string message)
        {
            if (level >= Level)
            {
                OnLog?.Invoke(message, level);
            }
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void Progress(long current, long total)
        {
            if (isLoading)
            {
                return;
            }
            isLoading = true;
            Task.Factory.StartNew(() =>
            {

                OnProgress?.Invoke(current, total);
                isLoading = false;
            });
        }

        public void Warning(string message)
        {
            Log(LogLevel.Warn, message);
        }
    }

    public delegate void LogEventHandler(string message, LogLevel level);
    public delegate void ProgressEventHandler(long current, long total);
}
