using NUnit.Framework;
using SlavaGu.NgenWrapper;

namespace NgenWrapper.Tests
{
    [TestFixture]
    public class NgenTest
    {
        [Test]
        public void InstallWithFilenameTest()
        {
            Assert.IsFalse(Ngen.IsAssemblyInstalled("DummyApplication.exe"));
            Assert.IsTrue(Ngen.InstallAssembly("DummyApplication.exe"));
            Assert.IsTrue(Ngen.IsAssemblyInstalled("DummyApplication.exe"));
            Assert.IsTrue(Ngen.UninstallAssembly("DummyApplication.exe"));
            Assert.IsFalse(Ngen.IsAssemblyInstalled("DummyApplication.exe"));
        }

        [Test]
        public void InstallWithAssemblyNameTest()
        {
            Assert.IsFalse(Ngen.IsAssemblyInstalled("DummyApplication, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"));
            Assert.IsTrue(Ngen.InstallAssembly("DummyApplication.exe"));
            Assert.IsTrue(Ngen.IsAssemblyInstalled("DummyApplication, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"));
            Assert.IsTrue(Ngen.UninstallAssembly("DummyApplication, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"));
            Assert.IsFalse(Ngen.IsAssemblyInstalled("DummyApplication, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"));
        }

    }
}
