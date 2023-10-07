using System;
using System.Collections.Generic;
using System.Text;

namespace ZoDream.Shared.Models
{
    public class KeyItem
    {
        public uint X { get; set; }

        public uint Y { get; set; }

        public uint Z { get; set; }

        public override string ToString()
        {
            return string.Format("{0:x} {1:x} {2:x}", X, Y, Z);
        }

        public KeyItem()
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
            try
            {
                X = Convert.ToUInt32(x, 16);
                Y = Convert.ToUInt32(y, 16);
                Z = Convert.ToUInt32(z, 16);
            }
            catch { }
        }

        public KeyItem Clone()
        {
            return new KeyItem(X, Y, Z);
        }
    }
}
