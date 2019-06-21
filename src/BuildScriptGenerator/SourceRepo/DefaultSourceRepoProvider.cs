// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal class DefaultSourceRepoProvider : ISourceRepoProvider
    {
        private readonly string _sourceDirectory;
        private readonly ILoggerFactory _loggerFactory;
        private LocalSourceRepo _sourceRepo;

        public DefaultSourceRepoProvider(IOptions<BuildScriptGeneratorOptions> options, ILoggerFactory loggerFactory)
        {
            _sourceDirectory = options.Value.SourceDir;
            _loggerFactory = loggerFactory;
        }

        public ISourceRepo GetSourceRepo()
        {
            if (_sourceRepo != null)
            {
                return _sourceRepo;
            }

            _sourceRepo = new LocalSourceRepo(_sourceDirectory, _loggerFactory);
            return _sourceRepo;
        }
    }
}