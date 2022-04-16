using System;
using System.Collections.Generic;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.CSharp
{
    public class Keys: KeyItem
    {

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

        public Keys() : base(0x12345678, 0x23456789, 0x34567890)
        {
        }

        public Keys(KeyItem key): base(key.X, key.Y, key.Z)
        {
            
        }

        public Keys(uint x, uint y, uint z): base(x, y, z)
        {
        }

        public Keys(string x, string y, string z): base(x, y, z)
        {
        }

    }
}
