// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public interface ITempDirectoryProvider
    {
        /// <summary>
        /// Creates a temporary directory and returns the path to it.
        /// </summary>
        /// <returns>The full path to the created temporary directory.</returns>
        string GetTempDirectory();
    }
}
