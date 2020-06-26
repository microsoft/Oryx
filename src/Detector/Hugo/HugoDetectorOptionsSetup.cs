// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.Detector.Hugo
{
    internal class HugoDetectorOptionsSetup: IConfigureOptions<HugoDetectorOptions>
    {
        private readonly IConfiguration _configuration;

        public HugoDetectorOptionsSetup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(HugoDetectorOptions options)
        {
            var hasHugoEnvironmentVariables = _configuration
                .AsEnumerable()
                .Where(kvp => kvp.Key.StartsWith("HUGO_"))
                .Any();
            options.HasHugoEnvironmentVariables = hasHugoEnvironmentVariables;
        }
    }
}
