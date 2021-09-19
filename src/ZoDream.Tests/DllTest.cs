using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Tests
{
    [TestClass]
    public class DllTest
    {

        [TestMethod]
        public void TestFinKey()
        {
            var res = CrackerDLL.FindKey("c.zip", "c.txt", "plain.zip", "plain.txt");
            Assert.AreEqual(res.x, (uint)0x27e208d7);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct KeyItem
    {
        public uint x, y, z;
    }

    public static class CrackerDLL 
    {

        [DllImport("cracker.dll", EntryPoint = "FindKey", CallingConvention = CallingConvention.Cdecl)]
        internal static extern KeyItem FindKey(string zipFile, string zipFileName, string plainFile, string plainFileName);
    }
}
