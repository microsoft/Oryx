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

        /// <summary>
        /// Appends a line to the versionsToBuild.txt file
        /// for the specified platform and Debian flavor.
        /// The file is sorted and duplicates are removed
        /// after the line is appended.
        /// </summary>
        /// <param name="platformName">The name of the platform.</param>
        /// <param name="line">The line to append to the file.</param>
        public void UpdateVersionsToBuildTxt(string platformName, string line)
        {
            try
            {
                foreach (string debianFlavor in Constants.DebianFlavors)
                {
                    if (debianFlavor == "bookworm" && platformName == "python")
                    {
                        continue;
                    }

                    string versionsToBuildTxtAbsolutePath = Path.Combine(
                        this.oryxRootPath,
                        "platforms",
                        platformName,
                        "versions",
                        debianFlavor,
                        Constants.VersionsToBuildTxtFileName);
                    File.AppendAllText(versionsToBuildTxtAbsolutePath, line + Environment.NewLine);

                    // sort
                    Console.WriteLine($"[UpdateVersionsToBuildTxt] Updating {versionsToBuildTxtAbsolutePath}...");
                    var contents = File.ReadAllLines(versionsToBuildTxtAbsolutePath);
                    Array.Sort(contents);
                    File.WriteAllLines(versionsToBuildTxtAbsolutePath, contents.Distinct());
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to update versionsToBuild.txt under {this.oryxRootPath}: {ex.Message}");
            }
        }
    }
}
