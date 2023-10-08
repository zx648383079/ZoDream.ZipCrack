using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Text;
using ZoDream.Shared;
using ZoDream.Shared.CSharp;
using ZoDream.Shared.Models;

namespace ZoDream.Tests
{
    [TestClass]
    public class CrackTest
    {
        // [TestMethod]
        public void TestUnpack()
        {
            var cracker = new Cracker();
            var keys = new KeyItem("27e208d7", "d91e8bd7", "10ae5016");
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
            var s = "我时一个并";
            Assert.AreEqual(s.Remove(1), "我");
        }
        // [TestMethod]
        public void TestStr2()
        {
            var s = "我时一个并";
            byte b = 90;
            Assert.AreEqual(s.Insert(0, Encoding.UTF8.GetString(new byte[] { b })), "我");
        }

        [TestMethod]
        public void TestDeflate()
        {
            // Zip.DeflateFile("D:\\Desktop\\sqllifang.sql", "D:\\Desktop\\test.zip");
            Assert.AreEqual(1, 1);
        }

        [TestMethod]
        public void TestPassword()
        {
            var rule = new PasswordRule("?l?d");
            var charset = rule.Charset;
            Assert.AreEqual(charset.Count, 36);
        }
    }
}