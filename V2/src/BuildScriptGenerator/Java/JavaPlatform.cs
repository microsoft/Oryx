// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.Java;

namespace Microsoft.Oryx.BuildScriptGenerator.Java
{
    /// <summary>
    /// Java Platform.
    /// </summary>
    internal class JavaPlatform : IProgrammingPlatform
    {
        /// <summary>
        /// The tar-gz option for Java modules.
        /// </summary>
        private readonly BuildScriptGeneratorOptions commonOptions;
        private readonly ScriptGeneratorOptionsForJava javaScriptGeneratorOptions;
        private readonly IJavaVersionProvider javaVersionProvider;
        private readonly IMavenVersionProvider mavenVersionProvider;
        private readonly ILogger<JavaPlatform> logger;
        private readonly IJavaPlatformDetector detector;
        private readonly JavaPlatformInstaller javaPlatformInstaller;
        private readonly MavenInstaller mavenInstaller;
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaPlatform"/> class.
        /// </summary>
        /// <param name="commonOptions">The <see cref="BuildScriptGeneratorOptions"/>.</param>
        /// <param name="javaScriptGeneratorOptions">The options for JavaScriptGenerator.</param>
        /// <param name="javaVersionProvider">The <see cref="JavaVersionProvider"/>.</param>
        /// <param name="mavenVersionProvider">The <see cref="IMavenVersionProvider"/>.</param>
        /// <param name="logger">The logger of Java platform.</param>
        /// <param name="detector">The detector of Java platform.</param>
        /// <param name="javaPlatformInstaller">The <see cref="JavaPlatformInstaller"/>.</param>
        /// <param name="mavenInstaller">The <see cref="MavenInstaller"/>.</param>
        public JavaPlatform(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IOptions<ScriptGeneratorOptionsForJava> javaScriptGeneratorOptions,
            IJavaVersionProvider javaVersionProvider,
            IMavenVersionProvider mavenVersionProvider,
            ILogger<JavaPlatform> logger,
            IJavaPlatformDetector detector,
            JavaPlatformInstaller javaPlatformInstaller,
            MavenInstaller mavenInstaller,
            TelemetryClient telemetryClient)
        {
            this.commonOptions = commonOptions.Value;
            this.javaScriptGeneratorOptions = javaScriptGeneratorOptions.Value;
            this.javaVersionProvider = javaVersionProvider;
            this.mavenVersionProvider = mavenVersionProvider;
            this.logger = logger;
            this.detector = detector;
            this.javaPlatformInstaller = javaPlatformInstaller;
            this.mavenInstaller = mavenInstaller;
            this.telemetryClient = telemetryClient;
        }

        /// <inheritdoc/>
        public string Name => JavaConstants.PlatformName;

        /// <inheritdoc/>
        public IEnumerable<string> SupportedVersions
        {
            get
            {
                var versionInfo = this.javaVersionProvider.GetVersionInfo();
                return versionInfo.SupportedVersions;
            }
        }

        /// <inheritdoc/>
        public PlatformDetectorResult Detect(RepositoryContext context)
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

        /// <inheritdoc/>
        public BuildScriptSnippet GenerateBashBuildScriptSnippet(
            BuildScriptGeneratorContext ctx,
            PlatformDetectorResult detectorResult)
        {
            var javaPlatformDetectorResult = detectorResult as JavaPlatformDetectorResult;
            if (javaPlatformDetectorResult == null)
            {
                throw new ArgumentException(
                    $"Expected '{nameof(detectorResult)}' argument to be of type " +
                    $"'{typeof(JavaPlatformDetectorResult)}' but got '{detectorResult.GetType()}'.");
            }

            var manifestFileProperties = new Dictionary<string, string>();

            // Write the platform name and version to the manifest file
            manifestFileProperties[ManifestFilePropertyKeys.JavaVersion] = detectorResult.PlatformVersion;

            string command = string.Empty;

            if (javaPlatformDetectorResult.UsesMavenWrapperTool)
            {
                if (this.commonOptions.ShouldPackage)
                {
                    command = JavaConstants.CreatePackageCommandUsingMavenWrapper;
                }
                else
                {
                    command = JavaConstants.CompileCommandUsingMavenWrapper;
                }
            }
            else if (javaPlatformDetectorResult.UsesMaven)
            {
                if (this.commonOptions.ShouldPackage)
                {
                    command = JavaConstants.CreatePackageCommandUsingMaven;
                }
                else
                {
                    command = JavaConstants.CompileCommandUsingMaven;
                }

                // Maven spits out lot of information related to downloading of packages which is too verbose.
                // Since the --quiet option is too quiet, we are trying to use a new switch below to just mute the
                // messages related to transfer progress of these downloads.
                // https://maven.apache.org/docs/3.6.1/release-notes.html#user-visible-changes
                var currentMavenVersion = new SemanticVersioning.Version(javaPlatformDetectorResult.MavenVersion);
                if (currentMavenVersion.CompareTo(JavaConstants.MinMavenVersionWithNoTransferProgressSupport) >= 0)
                {
                    command = $"{command} --no-transfer-progress";
                }
            }

            var scriptProps = new JavaBashBuildSnippetProperties();
            scriptProps.UsesMaven = javaPlatformDetectorResult.UsesMaven;
            scriptProps.UsesMavenWrapperTool = javaPlatformDetectorResult.UsesMavenWrapperTool;
            scriptProps.Command = command;

            string script = TemplateHelper.Render(
                TemplateHelper.TemplateResource.JavaBuildSnippet,
                scriptProps,
                this.logger,
                this.telemetryClient);

            return new BuildScriptSnippet
            {
                BashBuildScriptSnippet = script,
                BuildProperties = manifestFileProperties,
            };
        }

