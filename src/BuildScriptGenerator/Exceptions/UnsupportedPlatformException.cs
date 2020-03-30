// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Exceptions
{
    /// <summary>
    /// Supplied platform is not supported.
    /// </summary>
    public class UnsupportedPlatformException : InvalidUsageException
    {
        public UnsupportedPlatformException(string message)
            : base(message)
        {
        }
    }
}