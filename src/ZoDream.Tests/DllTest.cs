using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.CPlus;

namespace ZoDream.Tests
{
    [TestClass]
    public class DllTest
    {

        // [TestMethod]
        public void TestFinKey()
        {
            var res = CrackerNativeMethods.FindKey("c.zip", "c.txt", "plain.zip", "plain.txt", (p,t,m) =>
            {
                return true;
            });
            Assert.AreEqual(res.x, (uint)0x27e208d7);
        }
    }

}
