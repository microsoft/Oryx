// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using McMaster.Extensions.CommandLineUtils;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal static class IConsoleExtensions
    {
        public static void WriteErrorLine(this IConsole console, string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                console.Error.WriteLine("Error: " + message);
            }
        }
    }
}