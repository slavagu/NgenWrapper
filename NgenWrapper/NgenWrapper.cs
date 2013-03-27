using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using SlavaGu.ConsoleAppLauncher;

namespace SlavaGu.NgenWrapper
{
    /// <summary>
    /// Ngen.exe (Native Image Generator) wrapper
    /// </summary>
    public class NgenWrapper : IDisposable
    {
        private IConsoleApp _ngen;
        private readonly List<string> _errors = new List<string>();

        public NgenWrapper()
        {
            NgenExePath = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "ngen.exe");
        }

        /// <summary>
        /// Full path to ngen.exe, e.g. C:\Windows\Microsoft.NET\Framework\v4.0.30319\ngen.exe
        /// </summary>
        public string NgenExePath { get; set; }

        /// <summary>
        /// Ngen last exit code
        /// </summary>
        public int? ExitCode { get; private set; }

        /// <summary>
        /// Ngen error message list
        /// </summary>
        public IEnumerable<string> Errors 
        {
            get { return _errors.AsReadOnly(); }
        }

        /// <summary>
        /// Ngen status
        /// </summary>
        public bool IsRunning
        {
            get { return _ngen != null && (_ngen.State == AppState.Running || _ngen.State == AppState.Exiting); }
        }

        /// <summary>
        /// Generate native images for an assembly and its dependencies and install the images in the native image cache.
        /// </summary>
        /// <param name="assembly">Path of the assembly or full display name, e.g. 
        /// "myAssembly, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0038abc9deabfle5".</param>
        /// <param name="scenarios">Multiple images can be generated, depending on usage scenarios.</param>
        /// <param name="exeConfig">Use the configuration of the specified executable assembly.</param>
        /// <param name="appBase">When locating dependencies, use the specified directory as the application base.</param>
        public void InstallAssembly(string assembly, NgenScenarios scenarios = NgenScenarios.Default, string exeConfig = null,
                                    string appBase = null)
        {
            ThrowIfDisposed();
            ThrowIfRunning();

            _errors.Clear();
            ExitCode = null;
            _ngen = new ConsoleApp(NgenExePath, string.Format(@"install ""{0}"" /nologo", assembly));
            _ngen.ConsoleOutput += OnConsoleOutput;
            _ngen.Exited += OnConsoleExited;
            _ngen.Run();
        }

        /// <summary>
        /// Delete the native images of an assembly and its dependencies from the native image cache.
        /// </summary>
        /// <param name="assembly">Path of the assembly or full display name, e.g. 
        /// "myAssembly, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0038abc9deabfle5".</param>
        /// <param name="scenarios">Multiple images can be generated, depending on usage scenarios.</param>
        /// <param name="exeConfig">Use the configuration of the specified executable assembly.</param>
        /// <param name="appBase">When locating dependencies, use the specified directory as the application base.</param>
        public void UninstallAssembly(string assembly, NgenScenarios scenarios = NgenScenarios.Default, string exeConfig = null,
                                      string appBase = null)
        {
            ThrowIfDisposed();
            ThrowIfRunning();

            _errors.Clear();
            ExitCode = null;
            _ngen = new ConsoleApp(NgenExePath, string.Format(@"uninstall ""{0}"" /nologo /verbose", assembly));
            _ngen.ConsoleOutput += OnConsoleOutput;
            _ngen.Exited += OnConsoleExited;
            _ngen.Run();
        }

        /// <summary>
        /// Display the state of the native images for an assembly and its dependencies.
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public void DisplayAssembly(string assemblyName)
        {
            ThrowIfDisposed();
            ThrowIfRunning();

            _errors.Clear();
            ExitCode = null;
            _ngen = new ConsoleApp(NgenExePath, string.Format(@"display ""{0}"" /nologo", assemblyName));
            _ngen.ConsoleOutput += OnConsoleOutput;
            _ngen.Exited += OnConsoleExited;
            _ngen.Run();
        }

        /// <summary>
        /// Display the state of all native images.
        /// </summary>
        /// <returns></returns>
        public void DisplayAllAssemblies()
        {
            ThrowIfDisposed();
            ThrowIfRunning();

            _errors.Clear();
            ExitCode = null;
            _ngen = new ConsoleApp(NgenExePath, string.Format(@"display /nologo"));
            _ngen.ConsoleOutput += OnConsoleOutput;
            _ngen.Exited += OnConsoleExited;
            _ngen.Run();
        }

        /// <summary>
        /// Update native images that have become invalid.
        /// </summary>
        /// <returns></returns>
        public void UpdateAllAssemblies()
        {
            ThrowIfDisposed();
            ThrowIfRunning();

            _errors.Clear();
            ExitCode = null;
            _ngen = new ConsoleApp(NgenExePath, string.Format(@"update /nologo"));
            _ngen.ConsoleOutput += OnConsoleOutput;
            _ngen.Exited += OnConsoleExited;
            _ngen.Run();
        }

        public void Cancel(int forceCloseMillisecondsTimeout = Timeout.Infinite)
        {
            ThrowIfDisposed();

            if (_ngen != null)
            {
                _ngen.Stop(ConsoleSpecialKey.ControlBreak, forceCloseMillisecondsTimeout);
            }
        }

        public bool WaitForExit(int millisecondsTimeout = Timeout.Infinite)
        {
            ThrowIfDisposed();

            if (_ngen != null)
            {
                return _ngen.WaitForExit(millisecondsTimeout);
            }
            return true;
        }

        private void OnConsoleOutput(object sender, ConsoleOutputEventArgs e)
        {
            if (e.IsError)
            {
                _errors.Add(e.Line);
            }

            OnConsoleOutput(e);
        }

        private void OnConsoleExited(object sender, EventArgs e)
        {
            if (_ngen != null)
            {
                ExitCode = _ngen.ExitCode;
            }
            DisposeConsole();
            OnExited(e);
        }

        private void ThrowIfRunning()
        {
            if (_ngen != null)
                throw new InvalidOperationException("Previous operation has not finished.");
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("Object was disposed.");
        }

        private void DisposeConsole()
        {
            if (_ngen != null)
            {
                _ngen.ConsoleOutput -= OnConsoleOutput;
                _ngen.Exited -= OnConsoleExited;
                _ngen.Dispose();
                _ngen = null;
            }
        }

        public event EventHandler<EventArgs> Exited;

        public event EventHandler<ConsoleOutputEventArgs> ConsoleOutput;

        protected virtual void OnConsoleOutput(ConsoleOutputEventArgs e)
        {
            var handler = ConsoleOutput;
            if (handler != null)
                handler(this, e);
        }

        protected virtual void OnExited(EventArgs e)
        {
            var handler = Exited;
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
                    Cancel(2000);
                    DisposeConsole();
                }

                _disposed = true;
            }
        }

        ~NgenWrapper()
        {
            Dispose(false);
        }

        #endregion
    }
}
