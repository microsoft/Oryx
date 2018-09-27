// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Logging
{
    internal class FileLogger : ILogger, IDisposable
    {
        public const int DefaultMessageThreshold = 5;

        public FileLogger(string categoryName, string logFile, LogLevel minimumLogLevel)
        {
            CategoryName = categoryName;
            LogFile = logFile;
            MinimumLogLevel = minimumLogLevel;
            Messages = new List<string>();
        }

        // To enable unit testing
        internal string CategoryName { get; }

        internal string LogFile { get; }

        internal LogLevel MinimumLogLevel { get; }

        internal List<string> Messages { get; }

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

            // Any messages that don't meet the threshold are flushed out when this logger is disposed.
            // Look at FileLoggerProvider.Dispose (this method gets called when the DI container is disposed).
            if (Messages.Count >= DefaultMessageThreshold)
            {
                FlushMessages();
            }
        }

        private void FlushMessages()
        {
            if (Messages.Count > 0)
            {
                using (var streamWriter = File.AppendText(LogFile))
                {
                    foreach (var message in Messages)
                    {
                        streamWriter.WriteLine(message);
                    }
                    streamWriter.Flush();
                }
                Messages.Clear();
            }
        }

        public void Dispose()
        {
            try
            {
                FlushMessages();
            }
            catch { }
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
