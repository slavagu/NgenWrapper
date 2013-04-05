using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SlavaGu.NgenWrapper;

namespace NgenWrapper.Tests
{
    [TestFixture]
    public class NgenLauncherTest
    {
        const string AssemblyFile = @"DummyApplication.exe";

        [Test]
        public void display_and_uninstall_after_install_should_succeed()
        {
            var ngenLauncher = new NgenLauncher();

            // install
            var installedFiles = 0;
            ngenLauncher.Progress += (sender, e) =>
            {
                installedFiles++;
                Console.WriteLine("---> {0}/{1} {2} ", e.Current, e.Total, e.Assembly);
            };
            ngenLauncher.InstallAssembly(AssemblyFile);
            ngenLauncher.WaitForExit();
            Assert.AreEqual(0, ngenLauncher.ExitCode);
            Assert.AreEqual(3, installedFiles);

            // display
            ngenLauncher.DisplayAssembly(AssemblyFile);
            ngenLauncher.WaitForExit();
            Assert.AreEqual(0, ngenLauncher.ExitCode);

            // uninstall
            ngenLauncher = new NgenLauncher();
            var uninstalledFiles = 0;
            ngenLauncher.Progress += (sender, e) =>
            {
                uninstalledFiles++;
                Console.WriteLine("<--- {0}/{1} {2} ", e.Current, e.Total, e.Assembly);
            };
            ngenLauncher.UninstallAssembly(AssemblyFile);
            ngenLauncher.WaitForExit();
            Assert.AreEqual(0, ngenLauncher.ExitCode);
            Assert.AreEqual(3, uninstalledFiles);
        }

        [Test]
        public void display_and_uninstall_without_install_should_fail()
        {
            var ngenLauncher = new NgenLauncher();

            // display
            ngenLauncher.DisplayAssembly(AssemblyFile);
            ngenLauncher.WaitForExit();
            Assert.AreEqual(-1, ngenLauncher.ExitCode);

            // uninstall
            ngenLauncher.UninstallAssembly(AssemblyFile);
            ngenLauncher.WaitForExit();
            Assert.AreEqual(-1, ngenLauncher.ExitCode);
        }

    }
}
