using ConsoleApp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;

namespace Tests
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var r = new ReflectionReader();
            Assert.IsTrue(r.CheckIfFileExists(Directory.GetCurrentDirectory() + @".\Tests.dll"));
            
        }
    }
}
