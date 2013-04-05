using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using SlavaGu.ConsoleAppLauncher;

namespace SlavaGu.NgenWrapper
{
    /// <summary>
    /// Ngen.exe (Native Image Generator) wrapper
    /// </summary>
    public class NgenLauncher : IDisposable
    {
        private IConsoleApp _ngenApp;
        
        private List<string> _assemblies;
        private int _processedAssemblyCount;

        /// <summary>
        /// Ngen wrapper constructor
        /// </summary>
        public NgenLauncher()
        {
            NgenExePath = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "ngen.exe");
        }

        /// <summary>
        /// Full path to ngen.exe, e.g. "C:\Windows\Microsoft.NET\Framework\v4.0.30319\ngen.exe"
        /// </summary>
        public string NgenExePath { get; set; }

        /// <summary>
        /// Ngen exit code
        /// </summary>
        public int? ExitCode { get; private set; }

        /// <summary>
        /// Ngen status
        /// </summary>
        public bool IsRunning
        {
            get { return _ngenApp != null && (_ngenApp.State == AppState.Running || _ngenApp.State == AppState.Exiting); }
        }

        /// <summary>
        /// Generate native images for an assembly and its dependencies and install the images in the native image cache.
        /// </summary>
        /// <param name="assembly">Path of the assembly or full display name, e.g. 
        /// "myAssembly, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0038abc9deabfle5".</param>
        /// <param name="exeConfig">Use the configuration of the specified executable assembly.</param>
        /// <param name="appBase">When locating dependencies, use the specified directory as the application base.</param>
        /// <param name="scenarios">Multiple images can be generated, depending on usage scenarios.</param>
        public void InstallAssembly(string assembly, string exeConfig = null, string appBase = null, NgenScenarios scenarios = NgenScenarios.Default)
        {
            InitNgenAction();

            var cmdLine = string.Format("install \"{0}\" /nologo", assembly);

            cmdLine += BuildExtraCmdLine(exeConfig, appBase, scenarios);

            BuildAssemblyList(assembly, appBase);

            RunNgen(cmdLine);
        }

        /// <summary>
        /// Delete the native images of an assembly and its dependencies from the native image cache.
        /// </summary>
        /// <param name="assembly">Path of the assembly or full display name, e.g. 
        /// "myAssembly, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0038abc9deabfle5".</param>
        /// <param name="exeConfig">Use the configuration of the specified executable assembly.</param>
        /// <param name="appBase">When locating dependencies, use the specified directory as the application base.</param>
        /// <param name="scenarios">Multiple images can be generated, depending on usage scenarios.</param>
        public void UninstallAssembly(string assembly, string exeConfig = null, string appBase = null, NgenScenarios scenarios = NgenScenarios.Default)
        {
            InitNgenAction();

            var cmdLine = string.Format("uninstall \"{0}\" /nologo /verbose", assembly);

            cmdLine += BuildExtraCmdLine(exeConfig, appBase, scenarios);

            BuildAssemblyList(assembly, appBase);

            RunNgen(cmdLine);
        }

        /// <summary>
        /// Display the state of the native images for an assembly and its dependencies.
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public void DisplayAssembly(string assemblyName)
        {
            InitNgenAction();

            var cmdLine = string.Format("display \"{0}\" /nologo", assemblyName);

            RunNgen(cmdLine);
        }

        /// <summary>
        /// Display the state of all native images.
        /// </summary>
        /// <returns></returns>
        public void DisplayAllAssemblies()
        {
            InitNgenAction();

            var cmdLine = string.Format("display /nologo");

            RunNgen(cmdLine);
        }

        /// <summary>
        /// Update native images that have become invalid.
        /// </summary>
        /// <returns></returns>
        public void UpdateAllAssemblies()
        {
            InitNgenAction();

            var cmdLine = string.Format("update /nologo");

            RunNgen(cmdLine);
        }

        /// <summary>
        /// Stop current ngen action.
        /// </summary>
        /// <param name="forceCloseMillisecondsTimeout"></param>
        public void Cancel(int forceCloseMillisecondsTimeout = Timeout.Infinite)
        {
            ThrowIfDisposed();

            if (_ngenApp != null)
            {
                _ngenApp.Stop(ConsoleSpecialKey.ControlBreak, forceCloseMillisecondsTimeout);
            }
        }

        /// <summary>
        /// Wait until ngen exits.
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <returns>True if succeeded or False if timeout elapsed</returns>
        public bool WaitForExit(int millisecondsTimeout = Timeout.Infinite)
        {
            ThrowIfDisposed();

            if (_ngenApp != null)
            {
                return _ngenApp.WaitForExit(millisecondsTimeout);
            }
            return true;
        }

