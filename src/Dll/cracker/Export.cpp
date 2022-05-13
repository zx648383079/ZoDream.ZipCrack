#include "pch.h"
#include <cstdint>
#include "Cracker.hpp"
#include "Export.hpp"


ExportLogger::ExportLogger(LogLevel level, ProgressCallBack callback)
    : Logger{ level }, progressCallback{callback}
{
}

void ExportLogger::Log(LogLevel level, const char* message)
{
    if (!progressCallback(0, -1, message))
    {
        IsCancellationRequested = true;
    }
}

void ExportLogger::Progress(long current, long total)
{
    if (!progressCallback(current, total, ""))
    {
        IsCancellationRequested = true;
    }
}

KeyStruct parseKey(std::vector<Keys> keysvec) {
    KeyStruct res{};
    if (keysvec.empty())
    {
        return res;
    }
    else
    {
        res.x = keysvec[0].getX();
        res.y = keysvec[0].getY();
        res.z = keysvec[0].getZ();
        return res;
    }
}

Keys parseKey(KeyStruct keys)
{
    return Keys(keys.x, keys.y, keys.z);
}

KeyStruct __stdcall FindKey(const char* zipFile, const char* zipFileName, const char* plainFile, const char* plainFileName, ProgressCallBack callback)
{
    ExportLogger logger(Logger::LogLevel::Debug, callback);
    Cracker cracker(logger);
    std::vector<Keys> keysvec;
    try
    {
        keysvec = cracker.FindKey(zipFile, zipFileName, plainFile, plainFileName);
        return parseKey(keysvec);
    }
    catch (const std::exception& ex)
    {
        logger.Error(ex.what());
        return parseKey(keysvec);
    }
}

KeyStruct __stdcall FindKey2(const char* zipFile, long cipherBegin, long cipherEnd, const char* plainFile, long plainBegin, long plainEnd, ProgressCallBack callback)
{
    ExportLogger logger(Logger::LogLevel::Debug, callback);
    Cracker cracker(logger);
    std::vector<Keys> keysvec;
    try
    {
        keysvec = cracker.FindKey(zipFile, cipherBegin, cipherEnd, plainFile, plainBegin, plainEnd);
        return parseKey(keysvec);
    }
    catch (const std::exception& ex)
    {
        logger.Error(ex.what());
        return parseKey(keysvec);
    }
}

KeyStruct __stdcall FindKey3(const char* zipFile, const char* zipFileName, const char* plainFile, ProgressCallBack callback)
{
    ExportLogger logger(Logger::LogLevel::Debug, callback);
    Cracker cracker(logger);
    std::vector<Keys> keysvec;
    try
    {
        keysvec = cracker.FindKey(zipFile, zipFileName, plainFile);
        return parseKey(keysvec);
    }
    catch (const std::exception& ex)
    {
        logger.Error(ex.what());
        return parseKey(keysvec);
    }
}

KeyStruct __stdcall FindKey4(const char* zipFile, const char* zipFileName, const byte* plainData, int plainLength, ProgressCallBack callback)
{
    ExportLogger logger(Logger::LogLevel::Debug, callback);
    Cracker cracker(logger);
    std::vector<Keys> keysvec;
    try
    {
        bytevec plainVec;
        for (size_t i = 0; i < plainLength; i++)
        {
            plainVec.push_back(plainData[i]);
        }
        keysvec = cracker.FindKey(zipFile, zipFileName, plainVec);
        return parseKey(keysvec);
    }
    catch (const std::exception& ex)
    {
        logger.Error(ex.what());
        return parseKey(keysvec);
    }
}

KeyStruct __stdcall FindKey5(const char* zipFile, long cipherBegin, long cipherEnd, const byte* plainData, int plainLength, ProgressCallBack callback)
{
    ExportLogger logger(Logger::LogLevel::Debug, callback);
    Cracker cracker(logger);
    std::vector<Keys> keysvec;
    try
    {
        bytevec plainVec;
        for (size_t i = 0; i < plainLength; i++)
        {
            plainVec.push_back(plainData[i]);
        }
        keysvec = cracker.FindKey(zipFile, cipherEnd, cipherEnd, plainVec);
        return parseKey(keysvec);
    }
    catch (const std::exception& ex)
    {
        logger.Error(ex.what());
        return parseKey(keysvec);
    }
}

