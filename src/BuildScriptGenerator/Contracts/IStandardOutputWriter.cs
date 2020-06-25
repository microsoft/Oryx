// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Abstraction over the methods by which messages are written to various streams that
    /// that a user would be able to view. This is separate from logging as this does not
    /// go through telemetry, only to the console, or whichever medium is used.
    /// </summary>
    public interface IStandardOutputWriter
    {
        /// <summary>
        /// Writes a string.
        /// </summary>
        /// <param name="message">The string to be written.</param>
        void Write(string message);

        /// <summary>
        /// Writes an empty line with a line terminator.
        /// </summary>
        void WriteLine();

        /// <summary>
        /// Writes a string followed by a line terminator.
        /// </summary>
        /// <param name="message">The string to be written.</param>
        void WriteLine(string message);
    }
}
