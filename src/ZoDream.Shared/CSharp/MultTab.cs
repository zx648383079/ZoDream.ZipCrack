using System;
using System.Collections.Generic;
using System.Text;

namespace ZoDream.Shared.CSharp
{
    public class MultTab
    {
        public const uint MULT = 0x08088405;
        public const uint MULTINV = 0xd94fa8cd;
        public static MultTab Instance = new MultTab();

        public uint[] Tab { get; private set; } = new uint[256];
        public uint[] InvTab { get; private set; } = new uint[256];

        public IList<byte>[] MsbProdfiber2 { get; set; } = new List<byte>[256];
        public IList<byte>[] MsbProdfiber3 { get; set; } = new List<byte>[256];

        public MultTab()
        {
            for (int i = 0; i < 256; i++)
            {
                MsbProdfiber2[i] = new List<byte>();
                MsbProdfiber3[i] = new List<byte>();
            }
            uint prodinv = 0;
            for (uint x = 0; x < 256; x++, prodinv += MULTINV)
            {
                InvTab[x] = prodinv;
                var bx = (byte)x;
                MsbProdfiber2[Util.Msb(prodinv)].Add(bx);
                MsbProdfiber2[(Util.Msb(prodinv) + 1) % 256].Add(bx);


                MsbProdfiber3[(Util.Msb(prodinv) + 255) % 256].Add(bx);
                MsbProdfiber3[Util.Msb(prodinv)].Add(bx);
                MsbProdfiber3[(Util.Msb(prodinv) + 1) % 256].Add(bx);
            }
        }

        public static uint GetMultInv(byte x)
        {
            return Instance.InvTab[x];
        }

        public static IList<byte> GetMsbProdFiber2(byte msbprodinv)
        {
            return Instance.MsbProdfiber2[msbprodinv];
        }

        public static IList<byte> GetMsbProdFiber3(byte msbprodinv)
        {
            return Instance.MsbProdfiber3[msbprodinv];
        }
    }
}
