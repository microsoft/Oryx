using System;
using System.IO;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    internal static class IOHelpers
    {
        internal static string CreateTempDir(string parentPath)
        {
            return Directory.CreateDirectory(Path.Combine(parentPath, Guid.NewGuid().ToString("N"))).FullName;
        }

        internal static void CreateFile(string dirPath, string fileContent, params string[] filePathParts)
        {
            File.WriteAllText(Path.Combine(dirPath, Path.Combine(filePathParts)), fileContent);
        }
    }
}
