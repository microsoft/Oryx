// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Python;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    /// <summary>
    /// Gets hierarchical configuration from IConfiguration api and binds the properties on PythonDetectorOptions.
    /// </summary>
    public class PythonDetectorOptionsSetup : OptionsSetupBase, IConfigureOptions<PythonDetectorOptions>
    {
        public PythonDetectorOptionsSetup(IConfiguration configuration)
            : base(configuration)
        {
        }

        public void Configure(PythonDetectorOptions options)
        {
        }
    }
}
