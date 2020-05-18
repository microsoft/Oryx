// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    /// <summary>
    /// Gets hierarchical configuration from IConfiguration api and binds the properties on DotNetCoreDetectorOptions.
    /// </summary>
    public class DotNetCoreDetectorOptionsSetup : OptionsSetupBase, IConfigureOptions<DotNetCoreDetectorOptions>
    {
        public DotNetCoreDetectorOptionsSetup(IConfiguration configuration)
            : base(configuration)
        {
        }

        public void Configure(DotNetCoreDetectorOptions options)
        {
        }
    }
}
