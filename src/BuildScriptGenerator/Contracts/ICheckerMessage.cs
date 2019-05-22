// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Defines the interface of a message that can be emitted from an <see cref="IChecker"/>.
    /// </summary>
    public interface ICheckerMessage
    {
        /// <summary>
        /// Severity level.
        /// </summary>
        LogLevel Level { get; }

        /// <summary>
        /// Body of message. Can span multiple lines.
        /// </summary>
        string Content { get; }
    }
}
