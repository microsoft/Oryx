// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Logging
{
    internal class FileLoggerProvider : ILoggerProvider
    {
        private readonly BuildScriptGeneratorOptions _options;
        private readonly List<ILogger> _loggers;

        public FileLoggerProvider(IOptions<BuildScriptGeneratorOptions> options)
        {
            _options = options.Value;
            _loggers = new List<ILogger>();
        }

        public virtual ILogger CreateLogger(string categoryName)
        {
            if (!IsLoggingEnabled())
            {
                return NullLogger<FileLogger>.Instance;
            }

            var logger = new FileLogger(categoryName, _options.LogFile, _options.MinimumLogLevel);
            _loggers.Add(logger);
            return logger;
        }

        // This gets called when the DI container is disposed
        public void Dispose()
        {
            if (!IsLoggingEnabled())
            {
                return;
            }

            foreach (var logger in _loggers)
            {
                if (logger is IDisposable disposable)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch { }
                }
            }
        }

        private bool IsLoggingEnabled()
        {
            return !string.IsNullOrEmpty(_options.LogFile);
        }
    }
}
