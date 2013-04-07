using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SlavaGu.NgenWrapper;

namespace NgenWrapper.Tests
{
    [TestFixture]
    public class AssemblyAnalyzerTest
    {
        [Test]
        public void GetLocalReferencesTest()
        {
            var expected = new List<string>
            {
                "DummyAssembly1.dll",
                "DummyAssembly2.dll",
            };

            var actual = AssemblyAnalyzer.GetLocalReferences("DummyApplication.exe");

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [Test]
        public void GetShortAssemblyFileNameTest()
        {
            Assert.AreEqual(null, AssemblyAnalyzer.GetShortAssemblyFileName(null));
            Assert.AreEqual("", AssemblyAnalyzer.GetShortAssemblyFileName(""));

            Assert.AreEqual(@"Assembly1", AssemblyAnalyzer.GetShortAssemblyFileName(@"Assembly1"));
            Assert.AreEqual(@"assembly1.dll", AssemblyAnalyzer.GetShortAssemblyFileName(@"assembly1.dll"));
            Assert.AreEqual(@"Assembly1.EXE", AssemblyAnalyzer.GetShortAssemblyFileName(@"Assembly1.EXE"));
            Assert.AreEqual(@"Assembly1.exe.config", AssemblyAnalyzer.GetShortAssemblyFileName(@"Assembly1.exe.config"));
            
            Assert.AreEqual(@"DummyApplication.exe", AssemblyAnalyzer.GetShortAssemblyFileName(@"DummyApplication"));
            Assert.AreEqual(@"DummyAssembly1.dll", AssemblyAnalyzer.GetShortAssemblyFileName(@"DummyAssembly1"));
        }

        [Test]
        public void FileNameComparerTest()
        {
            var comparer = new AssemblyAnalyzer.FileNameComparer();

            Assert.IsTrue(comparer.Equals(null, null));
            Assert.IsTrue(comparer.Equals("", ""));
            Assert.IsFalse(comparer.Equals(null, ""));
            Assert.IsFalse(comparer.Equals("", null));
            
            Assert.IsTrue(comparer.Equals(@"assembly1", "assembly1"));
            Assert.IsTrue(comparer.Equals(@"assembly1.exe", "ASSEMBLY1.EXE"));
            Assert.IsTrue(comparer.Equals(@"Assembly1.dll", "assembly1"));
            Assert.IsTrue(comparer.Equals(@"Namespace.Assembly1.dll", "Namespace.assembly1"));
            Assert.IsTrue(comparer.Equals(@"Folder Name\\Namespace.Assembly1.dll", "folder name\\Namespace.assembly1"));
            Assert.IsTrue(comparer.Equals(@"Folder Name\\Namespace.Assembly1.dll", "Namespace.assembly1"));

            Assert.IsFalse(comparer.Equals(@"Assembly1", "assembly2"));
            Assert.IsFalse(comparer.Equals(@"Assembly1.exe", "Assembly1.dll"));
            Assert.IsFalse(comparer.Equals(@"Namespace.Assembly1.dll", "Namespace.assembly2"));
        }
    }
}
