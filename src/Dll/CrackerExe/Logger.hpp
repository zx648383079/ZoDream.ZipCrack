#pragma once

class Logger
{
public:
    enum class LogLevel
    {
        NotSet,
        Debug,
        Info,
        Warn,
        Error,
        Fatal,
        Audit
    };
    bool IsCancellationRequested = false;

    Logger();
    Logger(LogLevel level);

    virtual void Log(LogLevel level, const char* message) = 0;

    void Log(const char* message) {
        Log(Level, message);
    }

    void Info(const char* message) {
        Log(LogLevel::Info, message);
    }

    void Debug(const char* message) {
        Log(LogLevel::Debug, message);
    }

    void Warning(const char* message) {
        Log(LogLevel::Warn, message);
    }

    void Error(const char* message) {
        Log(LogLevel::Error, message);
    }

    virtual void Progress(long current, long total) = 0;

protected:
    LogLevel Level;
};