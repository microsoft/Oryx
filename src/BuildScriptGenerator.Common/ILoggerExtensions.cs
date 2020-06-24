// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.Common
{
    /// <summary>
    /// Extensions to ASP.NET Core's ILogger
    /// </summary>
    public static class ILoggerExtensions
    {
        /// <summary>
        /// Logs message and the supplied properties. The message must not have formatted strings as they are not processed.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="logLevel"></param>
        /// <param name="nonFormattedMessage">The message without any formatting strings in it.</param>
        /// <param name="properties"></param>
        public static void Log(
            this ILogger logger,
            LogLevel logLevel,
            string nonFormattedMessage,
            IDictionary<string, object> properties)
        {
            // For context: https://github.com/aspnet/Extensions/issues/668#issuecomment-536201597
            logger.Log(
                logLevel,
                eventId: 0,
                state: properties,
                exception: null,
                (state, exception) => nonFormattedMessage);
        }
    }
}
