// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System;

namespace Microsoft.Oryx.BuildScriptGenerator.Exceptions
{
    /// <summary>
    /// Exception to identify user errors, as opposed to system ones.
    /// </summary>
    public class InvalidUsageException : Exception
    {
        public InvalidUsageException(string message) : base(message)
        { }
    }
}