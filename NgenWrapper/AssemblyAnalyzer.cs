using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SlavaGu.NgenWrapper
{
    public static class AssemblyAnalyzer
    {
        /// <summary>
        /// Get list of locally referenced assemblies.
        /// </summary>
        /// <param name="assemblyFile">Assembly file name</param>
        /// <param name="appBase">Optional folder where to search for dependecies</param>
        /// <returns>List of local dependencies</returns>
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

        private static readonly Regex ExtensionRegex = new Regex(@"\.(exe|dll)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Get short name for assembly excluding folder name
        /// </summary>
        /// <param name="assemblyFileName">Assembly file name</param>
        /// <returns></returns>
        public static string GetShortAssemblyFileName(string assemblyFileName)
        {
            var s = Path.GetFileName(assemblyFileName);
            
            if (string.IsNullOrEmpty(s))
                return s;

            if (ExtensionRegex.IsMatch(s))
                return s;

            var exe = s + ".exe";
            if (File.Exists(exe))
                return exe;

            var dll = s + ".dll";
            if (File.Exists(dll))
                return dll;

            return s;
        }

        /// <summary>
        /// Compares assembly file names to match ngen output with local file names
        /// </summary>
        public class FileNameComparer : IEqualityComparer<string>
        {
            public bool Equals(string name1, string name2)
            {
                if (name1 == name2)
                    return true;

                if (name1 == null || name2 == null)
                    return false;

                name1 = Path.GetFileName(name1);
                name2 = Path.GetFileName(name2);

                if (name1 == null || name2 == null)
                    return false;

                var bothHaveExtension = ExtensionRegex.IsMatch(name1) && ExtensionRegex.IsMatch(name2);
                if (!bothHaveExtension)
                {
                    name1 = ExtensionRegex.Replace(name1, string.Empty);
                    name2 = ExtensionRegex.Replace(name2, string.Empty);
                }

                return name1.Equals(name2, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(string obj)
            {
                return obj.GetHashCode();
            }
        }

    }
}
