using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ZoDream.Shared.CPlus
{
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyStruct
    {
        public uint x, y, z;
    }
}
