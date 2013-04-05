using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SlavaGu.NgenWrapper
{
    public static class AssemblyHelper
    {
        /// <summary>
        /// Get list of referenced assemblies from the same directory as assemblyFile or from appBase, if provided
        /// </summary>
        /// <param name="assemblyFile"></param>
        /// <param name="appBase"></param>
        /// <returns></returns>
        public static List<string> GetLocalReferences(string assemblyFile, string appBase = null)
        {
            return FindReferencedAssemblies(assemblyFile, appBase).Distinct().OrderBy(a => a).ToList();
        }

        private static IEnumerable<string> FindReferencedAssemblies(string assemblyFile, string appBase)
        {
            var assembly = Assembly.ReflectionOnlyLoadFrom(assemblyFile);
            var folder = appBase ?? Path.GetDirectoryName(assemblyFile) ?? string.Empty;

            foreach (var assemblyName in assembly.GetReferencedAssemblies())
            {
                var referencedAssemblyFile = Path.Combine(folder, assemblyName.Name) + ".dll";
                if (File.Exists(referencedAssemblyFile))
                {
                    yield return referencedAssemblyFile;
                    foreach (var a in FindReferencedAssemblies(referencedAssemblyFile, appBase))
                    {
                        yield return a;
                    }
                }
            }
        }

        public static string GetShortAssemblyFileName(string s)
        {
            s = Path.GetFileName(s);
            
            if (s == null)
                return null;

            if (s.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                s.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                return s;

            if (File.Exists(s + ".exe"))
                return s + ".exe";

            return s + ".dll";
        }
    }
}
