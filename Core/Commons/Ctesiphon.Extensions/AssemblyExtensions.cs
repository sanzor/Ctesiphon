using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Ctesiphon.Extensions {
    public static class AssemblyExtensions {
        public static string ToCurrentAssemblyRootPath(this string targetFile) {
            var path = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().FullName).FullName, targetFile);
            return path;
        }
    }
}
