using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ZoDream.Shared.CSharp
{
    public class Attack
    {
        public const int SIZE = 12;
        public const int CONTIGUOUS_SIZE = 8;

        private readonly CrackData Data;
        private readonly int Index;
        public IList<Keys> SolutionItems = new List<Keys>();

        private uint[] ZItems = new uint[CONTIGUOUS_SIZE];
        private uint[] YItems = new uint[CONTIGUOUS_SIZE];
        private uint[] XItems = new uint[CONTIGUOUS_SIZE];

        public Attack(CrackData data, int index)
        {
            Data = data;
            Index = index + 1 - CONTIGUOUS_SIZE;
        }

        public void Carryout(uint z7_2_32)
        {
            ZItems[7] = z7_2_32;
            ExploreZlists(7);
        }

        public void ExploreZlists(int i)
        {
            if (i != 0) // the Z-list is not complete so generate Z{i-1}[2,32) values
            {
                // get Z{i-1}[10,32) from CRC32^-1
                var zim1_10_32 = Crc32Tab.GetZim1_10_32(ZItems[i]);

                // get Z{i-1}[2,16) values from keystream byte k{i-1} and Z{i-1}[10,16)
                var zim1_items = KeystreamTab.GetZi_2_16_vector(Data.KeyStream[Index + i - 1], zim1_10_32);
                foreach (var zim1_2_16 in zim1_items)
                {
                    // add Z{i-1}[2,32) to the Z-list
                    ZItems[i - 1] = zim1_10_32 | zim1_2_16;

                    // find Zi[0,2) from CRC32^1
                    ZItems[i] &= Util.MASK_2_32; // discard 2 least significant bits
                    ZItems[i] |= (Crc32Tab.Crc32Inv(ZItems[i], 0) ^ ZItems[i - 1]) >> 8;

                    // get Y{i+1}[24,32)
                    if (i < 7)
                    {
                        YItems[i + 1] = Crc32Tab.GetYi_24_32(ZItems[i + 1], ZItems[i]);
                    }
                    ExploreZlists(i - 1);
                }
            }
            else // the Z-list is complete so iterate over possible Y values
            {
                // guess Y7[8,24) and keep prod == (Y7[8,32) - 1) * mult^-1
                for (uint y7_8_24 = 0, prod = (MultTab.GetMultInv(Util.Msb(YItems[7])) << 24) - MultTab.MULTINV;
                    y7_8_24 < 1 << 24;
                    y7_8_24 += 1 << 8, prod += MultTab.MULTINV << 8)
                    // get possible Y7[0,8) values
                    foreach (var y7_0_8 in MultTab.GetMsbProdFiber3((byte)(Util.Msb(YItems[6]) - Util.Msb(prod))))
                        // filter Y7[0,8) using Y6[24,32)
                        if (prod + MultTab.GetMultInv(y7_0_8) - (YItems[6] & Util.MASK_24_32) <= Util.MAXDIFF_0_24)
                        {
                            YItems[7] = y7_0_8 | y7_8_24 | YItems[7] & Util.MASK_24_32;
                            ExploreYlists(7);
                        }
            }
        }

        public void ExploreYlists(int i)
        {
            if (i != 3) // the Y-list is not complete so generate Y{i-1} values
            {
                var fy = (YItems[i] - 1) * MultTab.MULTINV;
                var ffy = (fy - 1) * MultTab.MULTINV;

                // get possible LSB(Xi)
                foreach (var xi_0_8 in MultTab.GetMsbProdFiber2(Util.Msb(ffy - (YItems[i - 2] & Util.MASK_24_32))))
                {
                    // compute corresponding Y{i-1}
                    uint yim1 = fy - xi_0_8;

                    // filter values with Y{i-2}[24,32)
                    if (ffy - MultTab.GetMultInv(xi_0_8) - (YItems[i - 2] & Util.MASK_24_32) <= Util.MAXDIFF_0_24
                        && Util.Msb(yim1) == Util.Msb(YItems[i - 1]))
                    {
                        // add Y{i-1} to the Y-list
                        YItems[i - 1] = yim1;

                        // set Xi value
                        XItems[i] = xi_0_8;

                        ExploreYlists(i - 1);
                    }
                }
            }
            else // the Y-list is complete so check if the corresponding X-list is valid
            {
                TestXlist();
            }
        }

        public void TestXlist()
        {
            // compute X7
            for (int i = 5; i <= 7; i++)
            {
                XItems[i] = Crc32Tab.Crc32(XItems[i - 1], Data.PlainText[Index + i - 1])
                            & Util.MASK_8_32 // discard the LSB
                            | Util.Lsb(XItems[i]); // set the LSB
            }


            // compute X3
            var x = XItems[7];
            for (int i = 6; i >= 3; i--)
            {
                x = Crc32Tab.Crc32Inv(x, Data.PlainText[Index + i]);
            }

            // check that X3 fits with Y1[26,32)
            var y1_26_32 = Crc32Tab.GetYi_24_32(ZItems[1], ZItems[0]) & Util.MASK_26_32;
            if (((YItems[3] - 1) * MultTab.MULTINV - Util.Lsb(x) - 1) * MultTab.MULTINV - y1_26_32 > Util.MAXDIFF_0_26)
            {
                return;
            }

            // decipher and filter by comparing with remaining contiguous plaintext forward
            var keysForward = new Keys(XItems[7], YItems[7], ZItems[7]);
            keysForward.Update(Data.PlainText[Index + 7]);

            var p = Index + 8;
            var c = Data.Offset + Index + 8;
            for (; p != Data.PlainText.Count; ++p, ++c)
            {
                if ((Data.CipherText[c] ^ KeystreamTab.GetByte(keysForward.Z)) != Data.PlainText[p])
                {
                    return;
                }
                keysForward.Update(Data.PlainText[p]);
            }

            var indexForward = Data.Offset + Data.PlainText.Count;

            // and also backward
            var keysBackward = new Keys(x, YItems[3], ZItems[3]);
            var p1 = Index + 2;
            var c1 = Data.Offset + Index + 2;
            for (; p1 != -1; --p1, --c1)
            {
                var ct1 = Data.CipherText[c1];
                var pt1 = Data.PlainText[p1];
                keysBackward.UpdateBackward(ct1);
                if ((ct1 ^ KeystreamTab.GetByte(keysBackward.Z)) != pt1)
                {
                    return;
                }
            }

            var indexBackward = Data.Offset;

            // continue filtering with extra known plaintext
            foreach (var extra in Data.ExtraPlainText)
            {
                byte p3;
                if (extra.Item1 < indexBackward)
                {
                    keysBackward.UpdateBackward(Data.CipherText, indexBackward, extra.Item1);

                    indexBackward = extra.Item1;
                    p3 = (byte)(Data.CipherText[indexBackward] ^ KeystreamTab.GetByte(keysBackward.Z));
                }
                else
                {
                    keysForward.Update(Data.CipherText, indexForward, extra.Item1);
                    indexForward = extra.Item1;
                    p3 = (byte)(Data.CipherText[indexForward] ^ KeystreamTab.GetByte(keysForward.Z));
                }

                if (p != extra.Item2)
                {
                    return;
                }
            }

            // all tests passed so the keys are found

            // get the keys associated with the initial state
            keysBackward.UpdateBackward(Data.CipherText, indexBackward, 0);
            SolutionItems.Add(keysBackward);
        }
    }
}
