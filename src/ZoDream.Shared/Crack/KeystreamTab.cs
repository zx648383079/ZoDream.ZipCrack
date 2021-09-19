using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ZoDream.Shared.Crack
{
    public class KeystreamTab
    {
        public static KeystreamTab Instance = new KeystreamTab();

        public byte[] Tab { get; set; } = new byte[1 << 14];

        public uint[,] InvTab { get; set; } = new uint[256, 64];

        public IList<uint>[,] InvFilterTab { get; set; } = new List<uint>[256, 64];
        public bool[,] InvExists { get; set; } = new bool[256, 64];

        public KeystreamTab()
        {   var next = new int[256];
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    InvFilterTab[i, j] = new List<uint>();
                }
            }
            for (uint z_2_16 = 0; z_2_16 < 1<<16; z_2_16 += 4)
            {
                byte k = Util.Lsb((z_2_16 | 2) * (z_2_16 | 3) >> 8);
                Tab[z_2_16 >> 2] = k;
                InvTab[k, next[k]++] = z_2_16;
                InvFilterTab[k, z_2_16 >> 10].Add(z_2_16);
                InvExists[k, z_2_16 >> 10] = true;
            }
        }

        public static byte GetByte(uint zi)
        {
            return Instance.Tab[(zi & Util.MASK_0_16) >> 2];
        }

        public static uint[] GetZi_2_16_array(byte ki)
        {
            var items = new uint[64];
            for (int i = 0; i < 64; i++)
            {
                items[i] = Instance.InvTab[ki, i];
            }
            return items;
        }

        public static IList<uint> GetZi_2_16_vector(byte ki, uint zi_10_16)
        {
            return Instance.InvFilterTab[ki, (zi_10_16 & Util.MASK_0_16) >> 10];
        }

        public static bool HasZi_2_16(byte ki, uint zi_10_16)
        {
            return Instance.InvExists[ki, (zi_10_16 & Util.MASK_0_16) >> 10];
        }

    }
}
