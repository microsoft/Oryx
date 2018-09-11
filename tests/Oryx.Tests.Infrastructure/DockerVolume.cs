// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;

namespace Oryx.Tests.Infrastructure
{
    public class DockerVolume
    {
        public const string ReadOnlyDirRootInContainer = "/oryxtests/mounted";
        public const string WritableDirRootInContainer = "/oryxtests/mounted-writeable";

        private DockerVolume(string hostDir, string writeableContainerDir, string readonlyContainerDir)
        {
            HostDir = hostDir;
            ContainerDir = writeableContainerDir;
            ReadOnlyContainerDir = readonlyContainerDir;
        }

        /// <summary>
        /// Gets the full path to the host's directory which is being volume mounted.
        /// </summary>
        public string HostDir { get; }

        /// <summary>
        /// Gets the full path to the directory on the container which is mapped to
        /// the host's directory <see cref="HostDir"/>.
        /// For example, if host's directory is '/home/tests/app', then container directory would be something like
        /// '/oryxtests/app'
        /// </summary>
        public string ContainerDir { get; }

        // Do not expose to end user as they should only be communicating with the write-able directory
        internal string ReadOnlyContainerDir { get; }

        public static DockerVolume Create(string hostDir)
        {
            if (string.IsNullOrEmpty(hostDir))
            {
                throw new ArgumentException($"'{nameof(hostDir)}' cannot be null or empty.");
            }

            var dirInfo = new DirectoryInfo(hostDir);
            var containerDirName = dirInfo.Name;
            return Create(hostDir, containerDirName);
        }

        private static DockerVolume Create(string hostDir, string containerDirName)
        {
            // Note: Path.Combine is the ideal solution here but this would fail when we run the
            // tests on a windows machine (which most of us use).
            var readonlyContainerDir = $"{ReadOnlyDirRootInContainer}/{containerDirName}";
            var writeableContainerDir = $"{WritableDirRootInContainer}/{containerDirName}";

            return new DockerVolume(hostDir, writeableContainerDir, readonlyContainerDir);
        }
    }
}
