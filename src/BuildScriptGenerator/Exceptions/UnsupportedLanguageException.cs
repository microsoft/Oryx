// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Exceptions
{
    /// <summary>
    /// Supplied language is not supported
    /// </summary>
    public class UnsupportedLanguageException : InvalidUsageException
    {
        public UnsupportedLanguageException(string message)
            : base(message)
        {
        }
    }
}