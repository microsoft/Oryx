// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Php;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    /// <summary>
    /// Gets hierarchical configuration from IConfiguration api and binds the properties on PhpDetectorOptions.
    /// </summary>
    public class PhpDetectorOptionsSetup : OptionsSetupBase, IConfigureOptions<PhpDetectorOptions>
    {
        public PhpDetectorOptionsSetup(IConfiguration configuration)
            : base(configuration)
        {
        }

        public void Configure(PhpDetectorOptions options)
        {
        }
    }
}
