// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.Exceptions
{
    /// <summary>
    /// Supplied version is not supported.
    /// </summary>
    public class UnsupportedVersionException : InvalidUsageException
    {
        public UnsupportedVersionException(string message)
            : base(message)
        {
        }

        public UnsupportedVersionException(
            string platformName,
            string attemptedVersion,
            IEnumerable<string> supportedVersions)
            : this($"Platform '{platformName}' version '{attemptedVersion}' is unsupported. " +
                  $"Supported versions: {string.Join(", ", supportedVersions)}")
        {
        }
    }
}