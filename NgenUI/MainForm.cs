using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SlavaGu.ConsoleAppLauncher;

namespace SlavaGu.NgenUI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            AllowCancellation = GetAppSetting("AllowCancellation", true);
            Label1 = GetAppSetting("Label1", "Processing...");
            Label2 = GetAppSetting("Label2", "@assemblyName");

            InitializeComponent();
        }

        public bool AllowCancellation { get; private set; }
        public string Label1 { get; private set; }
        public string Label2 { get; private set; }

        private void MainFormLoad(object sender, EventArgs e)
        {
            Text = GetAppSetting("WindowTitle", "Ngen UI");
            label1.Text = "Loading...";
            label2.Text = "";
            
            if (!AllowCancellation)
            {
                cancelButton.Enabled = false;
            }

            var programName = Environment.GetCommandLineArgs()[0];
            var cmdLine = Environment.CommandLine.Replace("\"" + programName + "\"", "").Replace(programName, "").Trim();

            var ngenAction = Program.Args[0];
            switch (ngenAction)
            {
                case "install":
                case "uninstall":
                case "display":
                    var assemblyName = Program.Args[1];
                    label1.Text = Label1.Replace("@assemblyName", assemblyName);
                    RunNgenWithProgress(assemblyName, cmdLine);
                    break;

                default:
                    RunNgen(cmdLine);
                    label1.Text = Label1.Replace("@assemblyName", "assembly");
                    break;
            }
        }

        private void RunNgen(string cmdLine)
        {
            progressBar.Style = ProgressBarStyle.Marquee;

            var ngenExePath = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "ngen.exe");
            var app = new ConsoleApp(ngenExePath, cmdLine);
            app.Exited += (sender, args) => BeginInvoke((MethodInvoker)(Close));
            app.Run();
        }

        private void RunNgenWithProgress(string assemblyFile, string cmdLine)
        {
            var refs = GetReferences(assemblyFile);
            progressBar.Minimum = 0;
            progressBar.Maximum = refs.Count() + 1;
            progressBar.Step = 1;

            var regex = new Regex(@"(assembly )(?<assembly>.*?)(,| \(CLR)", RegexOptions.Compiled);
            var ngenExePath = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "ngen.exe");
            var app = new ConsoleApp(ngenExePath, cmdLine);
            app.ConsoleOutput += (o, args) =>
            {
                var match = regex.Match(args.Line);
                if (match.Success)
                {
                    var assembly = match.Groups["assembly"].Value;
                    assembly = Path.GetFileName(assembly);
                    BeginInvoke((MethodInvoker) (() => UpdateProgress(assembly)));
                }
            };
            app.Exited += (sender, args) => BeginInvoke((MethodInvoker)(Close));
            app.Run();
        }

        private void UpdateProgress(string assembly)
        {
            label2.Text = Label2.Replace("@assemblyName", assembly);
            progressBar.PerformStep();
        }

        private static IEnumerable<string> GetReferences(string assemblyFile)
        {
	        var res = GetLocalReferencedAssemblies(assemblyFile)
		        .Distinct()
		        .OrderBy(a => a)
		        .ToArray();
            return res;
        }

        private static IEnumerable<string> GetLocalReferencedAssemblies(string assemblyFile)
        {
            var assemblyFolder = Path.GetDirectoryName(assemblyFile);
            var assembly = Assembly.ReflectionOnlyLoadFrom(assemblyFile);

            foreach (AssemblyName assemblyName in assembly.GetReferencedAssemblies())
            {
                var referencedAssemblyFile = Path.Combine(assemblyFolder, assemblyName.Name) + ".dll";
                if (File.Exists(referencedAssemblyFile))
                {
                    yield return referencedAssemblyFile;
                    foreach (var a in GetLocalReferencedAssemblies(referencedAssemblyFile))
                    {
                        yield return a;
                    }
                }
            }
        }

        private void MainFormClosing(object sender, FormClosingEventArgs e)
        {
            Program.ExitCode = 0;
        }

        private void CancelClicked(object sender, EventArgs e)
        {
            Close();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var myCp = base.CreateParams;
                if (!AllowCancellation)
                {
                    // disable close button
                    myCp.ClassStyle |= 0x200;
                }
                return myCp;
            }
        } 

        private static T GetAppSetting<T>(string key, T defaultValue)
        {
            var value = ConfigurationManager.AppSettings[key];
            if (value != null)
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            return defaultValue;
        }

    }
}
