// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Logging
{
    internal class FileLoggerProvider : ILoggerProvider
    {
        public const int DefaultMessageThresholdLimit = 2;

        private readonly BuildScriptGeneratorOptions _options;
        private readonly ObservableList<string> _messages;

        public FileLoggerProvider(IOptions<BuildScriptGeneratorOptions> options)
            : this(options, DefaultMessageThresholdLimit)
        {
        }

        // To enable unit testing
        internal FileLoggerProvider(IOptions<BuildScriptGeneratorOptions> options, int messageThresholdLimit)
        {
            _options = options.Value;
            _messages = new ObservableList<string>(messageThresholdLimit);
            _messages.MessageThresholdLimitReached += MessageThresholdLimitReached;
        }

        public virtual ILogger CreateLogger(string categoryName)
        {
            if (!IsLoggingEnabled())
            {
                return NullLogger<FileLogger>.Instance;
            }

            return new FileLogger(categoryName, _messages, _options.MinimumLogLevel);
        }

        // This gets called when the DI container is disposed
        public void Dispose()
        {
            if (!IsLoggingEnabled())
            {
                return;
            }

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
                using (var streamWriter = File.AppendText(_options.LogFile))
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

        private bool IsLoggingEnabled()
        {
            return !string.IsNullOrEmpty(_options.LogFile);
        }
    }
}
