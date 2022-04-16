#include "ConsoleLogger.hpp"
#include <ctime>
#include <iomanip>

ConsoleLogger::ConsoleLogger(LogLevel level, std::ostream& os)
: Logger{level}, m_os{os}
{}

void ConsoleLogger::Log(LogLevel level, const char* message) {
    std::time_t t = std::time(nullptr);
    std::tm td;
    localtime_s(&td, &t);
    m_os << "[" << std::put_time(&td, "%T") << "]" << message << std::endl;
    last_is_progress = false;
}

void ConsoleLogger::Progress(long current, long total) {
    if (last_is_progress) {
        m_os << "\r";
    }
    m_os << "Progress:" << current << "/" << total << std::endl;
    last_is_progress = true;
}

bool ConsoleLogger::Callback(int progress, int total, const char* str)
{
    if (total < 0) {
        Info(str);
    }
    else {
        Progress(progress, total);
    }
    return !IsCancellationRequested;
}

ConsoleLogger::~ConsoleLogger()
{
    
}
