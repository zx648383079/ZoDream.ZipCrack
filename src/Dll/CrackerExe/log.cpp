#include "log.hpp"
#include "version.hpp"
#include <ctime>
#include <iomanip>

std::ostream& setupLog(std::ostream& os)
{
    return os << std::setfill('0') // leading zeros for keys
              << std::fixed << std::setprecision(1) // for progress percentage
              << "bkcrack " BKCRACK_VERSION " - " BKCRACK_COMPILATION_DATE; // version information
}

std::ostream& put_time(std::ostream& os)
{
    std::time_t t = std::time(nullptr);
    std::tm d;
    localtime_s(&d, &t);
    return os << std::put_time(&d, "%T");
}

std::ostream& operator<<(std::ostream& os, const KeyStruct& keys)
{
    return os << std::hex
              << std::setw(8) << keys.x << " "
              << std::setw(8) << keys.y << " "
              << std::setw(8) << keys.z
              << std::dec;
}
