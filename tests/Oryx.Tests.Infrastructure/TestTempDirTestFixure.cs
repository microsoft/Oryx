// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;

namespace Oryx.Tests.Infrastructure
{
    public class TestTempDirTestFixure : IDisposable
    {
        public TestTempDirTestFixure()
        {
            // Prefix with 'tmp/oryxtests' so that one could clean the directory explicitly if its
            // not cleaned automatically (for example, in cases where a build is stopped manually
            // on the CI agent.)
            RootDirPath = Path.Combine(Path.GetTempPath(), "oryxtests", Guid.NewGuid().ToString());

            Directory.CreateDirectory(RootDirPath);
        }

        public string RootDirPath { get; }

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
