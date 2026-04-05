// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Detects platforms in the provided source directory.
    /// </summary>
    public class DefaultPlatformsInformationProvider
    {
        private readonly IEnumerable<IProgrammingPlatform> platforms;
        private readonly IStandardOutputWriter outputWriter;
        private readonly ILogger<DefaultPlatformsInformationProvider> logger;
        private readonly BuildScriptGeneratorOptions commonOptions;

        public DefaultPlatformsInformationProvider(
            IEnumerable<IProgrammingPlatform> platforms,
            IStandardOutputWriter outputWriter,
            ILogger<DefaultPlatformsInformationProvider> logger,
            IOptions<BuildScriptGeneratorOptions> commonOptions)
        {
            this.platforms = platforms;
            this.outputWriter = outputWriter;
            this.logger = logger;
            this.commonOptions = commonOptions.Value;
        }

        /// <summary>
        /// Detects platforms in the provided source directory.
        /// </summary>
        /// <param name="context">The <see cref="RepositoryContext"/>.</param>
        /// <returns>A list of detected platform results.</returns>
        public IEnumerable<PlatformInfo> GetPlatformsInfo(RepositoryContext context)
        {
            var platformInfos = new List<PlatformInfo>();

            this.outputWriter.WriteLine($"Primary SDK Storage URL: {this.commonOptions.OryxSdkStorageBaseUrl}");
            this.outputWriter.WriteLine($"Backup SDK Storage URL: {this.commonOptions.OryxSdkStorageBackupBaseUrl}");
            this.outputWriter.WriteLine($"ACR SDK Registry URL: {this.commonOptions.OryxAcrSdkRegistryUrl ?? "(not set)"}");

            // Log SDK provider status and resolution priority
            this.outputWriter.WriteLine("SDK provider status:");
            this.outputWriter.WriteLine($"  External ACR SDK provider: {(this.commonOptions.EnableExternalAcrSdkProvider ? "Enabled" : "Disabled")}");
            this.outputWriter.WriteLine($"  External SDK provider: {(this.commonOptions.EnableExternalSdkProvider ? "Enabled" : "Disabled")}");
            this.outputWriter.WriteLine($"  Direct ACR SDK provider: {(this.commonOptions.EnableAcrSdkProvider ? "Enabled" : "Disabled")}");
            this.outputWriter.WriteLine($"  Blob SDK provider: Enabled");

            if (this.commonOptions.EnableExternalAcrSdkProvider && !string.IsNullOrEmpty(this.commonOptions.PlatformName))
            {
                this.outputWriter.WriteLine($"External ACR SDK provider is enabled. Only using user-specified platform: {this.commonOptions.PlatformName}");
            }

            // Try detecting ALL platforms since in some scenarios this is required.
            // For example, in case of a multi-platform app like ASP.NET Core + NodeJs, we might need to dynamically
            // install both these platforms' sdks before actually using any of their commands. So even though a user
            // of Oryx might explicitly supply the platform of the app as .NET Core, we still need to make sure the
            // build environment is setup with detected platforms' sdks.
            this.outputWriter.WriteLine("Detecting platforms...");

            foreach (var platform in this.platforms)
            {
                if (!this.ShouldDetectPlatform(platform, context))
                {
                    continue;
                }

                var detectionResult = platform.Detect(context);

                if (detectionResult != null)
                {
                    var toolsInPath = platform.GetToolsToBeSetInPath(context, detectionResult);

                    platformInfos.Add(new PlatformInfo
                    {
                        DetectorResult = detectionResult,
                        RequiredToolsInPath = toolsInPath,
                    });
                }
            }

            if (platformInfos.Any())
            {
                this.outputWriter.WriteLine("Detected following platforms:");
                foreach (var platformInfo in platformInfos)
                {
                    var detectorResult = platformInfo.DetectorResult;
                    this.outputWriter.WriteLine($"  {detectorResult.Platform}: {detectorResult.PlatformVersion}");
                }
            }
            else
            {
                this.outputWriter.WriteLine("Could not detect any platform in the source directory.");
            }

            return platformInfos;
        }

        private bool ShouldDetectPlatform(IProgrammingPlatform platform, RepositoryContext context)
        {
            // Check if a platform is enabled or not
            if (!platform.IsEnabled(context))
            {
                this.outputWriter.WriteLine(
                    $"Platform '{platform.Name}' has been disabled, so skipping detection for it.");
                return false;
            }

            if (this.commonOptions.EnableExternalAcrSdkProvider && platform.Name != this.commonOptions.PlatformName)
            {
                this.logger.LogDebug(
                    "Skipping detection for platform '{PlatformName}' because External ACR SDK provider is enabled and only user provided platform '{UserPlatform}' is considered.",
                    platform.Name,
                    this.commonOptions.PlatformName);
                return false;
            }

            return true;
        }
    }
}
