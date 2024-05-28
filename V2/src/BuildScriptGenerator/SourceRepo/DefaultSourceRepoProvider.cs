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
        private readonly string sourceDirectory;
        private readonly ILoggerFactory loggerFactory;
        private LocalSourceRepo sourceRepo;

        public DefaultSourceRepoProvider(IOptions<BuildScriptGeneratorOptions> options, ILoggerFactory loggerFactory)
        {
            this.sourceDirectory = options.Value.SourceDir;
            this.loggerFactory = loggerFactory;
        }

        public ISourceRepo GetSourceRepo()
        {
            if (this.sourceRepo != null)
            {
                return this.sourceRepo;
            }

            this.sourceRepo = new LocalSourceRepo(this.sourceDirectory, this.loggerFactory);
            return this.sourceRepo;
        }
    }
}