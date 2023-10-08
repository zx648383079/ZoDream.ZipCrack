# 介绍

已知加密ZIP中的一个文件，获取其他文件。

本程序参照 [kimci86/bkcrack](https://github.com/kimci86/bkcrack) 进行修改，主要使用 WPF 增加了可视化界面。~~UWP版就不做了~~

## 预览

![获取keys成功](screen/1.jpg)


## 文件介绍

|项目名|介绍|修改的内容|
|:--:|:--:|:--:|
|[Dll/CrackerExe](https://github.com/kimci86/bkcrack)|这是 c++ 控制台版，也使用动态链接库dll|使用vs2022进行编译改造|
|[Dll/Cracker](https://github.com/kimci86/bkcrack)|这是 c++ 版动态链接库，方便被c# 使用，c++ 与 c# 代码执行效率有很大差距，所以才有这个项目|增加了导出方法|
|src|这就是NET core WPF的界面||
|ZoDream.Shared|使用c# 重写了算法，两个版本，包含 c++ dll 的调用，及纯c#版||
|ZoDream.Tests|测试代码|


## 两个版本功能对比

|功能|c++ dll|c# dll|
|:----:|:---:|:---:|
|根据压缩中文件获取Key|√|√|
|根据文件获取Key|×|×|
|根据字符串获取Key|×|×|
|解压单个文件|√|√|
|解压全部文件|√|√|
|解压Deflated压缩的文件|×|√|
|更改密码|√|×|
|获取密码|√|√|

## 实现功能

1. 基于 `CRC32` 自动配对压缩文件
2. 实现获取 `internal keys`，  ~~同一个密码的不同压缩包Keys不通用？同一个压缩包同一个密码的keys是一样的~~
3. 基于 `internal keys` 解压全部文件
4. 支持 Stored, Deflated
5. 支持 c++ 版和 c# 版功能切换
6. c++ 版解压文件需要多一步解码单个文件

## 效率问题

`c++` 版的占用内存大概为 `40M`

`c#` 版的占用内存大概为 `400M` 


## 存在问题

1. 解压某些文件可能有问题，暂时没有解决，其他压缩编码待支持。。。
2. 不支持windows自带ZIP生成的文件
3. c# 版密码修复功能不可用，更改zip密码未实现
4. c++ 不支持直接解码Deflated


## 生成c++ exe

1. `属性` > `链接器` > `附加库目录`, 添加 dll 生成目录
2.  `属性` > `链接器` > `输入` > `附加依赖项`，添加 dll 的 `lib` 文件名 `cracker.lib`