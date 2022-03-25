// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Oryx.Detector.Exceptions
{
    /// <summary>
    /// Exception used when failing to parse files.
    /// </summary>
    public class FailedToParseFileException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToParseFileException"/> class.
        /// </summary>
        /// <param name="filePath">The file whose parsing caused this exception.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public FailedToParseFileException(string filePath, string message, Exception innerException)
            : base(message, innerException)
        {
            this.FilePath = filePath;
        }

        public string FilePath { get; }
    }
}
