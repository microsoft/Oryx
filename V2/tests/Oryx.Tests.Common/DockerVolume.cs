// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.Tests.Common
{
    public class DockerVolume
    {
        private const string DockerSocket = "/var/run/docker.sock";
        public static readonly DockerVolume DockerDaemonSocket = new DockerVolume(
            originalHostDir: null,
            mountedHostDir: DockerSocket,
            containerDir: DockerSocket);

        // VSTS variable used to identify if the tests are running in VSTS or not (for example, on dev machines)
        public const string VstsAgentNameEnivronmentVariable = "AGENT_NAME";

        // NOTE: Make sure to change the file "vsts\scripts\dockerCleanup.sh" if the name of the following directory
        // is changed.
        public const string MountedHostDirRootName = "OryxTestsMountedDirs";

        public const string ContainerDirRoot = "/oryxtests";

        private DockerVolume(string originalHostDir, string mountedHostDir, string containerDir)
        {
            OriginalHostDir = originalHostDir;
            MountedHostDir = mountedHostDir;
            ContainerDir = containerDir;
        }

        public string OriginalHostDir { get; }

        public string MountedHostDir { get; }

        /// <summary>
        /// Gets the full path to the directory on the container which is mapped to
        /// the host's directory <see cref="OriginalHostDir"/>.
        /// For example, if host's directory is '/home/tests/app', then container directory would be something like
        /// '/oryxtests/app'
        /// </summary>
        public string ContainerDir { get; }

        /// <summary>
        /// Creates a copy of a local directory, and returns a DockerVolume instance for mounting that copy in a
        /// container.
        /// </summary>
        /// <param name="hostDir">local directory to be used in a container</param>
        /// <returns>DockerVolume instance that can be used to mount the new copy of `originalDir`.</returns>
        /// <param name="writeToHostDir">a boolean which indicates if we want the actual directory or the copy of the actual directory</param>
        public static DockerVolume CreateMirror(string hostDir, bool writeToHostDir = false)
        {
            if (string.IsNullOrEmpty(hostDir))
            {
                throw new ArgumentException($"'{nameof(hostDir)}' cannot be null or empty.");
            }
            if (!Directory.Exists(hostDir))
            {
                throw new ArgumentException($"'{nameof(hostDir)}' must point to an existing directory.");
            }

            var dirInfo = new DirectoryInfo(hostDir);

            // Copy the host directory to a different location and mount that one as it's always possible that a
            // single sample app could be tested by different tests and we do not want to modify its original state
            // and also it would not be nice to see changes in git repository when running tests.

            // Since Docker containers run as 'root' and any content written into the mounted directory is owned by
            // the 'root', the CI agent which runs as a non-root account cannot delete that content, so we try to
            // create content in a well known location on the CI agent so that these folders are deleted during the
            // clean-up task.
            var agentName = Environment.GetEnvironmentVariable(VstsAgentNameEnivronmentVariable);
            string tempDirRoot = null;
            if (string.IsNullOrEmpty(agentName))
            {
                // On dev machines, create the temporary folders underneath the 'bin' hierarchy itself. This way
                // a user can clean those folders when they do 'git clean -xdf' on their source repo.
                var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
                tempDirRoot = fileInfo.Directory.FullName;
            }
            else
            {
                // Put the folders in a well known location which the CI build definition looks for to clean up.
                tempDirRoot = Path.Combine(Path.GetTempPath(), MountedHostDirRootName);
            }

            var writableHostDir = hostDir;
            if (!writeToHostDir)
            {
                writableHostDir = Path.Combine(
                    tempDirRoot,
                    Guid.NewGuid().ToString("N"),
                    dirInfo.Name);
                CopyDirectories(hostDir, writableHostDir, copySubDirs: true);
            }
            // Grant permissions to the folder we just copied on the host machine. The permisions here allow the
            // user(a non-root user) in the container to read/write/execute files.
            var linuxOS = OSPlatform.Create("LINUX");
            if (RuntimeInformation.IsOSPlatform(linuxOS))
            {
                ProcessHelper.RunProcess(
                    "chmod",
                    new[] { "-R", "777", writableHostDir },
                    workingDirectory: null,
                    waitTimeForExit: null);
            }

            var containerDirName = dirInfo.Name;

            // Note: Path.Combine is the ideal solution here but this would fail when we run the
            // tests on a windows machine (which most of us use).
            var containerDir = $"{ContainerDirRoot}/{containerDirName}";
            return new DockerVolume(hostDir, writableHostDir, containerDir);
        }

        private static void CopyDirectories(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            var dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (var subdir in dirs)
                {
                    var temppath = Path.Combine(destDirName, subdir.Name);
                    CopyDirectories(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}