using System;

namespace SlavaGu.NgenWrapper
{
    /// <summary>
    /// Holds event data for the Output event.
    /// </summary>
    public class NgenOutputEventArgs : EventArgs
    {
        public NgenOutputEventArgs(string line)
        {
            Line = line;
        }

        /// <summary>
        /// Output line printed out by ngen
        /// </summary>
        public string Line { get; private set; }
    }
}