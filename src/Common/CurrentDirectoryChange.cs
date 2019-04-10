// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;

namespace Microsoft.Oryx.Common
{
    public class CurrentDirectoryChange : IDisposable
    {
        private readonly string _originalCurrentDirectory;

        public CurrentDirectoryChange(string newCurrentDirectory)
        {
            _originalCurrentDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(newCurrentDirectory);
        }

        public void Dispose()
        {
            Directory.SetCurrentDirectory(_originalCurrentDirectory);
        }
    }
}
