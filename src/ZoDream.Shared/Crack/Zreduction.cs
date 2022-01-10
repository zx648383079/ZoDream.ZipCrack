using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Crack
{
    public class Zreduction
    {
        public const int WAIT_SIZE = 1 << 8;
        public const int TRACK_SIZE = 1 << 16;

        private readonly IList<byte> keyStream;

        private List<uint> ziVector = new List<uint>();

        private ICracker Contrainer;

        public List<uint> ZiVector
        {
            get { return ziVector; }
            set { ziVector = value; }
        }


        public int Index { get; set; } = 0;

        public int Count => ZiVector.Count;

        public Zreduction(ICracker cracker, IList<byte> keyS)
        {
            Contrainer = cracker;
            keyStream = keyS;
            Index = keyS.Count - 1;
            // ZiVector.Count = 1 << 22;
            var len = 1 << 22;
            for (uint i = 0; i < len; i++)
            {
                if (KeystreamTab.HasZi_2_16(keyStream[Index], i << 10))
                {
                    ZiVector.Add(i << 10);
                }
            }
        }

        public void Reduce()
        {
            // variables to keep track of the smallest Zi[2,32) vector
            var tracking = false;
            var bestCopy = new List<uint>();
            var bestIndex = Index;
            var bestSize = TRACK_SIZE;

            // variables to wait for a limited number of steps when a small enough vector is found
            var waiting = false;
            var wait = 0;

            var zim1_10_32_vector = new List<uint>();
            // zim1_10_32_vector.reserve(1 << 22);
            var zim1_10_32_set = new bool[1 << 22];

            for (var i = Index; i >= Attack.CONTIGUOUS_SIZE; i--)
            {
                zim1_10_32_vector.Clear();
                zim1_10_32_set = new bool[1 << 22];
                var number_of_zim1_2_32 = 0;

                // generate the Z{i-1}[10,32) values
                foreach (var zi_10_32 in ZiVector)
                    foreach (var zi_2_16 in KeystreamTab.GetZi_2_16_vector(keyStream[i], zi_10_32))
                    {
                        // get Z{i-1}[10,32) from CRC32^-1
                        var zim1_10_32 = Crc32Tab.GetZim1_10_32(zi_10_32 | zi_2_16);
                        // collect without duplicates only those that are compatible with keystream{i-1}
                        if (!zim1_10_32_set[zim1_10_32 >> 10] && KeystreamTab.HasZi_2_16(keyStream[i - 1], zim1_10_32))
                        {
                            zim1_10_32_vector.Add(zim1_10_32);
                            zim1_10_32_set[zim1_10_32 >> 10] = true;
                            number_of_zim1_2_32 += KeystreamTab.GetZi_2_16_vector(keyStream[i - 1], zim1_10_32).Count;
                        }
                    }

                // update smallest vector tracking
                if (number_of_zim1_2_32 <= bestSize) // new smallest number of Z[2,32) values
                {
                    tracking = true;
                    bestIndex = i - 1;
                    bestSize = number_of_zim1_2_32;
                    waiting = false;
                }
                else if (tracking) // number of Z{i-1}[2,32) values is bigger than bestSize
                {
                    if (bestIndex == i) // hit a minimum
                    {
                        // keep a copy of the vector because size is about to grow
                        Util.Swap(ref ziVector, ref bestCopy);

                        if (bestSize <= WAIT_SIZE)
                        {
                            // enable waiting
                            waiting = true;
                            wait = bestSize * 4; // arbitrary multiplicative constant
                        }
                    }

                    if (waiting && --wait == 0)
                        break;
                }
                if (Contrainer.Paused)
                {
                    return;
                }
                // put result in zi_vector
                Util.Swap(ref ziVector, ref zim1_10_32_vector);
                Contrainer.Logger?.Progress(keyStream.Count - i, keyStream.Count - Attack.CONTIGUOUS_SIZE);
                // std::cout << progress(keystream.size() - i, keystream.size() - Attack::CONTIGUOUS_SIZE) << std::flush << "\r";
            }

            // std::cout << std::endl;

            if (tracking)
            {
                // put bestCopy in zi_vector only if bestIndex is not the index of zi_vector
                if (bestIndex != Attack.CONTIGUOUS_SIZE - 1)
                {
                    Util.Swap(ref ziVector, ref bestCopy);
                }
                    
                Index = bestIndex;
            }
            else
            {
                Index = Attack.CONTIGUOUS_SIZE - 1;
            }
        }


        public void Generate()
        {
            var len = ZiVector.Count;
            for (int i = 0; i < len; i++)
            {
                var zi_2_16_vector = KeystreamTab.GetZi_2_16_vector(keyStream[Index], ZiVector[i]);
                for (int j = 1; j < zi_2_16_vector.Count; j++)
                {
                    ZiVector.Add(ZiVector[i] | zi_2_16_vector[j]);
                }
                ZiVector[i] |= zi_2_16_vector[0];
            }
        }
    }
}
