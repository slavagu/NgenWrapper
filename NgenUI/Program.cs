using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SlavaGu.NgenUI
{
    static class Program
    {
        public static string[] Args;
        public static int ExitCode = 0;

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

            if (args.Length == 0 || args[0] == "/?")
            {
                // redirect console output to parent process;
                // must be before any calls to Console.WriteLine()
                AttachConsole(AttachParentProcess);

                Console.WriteLine("");
                Console.WriteLine("NgenUI is a user interface for ngen.exe (CLR Native Image Generator)");
                Console.WriteLine("Usage: ngenui <ngen command line>, e.g. ngenui install MyAssembly.dll");
                Console.WriteLine("To customize the interface edit NgenUI.exe.config");

                // sending the enter key is not really needed, but otherwise the user thinks the app is still running by looking at the commandline. 
                // The enter key takes care of displaying the prompt again.
                SendKeys.SendWait("{ENTER}");
                Application.Exit();
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            
            return ExitCode;
        }
    }
}
