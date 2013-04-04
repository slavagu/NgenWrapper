using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SlavaGu.NgenUI
{
    static class Program
    {
        public enum ExitCodes
        {
            Success = 0,
            InvalidArguments = 90001,
            FatalError = 90002,
        }

        public static string[] Args;
        public static int ExitCode = (int)ExitCodes.Success;

        // defines for commandline output
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int AttachParentProcess = -1;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            Args = args;

            if (args.Length < 2)
            {
                // redirect console output to parent process;
                // must be before any calls to Console.WriteLine()
                AttachConsole(AttachParentProcess);

                Console.WriteLine("");
                Console.WriteLine("NgenUI is a user interface for ngen.exe (CLR Native Image Generator)");
                Console.WriteLine("Usage: ngenui <ngen action> <assembly>, e.g. ngenui install MyAssembly.dll");
                Console.WriteLine("Supported ngen actions: install, uninstall, display");
                Console.WriteLine("To customize the interface edit NgenUI.exe.config");

                // sending the enter key is not really needed, but otherwise the user thinks the app is still running by looking at the commandline. 
                // The enter key takes care of displaying the prompt again.
                SendKeys.SendWait("{ENTER}");
                Environment.ExitCode = (int)ExitCodes.InvalidArguments;
                Application.Exit();
            }
            else
            {
                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm());
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Error: " + ex);
                    ExitCode = (int)ExitCodes.FatalError;
                }
            }

            Trace.TraceInformation("Done. ExitCode={0}", ExitCode);
            Environment.ExitCode = ExitCode;
            return ExitCode;
        }
    }
}