        /// <inheritdoc/>
        public bool IsCleanRepo(ISourceRepo repo)
        {
            return true;
        }

        /// <inheritdoc/>
        public bool IsEnabled(RepositoryContext ctx)
        {
            return this.commonOptions.EnableJavaBuild;
        }

        /// <inheritdoc/>
        public bool IsEnabledForMultiPlatformBuild(RepositoryContext ctx)
        {
            return true;
        }

        /// <inheritdoc/>
        public string GenerateBashRunTimeInstallationScript(RunTimeInstallationScriptGeneratorOptions options)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToBuildOutputDir(BuildScriptGeneratorContext ctx)
        {
            return Array.Empty<string>();
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToIntermediateDir(BuildScriptGeneratorContext ctx)
        {
            return Array.Empty<string>();
        }

        /// <inheritdoc/>
        public string GetInstallerScriptSnippet(
            BuildScriptGeneratorContext context,
            PlatformDetectorResult detectorResult)
        {
            var javaPlatformDetectorResult = detectorResult as JavaPlatformDetectorResult;
            if (javaPlatformDetectorResult == null)
            {
                throw new ArgumentException(
                    $"Expected '{nameof(detectorResult)}' argument to be of type " +
                    $"'{typeof(JavaPlatformDetectorResult)}' but got '{detectorResult.GetType()}'.");
            }

            if (this.commonOptions.EnableDynamicInstall)
            {
                this.logger.LogDebug("Dynamic install is enabled.");

                var scriptBuilder = new StringBuilder();

                this.InstallJavaSdk(javaPlatformDetectorResult.PlatformVersion, scriptBuilder);

                // We need not setup Maven if repo already uses a Maven wrapper script.
                if (!javaPlatformDetectorResult.UsesMavenWrapperTool)
                {
                    this.InstallMaven(javaPlatformDetectorResult.MavenVersion, scriptBuilder);
                }

                if (scriptBuilder.Length == 0)
                {
                    return null;
                }

                return scriptBuilder.ToString();
            }
            else
            {
                this.logger.LogDebug("Dynamic install not enabled.");
                return null;
            }
        }

        /// <inheritdoc/>
        public IDictionary<string, string> GetToolsToBeSetInPath(
            RepositoryContext context,
            PlatformDetectorResult detectorResult)
        {
            var javaPlatformDetectorResult = detectorResult as JavaPlatformDetectorResult;
            if (javaPlatformDetectorResult == null)
            {
                throw new ArgumentException(
                    $"Expected '{nameof(detectorResult)}' argument to be of type " +
                    $"'{typeof(JavaPlatformDetectorResult)}' but got '{detectorResult.GetType()}'.");
            }

            var tools = new Dictionary<string, string>();
            tools[JavaConstants.PlatformName] = javaPlatformDetectorResult.PlatformVersion;

            if (javaPlatformDetectorResult.UsesMaven &&
                !javaPlatformDetectorResult.UsesMavenWrapperTool)
            {
                tools[JavaConstants.MavenName] = javaPlatformDetectorResult.MavenVersion;
            }

            return tools;
        }

        /// <inheritdoc/>
        public void ResolveVersions(RepositoryContext context, PlatformDetectorResult detectorResult)
        {
            var javaPlatformDetectorResult = detectorResult as JavaPlatformDetectorResult;
            if (javaPlatformDetectorResult == null)
            {
                throw new ArgumentException(
                    $"Expected '{nameof(detectorResult)}' argument to be of type " +
                    $"'{typeof(JavaPlatformDetectorResult)}' but got '{detectorResult.GetType()}'.");
            }

            this.ResolveVersionsUsingHierarchicalRules(javaPlatformDetectorResult);
        }

        private void ResolveVersionsUsingHierarchicalRules(JavaPlatformDetectorResult detectorResult)
        {
            var javaVersion = ResolveJavaVersion(detectorResult.PlatformVersion);
            javaVersion = this.GetMaxSatisfyingJavaVersionAndVerify(javaVersion);

            var mavenVersion = ResolveMavenVersion(detectorResult.MavenVersion);
            mavenVersion = this.GetMaxSatisfyingMavenVersionAndVerify(mavenVersion);

            detectorResult.PlatformVersion = javaVersion;
            detectorResult.MavenVersion = mavenVersion;

            string ResolveJavaVersion(string detectedVersion)
            {
                // Explicitly specified version by user wins over detected version
                if (!string.IsNullOrEmpty(this.javaScriptGeneratorOptions.JavaVersion))
                {
                    return this.javaScriptGeneratorOptions.JavaVersion;
                }

                // If a version was detected, then use it.
                if (detectedVersion != null)
                {
                    return detectedVersion;
                }

                // Explicitly specified default version by user wins over detected default
                if (!string.IsNullOrEmpty(this.javaScriptGeneratorOptions.JavaDefaultVersion))
                {
                    return this.javaScriptGeneratorOptions.JavaDefaultVersion;
                }

                // Fallback to default version detection
                var versionInfo = this.javaVersionProvider.GetVersionInfo();
                return versionInfo.DefaultVersion;
            }

            string ResolveMavenVersion(string detectedVersion)
            {
                // Explicitly specified version by user wins over detected version
                if (!string.IsNullOrEmpty(this.javaScriptGeneratorOptions.MavenVersion))
                {
                    return this.javaScriptGeneratorOptions.MavenVersion;
                }

                // If a version was detected, then use it.
                if (detectedVersion != null)
                {
                    return detectedVersion;
                }

                // Explicitly specified default version by user wins over detected default
                if (!string.IsNullOrEmpty(this.javaScriptGeneratorOptions.MavenDefaultVersion))
                {
                    return this.javaScriptGeneratorOptions.MavenDefaultVersion;
                }

                // Fallback to default version detection
                return JavaConstants.MavenVersion;
            }
        }

        private void InstallJavaSdk(string jdkVersion, StringBuilder scriptBuilder)
        {
            if (this.javaPlatformInstaller.IsVersionAlreadyInstalled(jdkVersion))
            {
                this.logger.LogDebug(
                   "JDK version {version} is already installed. So skipping installing it again.",
                   jdkVersion);
            }
            else
            {
                this.logger.LogDebug(
                    "JSDK version {version} is not installed. " +
                    "So generating an installation script snippet for it.",
                    jdkVersion);

                var script = this.javaPlatformInstaller.GetInstallerScriptSnippet(jdkVersion);
                scriptBuilder.AppendLine(script);
            }
        }

        private void InstallMaven(string mavenVersion, StringBuilder scriptBuilder)
        {
            // Install Maven
            if (this.mavenInstaller.IsVersionAlreadyInstalled(mavenVersion))
            {
                this.logger.LogDebug(
                   "Maven version {version} is already installed. So skipping installing it again.",
                   mavenVersion);
            }
            else
            {
                this.logger.LogDebug(
                    "Maven version {version} is not installed. " +
                    "So generating an installation script snippet for it.",
                    mavenVersion);

                var script = this.mavenInstaller.GetInstallerScriptSnippet(mavenVersion);
                scriptBuilder.AppendLine(script);
            }
        }

        private string GetMaxSatisfyingJavaVersionAndVerify(string version)
        {
            var versionInfo = this.javaVersionProvider.GetVersionInfo();
            var maxSatisfyingVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                version,
                versionInfo.SupportedVersions);

            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                var exception = new UnsupportedVersionException(
                    JavaConstants.PlatformName,
                    version,
                    versionInfo.SupportedVersions);
                this.logger.LogError(
                    exception,
                    $"Exception caught, the version '{version}' is not supported for the Java platform.");
                throw exception;
            }

            return maxSatisfyingVersion;
        }

        private string GetMaxSatisfyingMavenVersionAndVerify(string version)
        {
            var versionInfo = this.mavenVersionProvider.GetVersionInfo();
            var maxSatisfyingVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                version,
                versionInfo.SupportedVersions);

            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                var exception = new UnsupportedVersionException(
                    "maven",
                    version,
                    versionInfo.SupportedVersions);
                this.logger.LogError(
                    exception,
                    $"Exception caught, the version '{version}' is not supported for Maven.");
                throw exception;
            }

            return maxSatisfyingVersion;
        }
    }
}