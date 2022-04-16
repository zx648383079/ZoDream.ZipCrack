// CrackerExe.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//

#include <iostream>
#include <bitset>
#include "VirtualTerminalSupport.hpp"
#include "Arguments.hpp"
#include "ConsoleLogger.hpp"
#include "Cracker.h"


const char* usage = R"_(usage: bkcrack [options]
Crack legacy zip encryption with Biham and Kocher's known plaintext attack.

Mandatory:
 -c cipherfile      File containing the ciphertext
 -p plainfile       File containing the known plaintext

    or

 -k X Y Z           Internal password representation as three 32-bits integers
                      in hexadecimal (requires -d, -U, or -r)

Optional:
 -C encryptedzip    Zip archive containing cipherfile

 -P plainzip        Zip archive containing plainfile
 -o offset          Known plaintext offset relative to ciphertext
                      without encryption header (may be negative)
 -t size            Maximum number of bytes of plaintext to read
 -x offset data     Additional plaintext in hexadecimal starting
                      at the given offset (may be negative)

 -e                 Exhaustively try all the keys remaining after Z reduction

 -d decipheredfile  File to write the deciphered text (requires -c)
 -U unlockedzip password
                    File to write the encryped zip with the password set
                      to the given new password (requires -C)

 -r length charset  Try to recover the password up to the given length using
                      characters in the given charset. The charset is a
                      sequence of characters or shorcuts for predefined
                      charsets listed below. Example: ?l?d-.@

                      ?l lowercase letters
                      ?u uppercase letters
                      ?d decimal digits
                      ?s punctuation
                      ?a alpha-numerical characters (same as ?l?u?d)
                      ?p printable characters (same as ?a?s)
                      ?b all bytes (0x00 - 0xff)

 -h                 Show this help and exit)_";

ConsoleLogger logger(Logger::LogLevel::Debug, std::cout);
int main(int argc, char const* argv[])
{
    // enable virtual terminal support on Windows, no-op on other platforms
    VirtualTerminalSupport vtSupport;

    // setup output stream
    // std::cout << setupLog << std::endl;
    ProgressCallBack callback = [](int progress, int total, const char* str)->bool {
        return logger.Callback(progress, total, str);
    };
    const Arguments args(argc, argv);
    if (args.help)
    {
        std::cout << usage << std::endl;
        return 0;
    }

    else
        // find keys from known plaintext
    {
        KeyStruct keys = FindKey(args.cipherarchive.c_str(), 
            args.cipherfile.c_str(), args.plainarchive.c_str(), 
            args.plainfile.c_str(), callback);
    }

    // From there, keysvec is not empty.

    // decipher
    if (!args.decipheredfile.empty())
    {
        Unpack3(args.keys, args.cipherarchive.c_str(), args.cipherfile.c_str(), 
            args.decipheredfile.c_str(), callback);
    }

    // unlock
    if (!args.unlockedarchive.empty())
    {
        Pack2(args.keys, args.cipherarchive.c_str(), args.unlockedarchive.c_str(), 
            args.newPassword.c_str(), callback);
    }

    // recover password
    if (args.maxLength)
    {
        char* password;
        Recover(args.keys,
            args.maxLength, args.charset.c_str(),
            password, callback);

    }

    return 0;
}
