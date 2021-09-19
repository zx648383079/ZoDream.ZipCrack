using System;
using System.Collections.Generic;
using System.Text;

namespace ZoDream.Shared.Crack
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
    }
}
