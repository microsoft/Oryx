// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.Automation.Telemetry
{
    public class LoggerImpl : ILogger
    {
        private readonly ILoggerFactory loggerFactory;

        public LoggerImpl(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public ILogger<DotNet.DotNet> Logger => (ILogger<DotNet.DotNet>)this.loggerFactory.CreateLogger<DotNet.DotNet>();
    }
}
