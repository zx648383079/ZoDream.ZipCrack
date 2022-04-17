#pragma once
#include "pch.h"
#include <stdio.h>

const int LOG_BUF_LEN = 200;

#ifndef LOG_MSG_FORMAT
#define LOG_MSG_FORMAT char buffer[LOG_BUF_LEN];\
va_list p;\
va_start(p, message);\
vsprintf_s(buffer, message, p);\
va_end(p);
#endif // !LOG_MSG_FORMAT


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

    void Log(const char* message, ...) {
        LOG_MSG_FORMAT
        Log(Level, buffer);
    }

    void Info(const char* message, ...) {
        LOG_MSG_FORMAT
        Log(LogLevel::Info, buffer);
    }

    void Debug(const char* message, ...) {
        LOG_MSG_FORMAT
        Log(LogLevel::Debug, buffer);
    }

    void Warning(const char* message, ...) {
        LOG_MSG_FORMAT
        Log(LogLevel::Warn, buffer);
    }

    void Error(const char* message, ...) {
        LOG_MSG_FORMAT
        Log(LogLevel::Error, buffer);
    }

    virtual void Progress(long current, long total) = 0;

protected:
    LogLevel Level;
};