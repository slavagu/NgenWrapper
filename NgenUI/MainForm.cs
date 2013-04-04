using System;
using System.Configuration;
using System.Diagnostics;
using System.Windows.Forms;
using SlavaGu.NgenWrapper;

namespace SlavaGu.NgenUI
{
    public partial class MainForm : Form
    {
        private NgenLauncher _ngenLauncher;

        public MainForm()
        {
            LoadSettings();
            InitializeComponent();
        }

        public bool AllowCancellation { get; private set; }
        public string CancellationConfirmation { get; private set; }
        public string Label1 { get; private set; }
        public string Label2 { get; private set; }

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

        private void LoadSettings()
        {
            Label1 = GetAppSetting("Label1", "Processing...");
            Label2 = GetAppSetting("Label2", "@assemblyName");
            AllowCancellation = GetAppSetting("AllowCancellation", true);
            CancellationConfirmation = GetAppSetting("CancellationConfirmation", "Do you want to cancel the process?");
        }

        private void MainFormLoad(object sender, EventArgs e)
        {
            Text = GetAppSetting("WindowTitle", "Ngen UI");
            label1.Text = Label1;
            label2.Text = "";
            
            if (!AllowCancellation)
            {
                cancelButton.Enabled = false;
            }

            var ngenAction = Program.Args[0].ToLowerInvariant();
            var assembly = Program.Args[1];

            RunNgen(ngenAction, assembly);
        }

        private void RunNgen(string ngenAction, string assembly)
        {
            _ngenLauncher = new NgenLauncher();
            _ngenLauncher.Progress += OnNgenProgress;
            _ngenLauncher.Exit += OnNgenExit;

            switch (ngenAction)
            {
                case "install":
                    _ngenLauncher.InstallAssembly(assembly);
                    break;

                case "uninstall":
                    _ngenLauncher.UninstallAssembly(assembly);
                    break;

                case "display":
                    _ngenLauncher.DisplayAssembly(assembly);
                    break;

                default:
                    throw new ArgumentException("Ngen action not supported: " + ngenAction);
            }
        }

        private void OnNgenProgress(object sender, NgenProgressEventArgs e)
        {
            BeginInvoke((MethodInvoker)(() => UpdateProgress(e)));
        }

        private void OnNgenExit(object sender, NgenExitEventArgs e)
        {
            Program.ExitCode = e.ExitCode.GetValueOrDefault();
            _ngenLauncher.Dispose();
            _ngenLauncher = null;
            BeginInvoke((MethodInvoker)(Close));
        }

        private void UpdateProgress(NgenProgressEventArgs e)
        {
            label2.Text = Label2.Replace("@assemblyName", e.Assembly);

            progressBar.Minimum = 1;
            progressBar.Maximum = e.Total;
            progressBar.Step = 1;
            progressBar.PerformStep();
        }

        private void MainFormClosing(object sender, FormClosingEventArgs e)
        {
            if (_ngenLauncher == null)
                return;

            var answer = MessageBox.Show(CancellationConfirmation, Text, MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (answer == DialogResult.Yes && _ngenLauncher != null)
            {
                _ngenLauncher.Cancel();
            }
            e.Cancel = true;
        }

        private void CancelClicked(object sender, EventArgs e)
        {
            Close();
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
