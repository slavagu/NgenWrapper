using System;
using System.Collections.Generic;
using System.IO;

namespace SlavaGu.NgenWrapper
{
    public class AssemblyNameComparer : IEqualityComparer<string>
    {
        public bool Equals(string name1, string name2)
        {
            name1 = Path.GetFileNameWithoutExtension(name1);
            name2 = Path.GetFileNameWithoutExtension(name2);

            if (name1 == null && name2 == null)
                return true;

            if (name1 == null || name2 == null)
                return false;

            return name1.Equals(name2, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
    }
}