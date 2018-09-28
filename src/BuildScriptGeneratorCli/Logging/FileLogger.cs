// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Logging
{
    internal class FileLogger : ILogger
    {
        public FileLogger(string categoryName, IList<string> messages, LogLevel minimumLogLevel)
        {
            CategoryName = categoryName;
            Messages = messages;
            MinimumLogLevel = minimumLogLevel;
        }

        // To enable unit testing
        internal string CategoryName { get; }

        internal LogLevel MinimumLogLevel { get; }

        internal IList<string> Messages { get; }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NoopDisposable.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= MinimumLogLevel;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = $"{DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss")} : {logLevel} : {CategoryName} : {state}";
            if (exception != null)
            {
                message += Environment.NewLine + exception.ToString();
            }

            // There's only one thread that is going to add messages and flush messages if required,
            // so we wouldn't be having concurrency issues.
            Messages.Add(message);
        }

        private class NoopDisposable : IDisposable
        {
            public static NoopDisposable Instance = new NoopDisposable();

            public void Dispose()
            {
            }
        }
    }
}
