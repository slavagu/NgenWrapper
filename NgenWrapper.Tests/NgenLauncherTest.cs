using System;
using NUnit.Framework;
using SlavaGu.NgenWrapper;

namespace NgenWrapper.Tests
{
    [TestFixture]
    public class NgenLauncherTest
    {
        [Test]
        public void install_single_dll_assembly_should_succeed()
        {
            using (var ngenLauncher = new NgenLauncher())
            {
                ngenLauncher.InstallAssembly("DummyAssembly2.dll");
                ngenLauncher.WaitForExit();
                Assert.IsTrue(ngenLauncher.IsSuccessful);

                // cleanup
                ngenLauncher.UninstallAssembly("DummyAssembly2.dll");
                ngenLauncher.WaitForExit();
                Assert.IsTrue(ngenLauncher.IsSuccessful);
            }
        }

        [Test]
        public void install_dll_with_dependent_assembly_should_succeed()
        {
            using (var ngenLauncher = new NgenLauncher())
            {
                // ensure both assemblies are not installed
                ngenLauncher.DisplayAssembly("DummyAssembly1.dll");
                ngenLauncher.WaitForExit();
                Assert.IsFalse(ngenLauncher.IsSuccessful);
                ngenLauncher.DisplayAssembly("DummyAssembly2.dll");
                ngenLauncher.WaitForExit();
                Assert.IsFalse(ngenLauncher.IsSuccessful);

                // install both
                ngenLauncher.InstallAssembly("DummyAssembly1.dll");
                ngenLauncher.WaitForExit();
                Assert.IsTrue(ngenLauncher.IsSuccessful);

                // cleanup
                ngenLauncher.UninstallAssembly("DummyAssembly1.dll");
                ngenLauncher.WaitForExit();
                Assert.IsTrue(ngenLauncher.IsSuccessful);
            }
        }

        [Test]
        public void install_invalid_assembly_should_fail()
        {
            using (var ngenLauncher = new NgenLauncher())
            {
                ngenLauncher.InstallAssembly("nosuchfile");
                ngenLauncher.WaitForExit();
                Assert.IsFalse(ngenLauncher.IsSuccessful);
            }
        }

        [Test]
        public void install_with_progress_should_fire_proper_callbacks()
        {
            using (var ngenLauncher = new NgenLauncher())
            {
                // install
                var processedFiles = 0;
                ngenLauncher.Progress += (sender, e) =>
                {
                    processedFiles++;
                    Console.WriteLine("---> {0}/{1} {2} ", e.Current, e.Total, e.Assembly);
                };
                ngenLauncher.InstallAssembly("DummyApplication.exe");
                ngenLauncher.WaitForExit();
                Assert.AreEqual(0, ngenLauncher.ExitCode);
                Assert.AreEqual(3, processedFiles);

                // display
                ngenLauncher.DisplayAssembly("DummyApplication.exe");
                ngenLauncher.WaitForExit();
                Assert.AreEqual(0, ngenLauncher.ExitCode);
            }

            // uninstall
            using (var ngenLauncher = new NgenLauncher())
            {
                var processedFiles = 0;
                ngenLauncher.Progress += (sender, e) =>
                {
                    processedFiles++;
                    Console.WriteLine("<--- {0}/{1} {2} ", e.Current, e.Total, e.Assembly);
                };
                ngenLauncher.UninstallAssembly("DummyApplication.exe");
                ngenLauncher.WaitForExit();
                Assert.AreEqual(0, ngenLauncher.ExitCode);
                Assert.AreEqual(3, processedFiles);
            }
        }
    }
}
