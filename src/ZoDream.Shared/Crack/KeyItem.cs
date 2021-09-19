using System;
using System.Collections.Generic;

namespace ZoDream.Shared.Crack
{
    public class KeyItem
    {
        public uint X { get; set; }

        public uint Y { get; set; }

        public uint Z { get; set; }


        public void Update(string password)
        {
            foreach (char item in password)
            {
                Update((byte)item);
            }
        }

        public void Update(byte b)
        {
            X = Crc32Tab.Crc32(X, b);
            Y = (Y + Util.Lsb(X)) * MultTab.MULT + 1;
            Z = Crc32Tab.Crc32(Z, Util.Msb(Y));
        }

        public void Update(IList<byte> cipherText, int current, int target)
        {
            for (int i = current - 1; i != target - 1; --i)
            {
                Update((byte)(cipherText[i] ^ KeystreamTab.GetByte(Z)));
            }
        }

        public void UpdateBackward(byte c)
        {
            Z = Crc32Tab.Crc32Inv(Z, Util.Msb(Y));
            Y = (Y - 1) * MultTab.MULTINV - Util.Lsb(X);
            X = Crc32Tab.Crc32Inv(X, (byte)(c ^ KeystreamTab.GetByte(Z)));
        }

        public void UpdateBackward(IList<byte> cipherText, int current, int target)
        {

            for (int i = current - 1; i != target - 1; --i)
            {
                UpdateBackward(cipherText[i]);
            }
        }

        public void UpdateBackwardPlaintext(byte p)
        {
            Z = Crc32Tab.Crc32Inv(Z, Util.Msb(Y));
            Y = (Y - 1) * MultTab.MULTINV - Util.Lsb(X);
            X = Crc32Tab.Crc32Inv(X, p);
        }



        public override string ToString()
        {
            return string.Format("{0:x} {1:x} {2:x}", X, Y, Z);
        }

        public KeyItem(): this(0x12345678, 0x23456789, 0x34567890)
        {
        }

        public KeyItem(uint x, uint y, uint z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public KeyItem(string x, string y, string z)
        {
            X = Convert.ToUInt32(x, 16);
            Y = Convert.ToUInt32(y, 16);
            Z = Convert.ToUInt32(z, 16);
        }

        public KeyItem Clone()
        {
            return new KeyItem(X, Y, Z);
        }
    }
}
