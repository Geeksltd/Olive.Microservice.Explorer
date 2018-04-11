using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MacroserviceExplorer.Utils
{
    public static class Extensions
    {
        public static DirectoryInfo AsDirectory(this string dir)
        {
            return new DirectoryInfo(dir);
        }

        public static string NameWithoutExtension(this FileInfo file)
        {
            return Path.GetFileNameWithoutExtension(file.FullName);
        }
    }
}
