// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.CommandLine;
using System.CommandLine.IO;

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