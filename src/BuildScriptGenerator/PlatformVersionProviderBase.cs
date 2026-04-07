// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Base class for platform version providers that implements the 4-tier fallback chain:
    /// External ACR → External SDK → Direct ACR → CDN (blob storage).
    /// Used by Node.js, Python, PHP, and PHP Composer version providers.
    /// </summary>
    internal abstract class PlatformVersionProviderBase
    {
        private readonly BuildScriptGeneratorOptions options;
        private readonly ILogger logger;
        private readonly IStandardOutputWriter outputWriter;
        private PlatformVersionInfo versionInfo;

        protected PlatformVersionProviderBase(
            BuildScriptGeneratorOptions options,
            ILogger logger,
            IStandardOutputWriter outputWriter)
        {
            this.options = options;
            this.logger = logger;
            this.outputWriter = outputWriter;
        }

        protected abstract string PlatformName { get; }

        public PlatformVersionInfo GetVersionInfo()
        {
            if (this.versionInfo != null)
            {
                return this.versionInfo;
            }

            this.versionInfo = this.options.EnableDynamicInstall
                ? this.ResolveDynamicVersionInfo()
                : this.GetOnDiskVersionInfo();

            return this.versionInfo;
        }

        protected abstract PlatformVersionInfo GetOnDiskVersionInfo();

        protected abstract PlatformVersionInfo GetSdkStorageVersionInfo();

        protected abstract PlatformVersionInfo GetExternalVersionInfo();

        protected abstract PlatformVersionInfo GetExternalAcrVersionInfo();

        protected abstract PlatformVersionInfo GetAcrVersionInfo();

        private static bool HasSupportedVersions(PlatformVersionInfo versionInfo)
        {
            return versionInfo?.SupportedVersions != null && versionInfo.SupportedVersions.Any();
        }

        /// <summary>
        /// Resolves version info using the 4-tier provider chain.
        /// Priority: External ACR → External SDK → Direct ACR → CDN.
        /// </summary>
        private PlatformVersionInfo ResolveDynamicVersionInfo()
        {
            if (this.options.EnableExternalAcrSdkProvider)
            {
                var result = this.TryGetVersionInfo(this.GetExternalAcrVersionInfo, "external ACR");
                if (HasSupportedVersions(result))
                {
                    this.outputWriter.WriteLine("Version resolved using external ACR SDK provider.");
                    return result;
                }
            }

            if (this.options.EnableExternalSdkProvider)
            {
                var result = this.TryGetVersionInfo(this.GetExternalVersionInfo, "external SDK");
                if (HasSupportedVersions(result))
                {
                    this.outputWriter.WriteLine("Version resolved using external SDK provider.");
                    return result;
                }
            }

            if (this.options.EnableAcrSdkProvider)
            {
                var result = this.TryGetVersionInfo(this.GetAcrVersionInfo, "direct ACR");
                if (HasSupportedVersions(result))
                {
                    this.outputWriter.WriteLine("Version resolved using direct ACR SDK provider.");
                    return result;
                }
            }

            this.outputWriter.WriteLine("Version resolved using blob SDK storage provider(CDN).");
            return this.GetSdkStorageVersionInfo();
        }

        private PlatformVersionInfo TryGetVersionInfo(
            Func<PlatformVersionInfo> getVersionInfo,
            string providerName)
        {
            try
            {
                var result = getVersionInfo();
                if (result == null)
                {
                    this.logger.LogWarning(
                        "{ProviderName} version provider returned no version info for {PlatformName}. Trying next provider.",
                        providerName,
                        this.PlatformName);
                }

                return result;
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    "Error while getting version info from {ProviderName} provider for {PlatformName}. Ex: {Ex}",
                    providerName,
                    this.PlatformName,
                    ex);
                return null;
            }
        }
    }
}
