using System;

namespace SlavaGu.NgenWrapper
{
    [Flags]
    public enum NgenScenarios : short
    {
        Default = 0,

        /// <summary>
        /// Generate images that can be used under a debugger.
        /// </summary>
        Debug = 1,

        /// <summary>
        /// Generate images that can be used under a profiler.
        /// </summary>
        Profile = 2,

        /// <summary>
        /// Generate the minimal number of native images required by this scenario.
        /// </summary>
        NoDependencies = 4,
    }
}