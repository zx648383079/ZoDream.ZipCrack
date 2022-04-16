#include "pch.h"
#include "Logger.hpp";

Logger::Logger() : Level{ LogLevel::Debug } {

}

Logger::Logger(LogLevel level) : Level{level} {

}