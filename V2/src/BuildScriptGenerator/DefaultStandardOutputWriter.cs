// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Default implementation of IStandardOutputWriter that takes an action to write messages to the output.
    /// </summary>
    public class DefaultStandardOutputWriter : IStandardOutputWriter
    {
        private readonly Action<string> write;
        private readonly Action<string> writeLine;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultStandardOutputWriter"/> class.
        /// Default constructor that doesn't write anything if no parameters provided.
        /// </summary>
        public DefaultStandardOutputWriter()
        {
            this.write = (message) => { };
            this.writeLine = (message) => { };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultStandardOutputWriter"/> class.
        /// The provided action will also be used for the WriteLine() call by adding the proper line terminator.
        /// </summary>
        /// <param name="write">Action that takes a string and writes it to the output.</param>
        public DefaultStandardOutputWriter(Action<string> write)
        {
            this.write = write;
            this.writeLine = (message) => { write(string.Format("{0}\n", message)); };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultStandardOutputWriter"/> class.
        /// </summary>
        /// <param name="write">Action that takes a string and writes it to the output.</param>
        /// <param name="writeLine">Action that takes a string and writes it to the output with a line terminator.</param>
        public DefaultStandardOutputWriter(Action<string> write, Action<string> writeLine)
        {
            this.write = write;
            this.writeLine = writeLine;
        }

        /// <inheritdoc/>
        public void Write(string message)
        {
            this.write(message);
        }

        /// <inheritdoc/>
        public void WriteLine(string message)
        {
            this.writeLine(message);
        }

        /// <inheritdoc/>
        public void WriteLine()
        {
            this.WriteLine(message: null);
        }
    }
}
