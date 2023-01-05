// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;

namespace Microsoft.Oryx.Tests.Common
{
    public class TestTempDirTestFixture : IDisposable
    {
        public TestTempDirTestFixture()
        {
            // Prefix with 'tmp/oryxtests' so that one could clean the directory explicitly if its
            // not cleaned automatically (for example, in cases where a build is stopped manually
            // on the CI agent.)
            RootDirPath = Path.Combine(Path.GetTempPath(), "oryxtests", Guid.NewGuid().ToString());

            Directory.CreateDirectory(RootDirPath);

            // set DEBIAN_FLAVOR environment variable to any flavor
            // this is for when we are not running the CLI in an image, since all
            // images should already have an env var set
            Environment.SetEnvironmentVariable("DEBIAN_FLAVOR", "stretch");
        }

        public string RootDirPath { get; }

        public string CreateChildDir()
        {
            return Directory.CreateDirectory(GenerateRandomChildDirPath()).FullName;
        }

        public string GenerateRandomChildDirPath()
        {
            return Path.Combine(RootDirPath, Guid.NewGuid().ToString());
        }

        public void Dispose()
        {
            if (Directory.Exists(RootDirPath))
            {
                try
                {
                    Directory.Delete(RootDirPath, recursive: true);
                }
                catch
                {
                    // Do not throw in dispose
                }
            }
        }
    }
}