KeyStruct __stdcall FindKey6(const byte* zipData, int zipLength, const byte* plainData, int plainLength, ProgressCallBack callback)
{
    ExportLogger logger(Logger::LogLevel::Debug, callback);
    Cracker cracker(logger);
    std::vector<Keys> keysvec;
    try
    {
        bytevec plainVec;
        for (size_t i = 0; i < plainLength; i++)
        {
            plainVec.push_back(plainData[i]);
        }
        bytevec zipVec;
        for (size_t i = 0; i < zipLength; i++)
        {
            zipVec.push_back(zipData[i]);
        }
        keysvec = cracker.FindKey(zipVec, plainVec);
        return parseKey(keysvec);
    }
    catch (const std::exception& ex)
    {
        logger.Error(ex.what());
        return parseKey(keysvec);
    }
}

bool __stdcall Pack(KeyStruct keys, const char* zipFile, const char* distFile, ProgressCallBack callback)
{
    ExportLogger logger(Logger::LogLevel::Debug, callback);
    Cracker cracker(logger);
    try
    {
        return cracker.Pack(parseKey(keys), zipFile, distFile);
    }
    catch (const std::exception& ex)
    {
        logger.Error(ex.what());
        return false;
    }
}

bool __stdcall Pack2(KeyStruct keys, const char* zipFile, const char* distFile, const char* password, ProgressCallBack callback)
{
    ExportLogger logger(Logger::LogLevel::Debug, callback);
    Cracker cracker(logger);
    try
    {
        return cracker.Pack(parseKey(keys), zipFile, distFile, password);
    }
    catch (const std::exception& ex)
    {
        logger.Error(ex.what());
        return false;
    }
}
/// <summary>
/// ÐÞ¸´ÃÜÂë
/// </summary>
/// <param name="keys"></param>
/// <param name="length"></param>
/// <param name="rule"></param>
/// <param name="password">ÃÜÂë×Ö·û´®</param>
/// <param name="callback"></param>
/// <returns>ÃÜÂë×Ö·û´®µÄ³¤¶È</returns>
int __stdcall Recover(KeyStruct keys, size_t length, const char* rule, char* password, ProgressCallBack callback)
{
    ExportLogger logger(Logger::LogLevel::Debug, callback);
    Cracker cracker(logger);
    std::string res;
    try
    {
        bool success = cracker.RecoverPassword(parseKey(keys), length, rule, res);
        if (success) {
            logger.Info("password: %s", res.c_str());
            memcpy(password, res.c_str(), res.size());
            return res.size();
        }
    }
    catch (const std::exception& ex)
    {
        logger.Error(ex.what());
    }
    return 0;
}

bool __stdcall Unpack(KeyStruct keys, const char* cipherFile, const char* distFolder, ProgressCallBack callback)
{
    ExportLogger logger(Logger::LogLevel::Debug, callback);
    Cracker cracker(logger);
    try
    {
        return cracker.Unpack(parseKey(keys), cipherFile, distFolder);
    }
    catch (const std::exception& ex)
    {
        logger.Error(ex.what());
        return false;
    }
}

bool __stdcall Unpack2(KeyStruct keys, const char* cipherFile, long cipherBegin, long cipherEnd, const char* distFile, ProgressCallBack callback)
{
    ExportLogger logger(Logger::LogLevel::Debug, callback);
    Cracker cracker(logger);
    try
    {
        return cracker.Unpack(parseKey(keys), cipherFile, cipherBegin, cipherEnd, distFile);
    }
    catch (const std::exception& ex)
    {
        logger.Error(ex.what());
        return false;
    }
}

bool __stdcall Unpack3(KeyStruct keys, const char* cipherFile, const char* cipherFileName, const char* distFile, ProgressCallBack callback)
{
    ExportLogger logger(Logger::LogLevel::Debug, callback);
    Cracker cracker(logger);
    try
    {
        return cracker.Unpack(parseKey(keys), cipherFile, cipherFileName, distFile);
    }
    catch (const std::exception& ex)
    {
        logger.Error(ex.what());
        return false;
    }
}
