#ifndef BKCRACK_CONSOLEPROGRESS_HPP
#define BKCRACK_CONSOLEPROGRESS_HPP

#include <mutex>
#include <condition_variable>
#include <thread>
#include <iostream>
#include "Logger.hpp"

/// Progress indicator which prints itself at regular time intervals
class ConsoleLogger : public Logger
{
public:
    /// Start a thread to print progress
    ConsoleLogger(LogLevel level, std::ostream& os);

    /// Notify and stop the printing thread
    ~ConsoleLogger();

    void Log(LogLevel level, const char* message);

    void Progress(long current, long total);

    bool Callback(int progress, int total, const char* str);

private:
    std::ostream& m_os;
    bool last_is_progress = false; // ��һ����Ϣ�Ƿ��ǽ���
};

#endif // BKCRACK_CONSOLEPROGRESS_HPP
