// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    /// <summary>
    /// Exception to identify user errors, as opposed to system ones.
    /// </summary>
    internal class InvalidUsageException : Exception
    {
        public InvalidUsageException(string message) : base(message)
        { }
    }
}