# Introduce

Known to encrypt one file in a ZIP, get other files.

This program is modified with reference to [kimci86/bkcrack](https://github.com/kimci86/bkcrack), mainly using WPF to add a visual interface. ~~UWP version will not do it~~

👉[中文](README.zh.md)

## Preview

![Get keys successfully](screen/1.jpg)


## File introduction

|Project Name|Introduction|Modified Content|
|:--:|:--:|:--:|
|[Dll/CrackerExe](https://github.com/kimci86/bkcrack)|This is the c++ console version, which also uses the dynamic link library dll | use vs2022 to compile and transform|
|[Dll/Cracker](https://github.com/kimci86/bkcrack)|This is a C++ version of the dynamic link library, which is easy to use by c#. There is a big gap between the code execution efficiency of c++ and c#, so there is this project | many export methods have been added.|
|src|This is the interface of NET core WPF||
|ZoDream.Shared|The algorithm is rewritten using c#, two versions, including the call of c++ dll, and the pure c# version||
|ZoDream.Tests|test code|


## Functional comparison of the two versions

|Features|c++ dll|c# dll|
|:----:|:---:|:---:|
|Get the key from the compressed file|√|√|
|Get the key from the file|×|×|
|Get Key from string|×|×|
|Unzip a single file|√|√|
|Unzip all files|√|√|
|Unzip the Deflated compressed file|×|√|
|change the password|√|×|
|get password|√|√|

## implement function

1. Automatically pair compressed files based on `CRC32`
2. Realize the acquisition of `internal keys`, ~~Different compressed package Keys of the same password are not common? The keys of the same compressed package and the same password are the same~~
3. Extract all files based on `internal keys`
4. Support Stored, Deflated
5. Support c++ version and c# version function switch
6. C++ version decompressing files requires one more step to decode a single file

## Efficiency issues

The `c++` console version occupies about `40M` of memory

The occupied memory of the `c++` dll version is about `140M`

The memory footprint of the `c#` version is about `400M`


## There is a problem

1. There may be problems with decompressing some files, but it has not been resolved for the time being, and other compression codes are to be supported. . 
2. The files generated by Windows' own ZIP are not supported.
3. C# version change zip password not implemented
4. c++ does not support direct decoding of Deflated


## generate c++ exe

1. `Properties` > `Linker` > `Additional library directories`, add the dll build directory
2.  `Properties` > `Linker` > `Inputs` > `Additional Dependencies`, add the `lib` filename of the dll `cracker.lib`