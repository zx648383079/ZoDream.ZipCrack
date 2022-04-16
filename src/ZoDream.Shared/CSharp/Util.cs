using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ZoDream.Shared.CSharp
{
    public static class Util
    {
        public const uint MASK_0_16 = 0x0000ffff;
        public const uint MASK_0_24 = 0x00ffffff;
        public const uint MASK_0_26 = 0x03ffffff;
        public const uint MASK_0_32 = 0xffffffff;
        public const uint MASK_26_32 = 0xfc000000;
        public const uint MASK_24_32 = 0xff000000;
        public const uint MASK_10_32 = 0xfffffc00;
        public const uint MASK_8_32 = 0xffffff00;
        public const uint MASK_2_32 = 0xfffffffc;
        public const uint MAXDIFF_0_24 = MASK_0_24 + 0xff;
        public const uint MAXDIFF_0_26 = MASK_0_26 + 0xff;
        public static byte Lsb(uint x)
        {
            return (byte)x;
        }

        public static byte Msb(uint x)
        {
            return (byte)(x >> 24);
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T t = a;
            a = b;
            b = t;
        }

        /// <summary>
        /// 获取文件的Crc32
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string Crc32(string file)
        {
            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
            {
                return string.Empty;
            }
            using (var fs = File.OpenRead(file))
            {
                return Crc32(fs);
            }
        }

        /// <summary>
        /// 获取文件流的Crc32
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string Crc32(Stream stream)
        {
            uint res = 0;
            while (true)
            {
                var b = stream.ReadByte();
                if (b == -1)
                {
                    break;
                }
                res = Crc32Tab.Crc32(res, (byte)b);
            }
            return res.ToString("X");
        }
    }
}
