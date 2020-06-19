// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    /// <summary>
    /// Generates an installation script snippet to install PHP Composer.
    /// </summary>
    internal class PhpComposerInstaller : PlatformInstallerBase
    {
        public PhpComposerInstaller(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            ILoggerFactory loggerFactory)
            : base(commonOptions, loggerFactory)
        {
        }

        public virtual string GetInstallerScriptSnippet(string version)
        {
            var script = GetInstallerScriptSnippet(platformName: "php-composer", version);
            var scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine(script);
            scriptBuilder.AppendLine(
                $"export composer=\"{Constants.TemporaryInstallationDirectoryRoot}/php-composer/{version}/composer.phar\"");
            return scriptBuilder.ToString();
        }

        public virtual bool IsVersionAlreadyInstalled(string version)
        {
            return IsVersionInstalled(
                version,
                builtInDir: PhpConstants.InstalledPhpComposerVersionDir,
                dynamicInstallDir: $"{Constants.TemporaryInstallationDirectoryRoot}/php-composer");
        }
    }
}
