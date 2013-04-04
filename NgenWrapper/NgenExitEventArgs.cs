using System;

namespace SlavaGu.NgenWrapper
{
    /// <summary>
    /// Holds event data for the Exit event.
    /// </summary>
    public class NgenExitEventArgs : EventArgs
    {
        public NgenExitEventArgs(int? exitCode)
        {
            ExitCode = exitCode;
        }

        /// <summary>
        /// Exit code of Ngen.
        /// </summary>
        public int? ExitCode { get; private set; }
    }
}