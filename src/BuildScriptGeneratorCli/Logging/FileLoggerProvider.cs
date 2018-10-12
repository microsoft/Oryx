// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Logging
{
    internal class FileLoggerProvider : ILoggerProvider
    {
        public const string DefaultLogFileName = "log.txt";
        public const int DefaultMessageThresholdLimit = 2;

        private readonly BuildScriptGeneratorOptions _options;
        private readonly ObservableList<string> _messages;
        private readonly string _logFile;

        public FileLoggerProvider(
            ITempDirectoryProvider tempDirectoryProvider,
            IOptions<BuildScriptGeneratorOptions> options)
            : this(tempDirectoryProvider, options, DefaultMessageThresholdLimit)
        {
        }

        // To enable unit testing
        internal FileLoggerProvider(
            ITempDirectoryProvider tempDirectoryProvider,
            IOptions<BuildScriptGeneratorOptions> options,
            int messageThresholdLimit)
        {
            _options = options.Value;
            _messages = new ObservableList<string>(messageThresholdLimit);
            _messages.MessageThresholdLimitReached += MessageThresholdLimitReached;

            _logFile = _options.LogFile;
            // Provide a default log file if one is not provided by the user
            if (string.IsNullOrEmpty(_logFile))
            {
                _logFile = Path.Combine(tempDirectoryProvider.GetTempDirectory(), DefaultLogFileName);
            }
        }

        public virtual ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(categoryName, _messages, _options.MinimumLogLevel);
        }

        // This gets called when the DI container is disposed
        public void Dispose()
        {
            try
            {
                FlushMessages();
            }
            catch { }
        }

        private void MessageThresholdLimitReached(object sender, EventArgs e)
        {
            FlushMessages();
        }

        private void FlushMessages()
        {
            if (_messages.Count > 0)
            {
                using (var streamWriter = File.AppendText(_logFile))
                {
                    foreach (var message in _messages)
                    {
                        streamWriter.WriteLine(message);
                    }
                    streamWriter.Flush();
                }
                _messages.Clear();
            }
        }
    }
}
