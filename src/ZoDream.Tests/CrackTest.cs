using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Text;
using ZoDream.Shared;

namespace ZoDream.Tests
{
    [TestClass]
    public class CrackTest
    {
        // [TestMethod]
        public void TestUnpack()
        {
            var cracker = new Cracker();
            var keys = new ZoDream.Shared.Crack.KeyItem("27e208d7", "d91e8bd7", "10ae5016");
            var res = cracker.Unpack(keys, "c.zip", "p.zip", "saveFolder");
            Assert.IsTrue(res);
        }
        // [TestMethod]
        public void TestUnpack2()
        {
            var cracker = new Cracker();
            var res = cracker.Unpack("c.file", "p.file");
            Assert.IsTrue(res);
        }

        // [TestMethod]
        public void TestStr()
        {
            var s = "��ʱһ����";
            Assert.AreEqual(s.Remove(1), "��");
        }
        // [TestMethod]
        public void TestStr2()
        {
            var s = "��ʱһ����";
            byte b = 90;
            Assert.AreEqual(s.Insert(0, Encoding.UTF8.GetString(new byte[] { b })), "��");
        }
    }
}