        /// <summary>
        /// Fires when ngen outputs a new line
        /// </summary>
        public event EventHandler<NgenOutputEventArgs> Output;
        
        /// <summary>
        /// Fires when ngen starts processing local assembly during install or uninstall action.
        /// NOTE the order in which assemblies are processed depends on ngen and is non-deterministic.
        /// </summary>
        public event EventHandler<NgenProgressEventArgs> Progress;

        /// <summary>
        /// Fires when ngen exits
        /// </summary>
        public event EventHandler<NgenExitEventArgs> Exit;

        private static string BuildExtraCmdLine(string exeConfig, string appBase, NgenScenarios scenarios)
        {
            var cmdLine = string.Empty;

            if (exeConfig != null)
                cmdLine += string.Format(" /ExeConfig:\"{0}\"", exeConfig);

            if (appBase != null)
                cmdLine += string.Format(" /AppBase:\"{0}\"", appBase);

            if (scenarios.HasFlag(NgenScenarios.Debug))
                cmdLine += " /Debug";

            if (scenarios.HasFlag(NgenScenarios.Profile))
                cmdLine += " /Profile";

            if (scenarios.HasFlag(NgenScenarios.NoDependencies))
                cmdLine += " /NoDependencies";

            return cmdLine;
        }

        private void BuildAssemblyList(string assembly, string appBase)
        {
            if (File.Exists(assembly))
            {
                _assemblies = new List<string> { assembly };
                _assemblies.AddRange(AssemblyHelper.GetLocalReferences(assembly, appBase));
            }
        }

        private void OnConsoleOutput(object sender, ConsoleOutputEventArgs e)
        {
            try
            {
                OnOutput(new NgenOutputEventArgs(e.Line));
            }
            catch (Exception ex)
            {
                Trace.TraceError("OnOutput exception ignored: Error={0}", ex.ToString());
            }

            UpdateProgress(e.Line);
        }

        private static readonly Regex AssemblyRegex = new Regex(@"(assembly )(?<assembly>.*?)(,| \(CLR)", RegexOptions.Compiled);
        private static readonly AssemblyNameComparer AssemblyComparer = new AssemblyNameComparer();

        private void UpdateProgress(string outputLine)
        {
            // try to find what assembly is being processed now
            if (_assemblies != null && _assemblies.Count > 0)
            {
                var match = AssemblyRegex.Match(outputLine);
                if (match.Success)
                {
                    var assembly = match.Groups["assembly"].Value;
                    if (_assemblies.Contains(assembly, AssemblyComparer))
                    {
                        try
                        {
                            _processedAssemblyCount++;
                            var shortFileName = AssemblyHelper.GetShortAssemblyFileName(assembly);
                            OnProgress(new NgenProgressEventArgs(shortFileName, _processedAssemblyCount, _assemblies.Count));
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError("OnProgress exception ignored: Error={0}", ex.ToString());
                        }
                    }
                }
            }
        }

        private void OnConsoleExited(object sender, EventArgs e)
        {
            if (_ngenApp != null)
            {
                ExitCode = _ngenApp.ExitCode;
            }
            DisposeNgen();
            OnExit(new NgenExitEventArgs(ExitCode));
        }

        private void ThrowIfRunning()
        {
            if (_ngenApp != null)
                throw new InvalidOperationException("Previous ngen action has not finished.");
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("Object was disposed.");
        }

        private void InitNgenAction()
        {
            ThrowIfDisposed();
            ThrowIfRunning();

            ExitCode = null;
            _assemblies = null;
            _processedAssemblyCount = 0;
        }

        private void RunNgen(string cmdLine)
        {
            _ngenApp = new ConsoleApp(NgenExePath, cmdLine);
            _ngenApp.ConsoleOutput += OnConsoleOutput;
            _ngenApp.Exited += OnConsoleExited;
            _ngenApp.Run();
        }

        private void DisposeNgen()
        {
            if (_ngenApp != null)
            {
                _ngenApp.ConsoleOutput -= OnConsoleOutput;
                _ngenApp.Exited -= OnConsoleExited;
                _ngenApp.Dispose();
                _ngenApp = null;
            }
        }

        protected virtual void OnOutput(NgenOutputEventArgs e)
        {
            var handler = Output;
            if (handler != null)
                handler(this, e);
        }

        protected virtual void OnProgress(NgenProgressEventArgs e)
        {
            var handler = Progress;
            if (handler != null)
                handler(this, e);
        }

        protected virtual void OnExit(NgenExitEventArgs e)
        {
            var handler = Exit;
            if (handler != null)
                handler(this, e);
        }


        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Cancel(1000);
                    DisposeNgen();
                }

                _disposed = true;
            }
        }

        ~NgenLauncher()
        {
            Dispose(false);
        }

        #endregion
    }
}
