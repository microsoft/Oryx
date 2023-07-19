// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Defines extension methods for direct interaction with Application Insights.
    /// </summary>
    public static class LoggerAiExtensions
    {
        private const int AiMessageLengthLimit = 32768;

        /// <summary>
        /// Logs a long message in chunks, with each chunk limited in length to 2^15.
        /// </summary>
        /// <param name="logger">An instance of an <see cref="ILogger"/>.</param>
        /// <param name="level">The level of logging; will apply to all chunks.</param>
        /// <param name="header">The chunk header; will be followed by chunk index, a colon, and a line break.</param>
        /// <param name="nonFormattedMessage">The long message to be chunkified and logged.</param>
        public static void LogLongMessage(
            this ILogger logger,
            LogLevel level,
            [NotNull] string header,
            string nonFormattedMessage,
            IDictionary<string, object> properties)
        {
            int maxChunkLen = AiMessageLengthLimit - header.Length - 16; // 16 should cover for the header formatting
            int i = 0;
            var chunks = nonFormattedMessage.Chunkify(maxChunkLen);
            foreach (string chunk in chunks)
            {
                logger.Log(level, $"{header} ({++i}/{chunks.Count}):\n{chunk}", properties: properties);
            }
        }
    }
}
