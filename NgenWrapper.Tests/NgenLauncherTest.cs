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
        [Test]
        public void InstallAssemblyTest()
        {
            var ngenLauncher = new NgenLauncher();
            ngenLauncher.Progress += (sender, e) => Console.WriteLine("----> {0}/{1} {2} ", e.Current, e.Total,  e.Assembly);
            ngenLauncher.InstallAssembly(@"C:\Program Files\Cerebrata\AzureDiagnosticsManager\Cerebrata.AzureDiagnosticsManager.exe");
            ngenLauncher.WaitForExit();
            Assert.AreEqual(0, ngenLauncher.ExitCode);
        }

        [Test]
        public void UninstallAssemblyTest()
        {
            var ngenLauncher = new NgenLauncher();
            ngenLauncher.Progress += (sender, e) => Console.WriteLine("----> {0}/{1} {2} ", e.Current, e.Total,  e.Assembly);
            ngenLauncher.UninstallAssembly(@"C:\Program Files\Cerebrata\AzureDiagnosticsManager\Cerebrata.AzureDiagnosticsManager.exe");
            ngenLauncher.WaitForExit();
            Assert.AreEqual(0, ngenLauncher.ExitCode);
        }

        [Test]
        public void DisplayAssemblyTest()
        {
            var ngenLauncher = new NgenLauncher();
            ngenLauncher.DisplayAssembly(@"C:\Program Files\Cerebrata\AzureDiagnosticsManager\Cerebrata.AzureDiagnosticsManager.exe");
            ngenLauncher.WaitForExit();
            Assert.AreEqual(0, ngenLauncher.ExitCode);
        }
    }
}
