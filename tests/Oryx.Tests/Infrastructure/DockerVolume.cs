// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;

namespace Oryx.Tests.Infrastructure
{
    public class DockerVolume
    {
        public DockerVolume(string hostDir, string containerDir)
        {
            if (string.IsNullOrEmpty(hostDir))
            {
                throw new ArgumentException($"'{nameof(hostDir)}' cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(containerDir))
            {
                throw new ArgumentException($"'{nameof(containerDir)}' cannot be null or empty.");
            }

            HostDir = hostDir;
            ContainerDir = containerDir;
        }

        public string HostDir { get; }

        public string ContainerDir { get; }
    }
}
