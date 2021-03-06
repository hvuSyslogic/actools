﻿using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using AcTools.AcdFile;
using AcTools.DataFile;
using NUnit.Framework;

namespace AcTools.Tests {
    [TestFixture]
    public class DataWrapperTest {
        private static string GetTestDir([CallerFilePath] string callerFilePath = null) => Path.Combine(Path.GetDirectoryName(callerFilePath) ?? "", "test");

        private static string TestDir => GetTestDir();

        [Test]
        public void TestPacked() {
            var file = DataWrapper.FromCarDirectory(Path.Combine(TestDir, "data", "peugeot_504"));
            Assert.AreEqual("VALID_INI_FILE", file.GetRawFile("mirrors.ini").Content);
            Assert.AreEqual("VALID_LUT_FILE", file.GetRawFile("power.lut").Content);

            file = DataWrapper.FromCarDirectory(Path.Combine(TestDir, "data", "peugeot_504_unpacked"));
            Assert.AreEqual("VALID_INI_FILE", file.GetRawFile("mirrors.ini").Content);
            Assert.AreEqual("VALID_LUT_FILE", file.GetRawFile("power.lut").Content);
        }

        [Test]
        public void TestEnc() {
            var enc = AcdEncryption.FromAcdFilename("anything/actually");
            var bytes = Encoding.UTF8.GetBytes("Long testing string with русскими символами and emojis like 😺");
            var cloned = bytes.ToArray();

            //enc.Encrypt(cloned);
            //enc.Decrypt(cloned);
            // TODO

            Assert.IsTrue(bytes.SequenceEqual(cloned));
        }
    }
}