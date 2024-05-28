// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Exceptions
{
    /// <summary>
    /// Thrown when an image used for the 'oryx dockerfile' command is invalid.
    /// </summary>
    public class InvalidDockerfileImageException : InvalidUsageException
    {
        public InvalidDockerfileImageException(string message)
            : base(message)
        {
        }
    }
}