using System;

namespace SlavaGu.NgenWrapper
{
    /// <summary>
    /// Holds event data for the Progress event.
    /// </summary>
    public class NgenProgressEventArgs : EventArgs
    {
        public NgenProgressEventArgs(string assembly, int current, int total)
        {
            Assembly = assembly;
            Current = current;
            Total = total;
        }

        /// <summary>
        /// Current assembly name.
        /// </summary>
        public string Assembly { get; private set; }

        /// <summary>
        /// Number of the currently processed assembly. This is always less than or equal to Total.
        /// </summary>
        public int Current { get; private set; }

        /// <summary>
        /// Total number of local assemblies to be processed by Ngen.
        /// </summary>
        public int Total { get; private set; }
    }
}