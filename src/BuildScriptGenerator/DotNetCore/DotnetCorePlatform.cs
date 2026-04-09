// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.Common.Extensions;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.DotNetCore;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    /// <summary>
    /// .NET Core platform.
    /// </summary>
    [BuildProperty(
        DotNetCoreConstants.ProjectBuildPropertyKey,
        DotNetCoreConstants.ProjectBuildPropertyKeyDocumentation)]
    internal class DotNetCorePlatform : IProgrammingPlatform
    {
        private readonly IDotNetCoreVersionProvider versionProvider;
        private readonly DotNetCoreExternalAcrVersionProvider externalAcrVersionProvider;
        private readonly ILogger<DotNetCorePlatform> logger;
        private readonly IDotNetCorePlatformDetector detector;
        private readonly DotNetCoreScriptGeneratorOptions dotNetCoreScriptGeneratorOptions;
        private readonly BuildScriptGeneratorOptions commonOptions;
        private readonly DotNetCorePlatformInstaller platformInstaller;
        private readonly GlobalJsonSdkResolver globalJsonSdkResolver;
        private readonly IExternalSdkProvider externalSdkProvider;
        private readonly IExternalAcrSdkProvider externalAcrSdkProvider;
        private readonly IAcrSdkProvider acrSdkProvider;
        private readonly TelemetryClient telemetryClient;
        private readonly IStandardOutputWriter outputWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetCorePlatform"/> class.
        /// </summary>
        /// <param name="versionProvider">The .NET version provider.</param>
        /// <param name="logger">The logger of .NET platform.</param>
        /// <param name="detector">The detector of .NET platform.</param>
        /// <param name="commonOptions">The build options for BuildScriptGenerator.</param>
        /// <param name="dotNetCoreScriptGeneratorOptions">The options if .NET platform.</param>
        /// <param name="platformInstaller">The <see cref="DotNetCorePlatformInstaller"/>.</param>
        /// <param name="globalJsonSdkResolver">The <see cref="GlobalJsonSdkResolver"/>.</param>
        public DotNetCorePlatform(
            IDotNetCoreVersionProvider versionProvider,
            DotNetCoreExternalAcrVersionProvider externalAcrVersionProvider,
            ILogger<DotNetCorePlatform> logger,
            IDotNetCorePlatformDetector detector,
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IOptions<DotNetCoreScriptGeneratorOptions> dotNetCoreScriptGeneratorOptions,
            DotNetCorePlatformInstaller platformInstaller,
            GlobalJsonSdkResolver globalJsonSdkResolver,
            IExternalSdkProvider externalSdkProvider,
            IExternalAcrSdkProvider externalAcrSdkProvider,
            IAcrSdkProvider acrSdkProvider,
            TelemetryClient telemetryClient,
            IStandardOutputWriter outputWriter)
        {
            this.versionProvider = versionProvider;
            this.externalAcrVersionProvider = externalAcrVersionProvider;
            this.logger = logger;
            this.detector = detector;
            this.dotNetCoreScriptGeneratorOptions = dotNetCoreScriptGeneratorOptions.Value;
            this.commonOptions = commonOptions.Value;
            this.platformInstaller = platformInstaller;
            this.globalJsonSdkResolver = globalJsonSdkResolver;
            this.externalSdkProvider = externalSdkProvider;
            this.externalAcrSdkProvider = externalAcrSdkProvider;
            this.acrSdkProvider = acrSdkProvider;
            this.telemetryClient = telemetryClient;
            this.outputWriter = outputWriter;
        }

        /// <inheritdoc/>
        public string Name => DotNetCoreConstants.PlatformName;

        /// <inheritdoc/>
        public IEnumerable<string> SupportedVersions
        {
            get
            {
                var versionMap = this.versionProvider.GetSupportedVersions();

                // Map is from runtime version => sdk version
                return versionMap.Keys;
            }
        }

        /// <inheritdoc/>
        public PlatformDetectorResult Detect(RepositoryContext context)
        {
            try
            {
                var detectionResult = this.detector.Detect(new DetectorContext
                {
                    SourceRepo = new Detector.LocalSourceRepo(context.SourceRepo.RootPath),
                });
                if (detectionResult == null)
                {
                    return null;
                }

                this.ResolveVersions(context, detectionResult);
                return detectionResult;
            }
            catch (InvalidProjectFileException e)
            {
                this.logger.LogError(e, "Error occurred while trying to detect for .Net Core application(s)");
                throw new InvalidUsageException(e.Message);
            }
        }

        /// <inheritdoc/>
        public BuildScriptSnippet GenerateBashBuildScriptSnippet(
            BuildScriptGeneratorContext context,
            PlatformDetectorResult detectorResult)
        {
            var dotNetCorePlatformDetectorResult = detectorResult as DotNetCorePlatformDetectorResult;
            if (dotNetCorePlatformDetectorResult == null)
            {
                throw new ArgumentException(
                    $"Expected '{nameof(detectorResult)}' argument to be of type " +
                    $"'{typeof(DotNetCorePlatformDetectorResult)}' but got '{detectorResult.GetType()}'.");
            }

            var manifestFileProperties = new Dictionary<string, string>();
            manifestFileProperties[ManifestFilePropertyKeys.OperationId] = context.OperationId;
            manifestFileProperties[ManifestFilePropertyKeys.DotNetCoreRuntimeVersion]
                = dotNetCorePlatformDetectorResult.PlatformVersion;
            manifestFileProperties[ManifestFilePropertyKeys.DotNetCoreSdkVersion]
                = dotNetCorePlatformDetectorResult.SdkVersion;

            // optional field
            string outputType = dotNetCorePlatformDetectorResult.OutputType;
            if (!string.IsNullOrEmpty(outputType))
            {
                manifestFileProperties[ManifestFilePropertyKeys.OutputType] = outputType;
            }

            var projectFile = dotNetCorePlatformDetectorResult.ProjectFile;
            if (string.IsNullOrEmpty(projectFile))
            {
                return null;
            }

            string installBlazorWebAssemblyAOTWorkloadCommand = null;
            if (dotNetCorePlatformDetectorResult.InstallAOTWorkloads)
            {
                installBlazorWebAssemblyAOTWorkloadCommand = DotNetCoreConstants.InstallBlazorWebAssemblyAOTWorkloadCommand;
                manifestFileProperties[ManifestFilePropertyKeys.Frameworks] = "blazor";
                this.logger.LogInformation("Detected the following frameworks: blazor");
                this.outputWriter.WriteLine("Detected the following frameworks: blazor");
            }

            var templateProperties = new DotNetCoreBashBuildSnippetProperties
            {
                ProjectFile = projectFile,
                Configuration = this.GetBuildConfiguration(),
                InstallBlazorWebAssemblyAOTWorkloadCommand = installBlazorWebAssemblyAOTWorkloadCommand,
            };

            var script = TemplateHelper.Render(
                TemplateHelper.TemplateResource.DotNetCoreSnippet,
                templateProperties,
                this.logger,
                this.telemetryClient);

            SetStartupFileNameInfoInManifestFile(context, projectFile, manifestFileProperties);

            return new BuildScriptSnippet
            {
                BashBuildScriptSnippet = script,
                BuildProperties = manifestFileProperties,

                // Setting this to false to avoid copying files like '.cs' to the destination
                CopySourceDirectoryContentToDestinationDirectory = false,
            };
        }

        /// <inheritdoc/>
        public bool IsCleanRepo(ISourceRepo repo)
        {
            return true;
        }

        /// <inheritdoc/>
        public string GenerateBashRunTimeInstallationScript(RunTimeInstallationScriptGeneratorOptions options)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool IsEnabled(RepositoryContext ctx)
        {
            return this.commonOptions.EnableDotNetCoreBuild;
        }

        /// <inheritdoc/>
        public bool IsEnabledForMultiPlatformBuild(RepositoryContext ctx)
        {
            return true;
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToBuildOutputDir(
            BuildScriptGeneratorContext scriptGeneratorContext)
        {
            var dirs = new List<string>();
            dirs.Add("obj");
            dirs.Add("bin");
            return dirs;
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToIntermediateDir(
            BuildScriptGeneratorContext scriptGeneratorContext)
        {
            var dirs = new List<string>();
            dirs.Add(".git");
            dirs.Add("obj");
            dirs.Add("bin");
            return dirs;
        }

        /// <inheritdoc/>
        public string GetInstallerScriptSnippet(
            BuildScriptGeneratorContext context,
            PlatformDetectorResult detectorResult)
        {
            var dotNetCorePlatformDetectorResult = detectorResult as DotNetCorePlatformDetectorResult;
            if (dotNetCorePlatformDetectorResult == null)
            {
                throw new ArgumentException(
                    $"Expected '{nameof(detectorResult)}' argument to be of type " +
                    $"'{typeof(DotNetCorePlatformDetectorResult)}' but got '{detectorResult.GetType()}'.");
            }

            if (!this.commonOptions.EnableDynamicInstall)
            {
                this.logger.LogDebug("Dynamic install is not enabled.");
                return null;
            }

            this.logger.LogDebug("Dynamic install is enabled.");

            var sdkVersion = dotNetCorePlatformDetectorResult.SdkVersion;

            if (this.platformInstaller.IsVersionAlreadyInstalled(sdkVersion))
            {
                this.logger.LogDebug(
                    "DotNetCore SDK version {globalJsonSdkVersion} is already installed. So skipping installing it again.",
                    sdkVersion);
                return null;
            }

            // Priority: External SDK ACR → External-SDK → Direct-ACR → CDN
            // 1. Try External SDK ACR (socket → ACR)
            if (this.commonOptions.EnableExternalAcrSdkProvider)
            {
                var result = this.TryInstallFromExternalAcrSdkProvider(sdkVersion);
                if (result != null)
                {
                    return result;
                }
            }

            // 2. Try External-SDK (socket → blob storage)
            if (this.commonOptions.EnableExternalSdkProvider)
            {
                var result = this.TryInstallFromExternalSdkProvider(sdkVersion);
                if (result != null)
                {
                    return result;
                }
            }

            // 3. Try Direct-ACR (direct OCI API calls)
            if (this.commonOptions.EnableAcrSdkProvider)
            {
                var runtimeVersion = dotNetCorePlatformDetectorResult.PlatformVersion;
                var result = this.TryInstallFromAcrSdkProvider(sdkVersion, runtimeVersion);
                if (result != null)
                {
                    return result;
                }
            }

            // 4. CDN fallback
            this.outputWriter.WriteLine($"Falling back to CDN for '{this.Name}' version '{sdkVersion}'.");
            this.logger.LogDebug(
                "DotNetCore SDK version {globalJsonSdkVersion} is not installed. So generating an installation script snippet for it.",
                sdkVersion);
            return this.platformInstaller.GetInstallerScriptSnippet(sdkVersion);
        }

        /// <inheritdoc/>
        public void ResolveVersions(RepositoryContext context, PlatformDetectorResult detectorResult)
        {
            var dotNetCorePlatformDetectorResult = detectorResult as DotNetCorePlatformDetectorResult;
            if (dotNetCorePlatformDetectorResult == null)
            {
                throw new ArgumentException(
                    $"Expected '{nameof(detectorResult)}' argument to be of type " +
                    $"'{typeof(DotNetCorePlatformDetectorResult)}' but got '{detectorResult.GetType()}'.");
            }

            // Resolve runtime version (same for all flows — ExternalAcrSdkProvider does not affect this).
            var resolvedRuntimeVersion = this.GetRuntimeVersionUsingHierarchicalRules(
                dotNetCorePlatformDetectorResult.PlatformVersion);
            resolvedRuntimeVersion = this.GetMaxSatisfyingRuntimeVersionAndVerify(resolvedRuntimeVersion);
            dotNetCorePlatformDetectorResult.PlatformVersion = resolvedRuntimeVersion;

            // Resolve SDK version.
            // External ACR provider dictates the SDK version directly — no runtime→SDK lookup needed.
            // This is .NET-specific: other platforms have a single version,
            // but .NET has separate runtime and SDK versions. The external host already knows
            // which SDK companion image to use, so we trust its SDK version.
            if (this.commonOptions.EnableExternalAcrSdkProvider)
            {
                try
                {
                    var dictatedSdk = this.externalAcrVersionProvider.GetSdkVersion();
                    if (!string.IsNullOrEmpty(dictatedSdk))
                    {
                        this.logger.LogInformation(
                            "External ACR provider returned .NET SDK version {Version}. Using it directly.",
                            dictatedSdk);
                        dotNetCorePlatformDetectorResult.SdkVersion = dictatedSdk;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(
                        ex,
                        "Error getting SDK version from external ACR provider. Falling back to next provider for SDK resolution.");
                }
            }

            // Normal SDK resolution: look up from runtime→SDK version map.
            var versionMap = this.versionProvider.GetSupportedVersions();
            var sdkVersion = this.GetSdkVersion(context, dotNetCorePlatformDetectorResult.PlatformVersion, versionMap);
            dotNetCorePlatformDetectorResult.SdkVersion = sdkVersion;
        }

        /// <inheritdoc/>
        public IDictionary<string, string> GetToolsToBeSetInPath(
            RepositoryContext context,
            PlatformDetectorResult detectorResult)
        {
            var dotNetCorePlatformDetectorResult = detectorResult as DotNetCorePlatformDetectorResult;
            if (dotNetCorePlatformDetectorResult == null)
            {
                throw new ArgumentException(
                    $"Expected '{nameof(detectorResult)}' argument to be of type " +
                    $"'{typeof(DotNetCorePlatformDetectorResult)}' but got '{detectorResult.GetType()}'.");
            }

            var tools = new Dictionary<string, string>();
            tools[DotNetCoreConstants.PlatformName] = dotNetCorePlatformDetectorResult.SdkVersion;
            return tools;
        }

        /// <summary>
        /// Even though the runtime container has the logic of finding out the startup file based on
        /// 'runtimeconfig.json' prefix, we still set the name in the manifest file because of the following
        /// scenario: let's say output directory currently has 'foo.dll' and user made a change to the project
        /// name or assembly name property to 'bar' which causes 'bar.dll' to be published. If the output
        /// directory was NOT cleaned, then we would now be having both 'foo.runtimeconfig.json' and
        /// 'bar.runtimeconfig.json' which causes a problem for runtime container as it cannot figure out the
        /// right startup DLL. So, to help that scenario we always set the start-up file name in manifest file.
        /// The runtime container will first look into manifest file to find the startup filename, if the
        /// file name is not present or if a manifest file is not present at all(ex: in case of VS Publish where
        /// the build does not happen with Oryx), then the runtime container's logic will fallback to looking at
        /// runtimeconfig.json prefixes.
        /// </summary>
        private static void SetStartupFileNameInfoInManifestFile(
            BuildScriptGeneratorContext context,
            string projectFile,
            IDictionary<string, string> buildProperties)
        {
            string startupDllFileName;
            var projectFileContent = context.SourceRepo.ReadFile(projectFile);
            var projFileDoc = XDocument.Load(new StringReader(projectFileContent));
            var assemblyNameElement = projFileDoc.XPathSelectElement(DotNetCoreConstants.AssemblyNameXPathExpression);
            if (assemblyNameElement == null)
            {
                var name = Path.GetFileNameWithoutExtension(projectFile);
                startupDllFileName = $"{name}.dll";
            }
            else
            {
                startupDllFileName = $"{assemblyNameElement.Value}.dll";
            }

            buildProperties[DotNetCoreManifestFilePropertyKeys.StartupDllFileName] = startupDllFileName;
        }

        private string TryInstallFromAcrSdkProvider(string sdkVersion, string runtimeVersion)
        {
            this.logger.LogDebug(
                "DotNetCore SDK version {version} is not installed. ACR SDK provider is enabled, so trying to fetch SDK using it.",
                sdkVersion);

            try
            {
                var result = this.acrSdkProvider.RequestSdkFromAcrAsync(
                    this.Name, sdkVersion, this.commonOptions.DebianFlavor, runtimeVersion).Result;

                if (result)
                {
                    this.logger.LogDebug(
                        "DotNetCore SDK version {version} is fetched successfully using ACR SDK provider.",
                        sdkVersion);
                    this.outputWriter.WriteLine($"SDK for '{this.Name}' version '{sdkVersion}' fetched via direct ACR provider.");
                    return this.platformInstaller.GetInstallerScriptSnippet(sdkVersion, skipSdkBinaryDownload: true);
                }

                this.logger.LogDebug(
                    "DotNetCore SDK version {version} is not fetched via ACR SDK provider. Trying next provider.",
                    sdkVersion);
                this.outputWriter.WriteLine($"Failed to fetch SDK via direct ACR provider for '{this.Name}' version '{sdkVersion}'. Trying next provider.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Error while fetching DotNetCore SDK version {version} using ACR SDK provider. Trying next provider.",
                    sdkVersion);
                this.outputWriter.WriteLine($"Error fetching SDK via direct ACR provider for '{this.Name}' version '{sdkVersion}'. Trying next provider.");
            }

            return null;
        }

        private string TryInstallFromExternalSdkProvider(string sdkVersion)
        {
            this.logger.LogDebug(
                "DotNetCore SDK version {version} is not installed. External SDK provider is enabled so trying to fetch SDK using it.",
                sdkVersion);

            try
            {
                var blobName = BlobNameHelper.GetBlobNameForVersion(this.Name, sdkVersion, this.commonOptions.DebianFlavor);
                if (this.externalSdkProvider.RequestBlobAsync(this.Name, blobName).Result)
                {
                    this.logger.LogDebug(
                        "DotNetCore SDK version {version} is fetched successfully using external SDK provider. Skipping platform binary download.",
                        sdkVersion);
                    this.outputWriter.WriteLine($"SDK for '{this.Name}' version '{sdkVersion}' fetched via external SDK provider.");
                    return this.platformInstaller.GetInstallerScriptSnippet(sdkVersion, skipSdkBinaryDownload: true);
                }

                this.logger.LogDebug(
                    "DotNetCore SDK version {version} is not fetched successfully using external SDK provider. Generating installation script snippet.",
                    sdkVersion);
                this.outputWriter.WriteLine($"Failed to fetch SDK via external SDK provider for '{this.Name}' version '{sdkVersion}'. Trying next provider.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Error while fetching DotNetCore SDK version version {version} using external SDK provider.",
                    sdkVersion);
                this.outputWriter.WriteLine($"Error fetching SDK via external SDK provider for '{this.Name}' version '{sdkVersion}'. Trying next provider.");
            }

            return null;
        }

        private string TryInstallFromExternalAcrSdkProvider(string sdkVersion)
        {
            this.logger.LogDebug(
                "DotNetCore SDK version {version} is not installed. External ACR SDK provider is enabled, so trying to fetch SDK using it.",
                sdkVersion);

            try
            {
                if (this.externalAcrSdkProvider.RequestSdkAsync(
                    this.Name, sdkVersion, this.commonOptions.DebianFlavor).Result)
                {
                    this.logger.LogDebug(
                        "DotNetCore SDK version {version} is fetched successfully using external ACR SDK provider. Skipping platform binary download.",
                        sdkVersion);
                    this.outputWriter.WriteLine($"SDK for '{this.Name}' version '{sdkVersion}' fetched via external ACR provider.");
                    return this.platformInstaller.GetInstallerScriptSnippet(sdkVersion, skipSdkBinaryDownload: true);
                }

                this.logger.LogDebug(
                    "DotNetCore SDK version {version} is not fetched via external ACR SDK provider. Trying next provider.",
                    sdkVersion);
                this.outputWriter.WriteLine($"Failed to fetch SDK via external ACR provider for '{this.Name}' version '{sdkVersion}'. Trying next provider.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Error while fetching DotNetCore SDK version {version} using external ACR SDK provider. Trying next provider.",
                    sdkVersion);
                this.outputWriter.WriteLine($"Error fetching SDK via external ACR provider for '{this.Name}' version '{sdkVersion}'. Trying next provider.");
            }

            return null;
        }

        private string GetSdkVersion(
            RepositoryContext context,
            string runtimeVersion,
            Dictionary<string, string> versionMap)
        {
            if (this.commonOptions.EnableDynamicInstall
                && context.SourceRepo.FileExists(DotNetCoreConstants.GlobalJsonFileName))
            {
                var availableSdks = versionMap.Values;
                var globalJsonSdkVersion = this.globalJsonSdkResolver.GetSatisfyingSdkVersion(
                    context.SourceRepo,
                    runtimeVersion,
                    availableSdks);
                return globalJsonSdkVersion;
            }

            return versionMap[runtimeVersion];
        }

        private string GetBuildConfiguration()
        {
            var configuration = this.dotNetCoreScriptGeneratorOptions.MSBuildConfiguration;
            if (string.IsNullOrEmpty(configuration))
            {
                configuration = DotNetCoreConstants.DefaultMSBuildConfiguration;
            }

            return configuration;
        }

        private string GetMaxSatisfyingRuntimeVersionAndVerify(string runtimeVersion)
        {
            var versionMap = this.versionProvider.GetSupportedVersions();

            // Since our semantic versioning library does not work with .NET Core preview version format, here
            // we do some trivial way of finding the latest version which matches a given runtime version
            // Runtime versions are usually like: 1.0, 2.1, 3.1, 5.0 etc.
            // (these are constructed from netcoreapp21, netcoreapp31 etc.)
            // Preview version of sdks also have preview versions of runtime versions and hence they
            // have '-' in their names.
            var nonPreviewRuntimeVersions = versionMap.Keys.Where(version => version.IndexOf("-") < 0);
            var maxSatisfyingVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                runtimeVersion,
                nonPreviewRuntimeVersions);

            // Check if a preview version is available
            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                // NOTE:
                // Preview versions: 5.0.0-preview.3.20214.6, 5.0.0-preview.2.20160.6, 5.0.0-preview.1.20120.5
                var previewRuntimeVersions = versionMap.Keys
                    .Where(version => version.Contains("-"))
                    .Where(version => version.StartsWith(runtimeVersion))
                    .OrderByDescending(version => version);
                if (previewRuntimeVersions.Any())
                {
                    maxSatisfyingVersion = previewRuntimeVersions.First();
                }
            }

            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                var exception = new UnsupportedVersionException(
                    DotNetCoreConstants.PlatformName,
                    runtimeVersion,
                    versionMap.Keys);
                this.logger.LogError(
                    exception,
                    $"Exception caught, the version '{runtimeVersion}' is not supported for the .NET Core platform.");
                throw exception;
            }

            return maxSatisfyingVersion;
        }

        private string GetRuntimeVersionUsingHierarchicalRules(string detectedVersion)
        {
            // Explicitly specified version by user wins over detected version
            if (!string.IsNullOrEmpty(this.dotNetCoreScriptGeneratorOptions.DotNetCoreRuntimeVersion))
            {
                return this.dotNetCoreScriptGeneratorOptions.DotNetCoreRuntimeVersion;
            }

            // If a version was detected, then use it.
            if (!string.IsNullOrEmpty(detectedVersion))
            {
                return detectedVersion;
            }

            // Explicitly specified default version by user wins over detected default
            if (!string.IsNullOrEmpty(this.dotNetCoreScriptGeneratorOptions.DefaultRuntimeVersion))
            {
                return this.dotNetCoreScriptGeneratorOptions.DefaultRuntimeVersion;
            }

            // Fallback to default version detection
            var defaultVersion = this.versionProvider.GetDefaultRuntimeVersion();
            return defaultVersion;
        }

        private bool TryGetExplicitVersion(out string explicitVersion)
        {
            explicitVersion = null;

            var platformName = this.commonOptions.PlatformName;
            if (platformName.EqualsIgnoreCase(DotNetCoreConstants.PlatformName))
            {
                if (string.IsNullOrWhiteSpace(this.dotNetCoreScriptGeneratorOptions.DotNetCoreRuntimeVersion))
                {
                    return false;
                }

                explicitVersion = this.dotNetCoreScriptGeneratorOptions.DotNetCoreRuntimeVersion;
                return true;
            }

            return false;
        }
    }
}