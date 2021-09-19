#include "pch.h"
#include <cstdint>
#include "Arguments.hpp"
#include "Data.hpp"
#include "Zreduction.hpp"
#include "Attack.hpp"


struct KeyItem
{
	std::uint32_t x, y, z;
};

extern "C" _declspec(dllexport) 
KeyItem FindKey(char* zipFile, char* zipFileName, char* plainFile, char* plainFileName)
{
    KeyItem res;
    res.x = 0;
    res.y = 0;
    res.z = 0;
	Arguments args;
	args.cipherarchive = zipFile;
	args.cipherfile = zipFileName;
	args.plainarchive = plainFile;
	args.plainfile = plainFileName;
    std::vector<Keys> keysvec;
    Data data;
    try
    {
        data.load(args);
    }
    catch (const BaseError& e)
    {
        return res;
    }
    Zreduction zr(data.keystream);
    if (data.keystream.size() > Attack::CONTIGUOUS_SIZE)
    {
        zr.reduce();
    }

    // generate Zi[2,32) values
    zr.generate();

    // iterate over remaining Zi[2,32) values
    const uint32* candidates = zr.data();


    const std::int32_t size = zr.size();
    std::int32_t done = 0;

    Attack attack(data, zr.getIndex(), keysvec);

    const bool canStop = !args.exhaustive;
    bool shouldStop = false;

    for (std::int32_t i = 0; i < size; ++i) // OpenMP 2.0 requires signed index variable
    {
        if (shouldStop)
            continue; // cannot break out of an OpenMP for loop

        attack.carryout(candidates[i]);
        {
            shouldStop = canStop && !keysvec.empty();
        }
    }

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