// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;

namespace Microsoft.Oryx.Automation.Services
{
    public class FileService : IFileService
    {
        private string oryxRootPath;

        public FileService(string oryxRootPath)
        {
            this.oryxRootPath = oryxRootPath;
        }

        public void UpdateVersionsToBuildTxt(string platformName, string line)
        {
            foreach (string debianFlavor in Constants.DebianFlavors)
            {
                var versionsToBuildTxtAbsolutePath = Path.Combine(
                    this.oryxRootPath,
                    "platforms",
                    platformName,
                    "versions",
                    debianFlavor,
                    Constants.VersionsToBuildTxtFileName);
                System.IO.File.AppendAllText(versionsToBuildTxtAbsolutePath, line);

                // sort
                Console.WriteLine($"[UpdateVersionsToBuildTxt] Updating {versionsToBuildTxtAbsolutePath}...");
                var contents = System.IO.File.ReadAllLines(versionsToBuildTxtAbsolutePath);
                Array.Sort(contents);
                System.IO.File.WriteAllLines(versionsToBuildTxtAbsolutePath, contents.Distinct());
            }
        }
    }
}
