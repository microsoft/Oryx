// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Oryx.BuildScriptGenerator.Exceptions
{
    /// <summary>
    /// Exception to identify user errors, as opposed to system ones.
    /// Its message must be user friendly, and can be displayed directly to the user.
    /// </summary>
    public class InvalidUsageException : Exception
    {
        public InvalidUsageException(string message)
            : base(message)
        {
        }
    }
}