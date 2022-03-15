// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class DefaultRunTimeInstallationScriptGenerator : IRunTimeInstallationScriptGenerator
    {
        private readonly IEnumerable<IProgrammingPlatform> programmingPlatforms;

        public DefaultRunTimeInstallationScriptGenerator(IEnumerable<IProgrammingPlatform> programmingPlatforms)
        {
            this.programmingPlatforms = programmingPlatforms;
        }

        public string GenerateBashScript(string targetPlatformName, RunTimeInstallationScriptGeneratorOptions opts)
        {
            var targetPlatform = this.programmingPlatforms
                .Where(p => p.Name.EqualsIgnoreCase(targetPlatformName))
                .FirstOrDefault();

            if (targetPlatform == null)
            {
                throw new UnsupportedPlatformException($"Platform '{targetPlatformName}' is not supported.");
            }

            if (!targetPlatform.SupportedVersions.Contains(opts.PlatformVersion))
            {
                throw new UnsupportedVersionException(
                    targetPlatformName,
                    opts.PlatformVersion,
                    targetPlatform.SupportedVersions);
            }

            var runScript = targetPlatform.GenerateBashRunTimeInstallationScript(opts);
            return runScript;
        }
    }
}
