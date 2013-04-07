
namespace SlavaGu.NgenWrapper
{
    /// <summary>
    /// Ngen.exe (Native Image Generator) wrapper for most common ngening purposes.
    /// For custom scenarios or asynchronous patterns use NgenLauncher directly.
    /// </summary>
    public static class Ngen
    {
        /// <summary>
        /// Generate native images for an assembly and its dependencies and install the images in the native image cache.
        /// </summary>
        /// <param name="assembly">Path of the assembly or full display name.</param>
        /// <returns>True if installation succeeded or the assembly has already been installed, or False otherwise.</returns>
        public static bool InstallAssembly(string assembly)
        {
            using (var ngenLauncher = new NgenLauncher())
            {
                ngenLauncher.InstallAssembly(assembly);
                ngenLauncher.WaitForExit();
                return ngenLauncher.IsSuccessful;
            }
        }

        /// <summary>
        /// Delete the native images of an assembly and its dependencies from the native image cache.
        /// </summary>
        /// <param name="assembly">Path of the assembly or full display name.</param>
        /// <returns>True if uninstallation succeeded, or False if the assembly is not installed.</returns>
        public static bool UninstallAssembly(string assembly)
        {
            using (var ngenLauncher = new NgenLauncher())
            {
                ngenLauncher.UninstallAssembly(assembly);
                ngenLauncher.WaitForExit();
                return ngenLauncher.IsSuccessful;
            }
        }

        /// <summary>
        /// Check if the native image for an assembly has been installed.
        /// </summary>
        /// <param name="assembly">Path of the assembly or full display name.</param>
        /// <remarks>Referenced assemblies are not included in the check.</remarks>
        public static bool IsAssemblyInstalled(string assembly)
        {
            using (var ngenLauncher = new NgenLauncher())
            {
                ngenLauncher.DisplayAssembly(assembly);
                ngenLauncher.WaitForExit();
                return ngenLauncher.IsSuccessful;
            }
        }

    }
}
