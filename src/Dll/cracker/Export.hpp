#pragma once
#include <cstdint>
#include "Logger.hpp"
#include "types.hpp"

struct KeyStruct
{
	std::uint32_t x, y, z;
};

/// <summary>
/// 返回 false 是取消, true 是继续
/// </summary>
typedef bool (*ProgressCallBack)(int progress, int total, const char* str);

class ExportLogger : public Logger
{
public:
    ExportLogger(LogLevel level, ProgressCallBack callback);

    void Log(LogLevel level, const char* message);

    void Progress(long current, long total);
private:
    ProgressCallBack progressCallback;
};

KeyStruct parseKey(std::vector<Keys> keysvec);
Keys parseKey(KeyStruct keys);

#ifdef __cplusplus
extern "C" {
#endif // __cplusplus
    _declspec(dllexport) KeyStruct __stdcall FindKey(const char* zipFile, const char* zipFileName, const char* plainFile, const char* plainFileName, ProgressCallBack callback);

    _declspec(dllexport) KeyStruct __stdcall FindKey2(const char* zipFile, long cipherBegin, long cipherEnd, const char* plainFile, long plainBegin, long plainEnd, ProgressCallBack callback);

    _declspec(dllexport) KeyStruct __stdcall FindKey3(const char* zipFile, const char* zipFileName, const char* plainFile, ProgressCallBack callback);

    _declspec(dllexport) KeyStruct __stdcall FindKey4(const char* zipFile, const char* zipFileName, const byte* plainData, int plainLength, ProgressCallBack callback);

    _declspec(dllexport) KeyStruct __stdcall FindKey5(const char* zipFile, long cipherBegin, long cipherEnd, const byte* plainData, int plainLength, ProgressCallBack callback);

    _declspec(dllexport) KeyStruct __stdcall FindKey6(const byte* zipData, int zipLength, const byte* plainData, int plainLength, ProgressCallBack callback);

    _declspec(dllexport) bool __stdcall Pack(KeyStruct keys, const char* zipFile, const char* distFile, ProgressCallBack callback);

    _declspec(dllexport) bool __stdcall Pack2(KeyStruct keys, const char* zipFile, const char* distFile, const char* password, ProgressCallBack callback);

    _declspec(dllexport) int __stdcall Recover(KeyStruct keys, size_t length, const char* rule, char* password, ProgressCallBack callback);

    _declspec(dllexport) bool __stdcall Unpack(KeyStruct keys, const char* cipherFile, const char* distFolder, ProgressCallBack callback);

    _declspec(dllexport) bool __stdcall Unpack2(KeyStruct keys, const char* cipherFile, long cipherBegin, long cipherEnd, const char* distFile, ProgressCallBack callback);

    _declspec(dllexport) bool __stdcall Unpack3(KeyStruct keys, const char* cipherFile, const char* cipherFileName, const char* distFile, ProgressCallBack callback);

#ifdef __cplusplus
}
#endif // __cplusplus