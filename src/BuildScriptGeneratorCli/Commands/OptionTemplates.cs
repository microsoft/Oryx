// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal static class OptionTemplates
    {
        public const string Log = "--log-file";
        public const string LogDescription = "The file to which the log will be written.";
        public const string Debug = "--debug";
        public const string DebugDescription = "Print stack traces for exceptions.";
        public const string Platform = "--platform";
        public const string PlatformDescription = "The name of the programming platform used in the provided source directory.";
        public const string PlatformVersion = "--platform-version";
        public const string PlatformVersionDescription = "The version of the programming platform used in the provided source directory.";

        [Obsolete("Previous name for --platform")]
        public static readonly string[] Language = new string[] { "-l", "--language" };
        public static readonly string LanguageDescription = "The name of the programming platform used in the provided source directory.";
        [Obsolete("Previous name for --platform-version")]
        public static readonly string LanguageVersion = "--language-version";
        public static readonly string LanguageVersionDescription = "The version of the programming platform used in the provided source directory.";

        public static readonly string RuntimePlatform = "--runtime-platform";
        public static readonly string RuntimePlatformDescription = "The runtime platform to use in the Dockerfile. If not provided, the value for --platform will be used, otherwise the runtime platform will be auto-detected.";
        public static readonly string RuntimePlatformVersion = "--runtime-platform-version";
        public static readonly string RuntimePlatformVersionDescription = "The version of the runtime to use in the Dockerfile. If not provided, an attempt will be made to determine the runtime version to use based on the detected platform version, otherwise the 'dynamic' runtime image will be used.";

        public static readonly string Package = "--package";
        public static readonly string PackageDescription = "Package the built sources into a platform-specific format.";
        public static readonly string OsRequirements = "--os-requirements";
        public static readonly string OsRequirementsDescription = "Comma-separated list of operating system packages that will be installed (using apt-get) before building the application.";
        public static readonly string[] Property = new string[] { "-p", "--property" };
        public static readonly string PropertyDescription = "Additional information used by this tool to generate and run build scripts.";

        public static readonly string ManifestDir = "--manifest-dir";
        public static readonly string ManifestDirDescription = "The path to a directory into which the build manifest file should be written.";
        public static readonly string BuildCommandsFileName = "--buildcommands-file";
        public static readonly string BuildCommandsFileNameDescription = "Name of the file where list of build commands will be printed for node and python applications.";
        public static readonly string AppType = "--apptype";
        public static readonly string AppTypeDescription = "Type of application that the source directory has, for example: 'functions' or 'static-sites' etc.";
        public static readonly string CompressDestinationDir = "--compress-destination-dir";
        public static readonly string CompressDestinationDirDescription = "Compresses the destination directory(excluding the manifest file) into a tarball.";
        public static readonly string DynamicInstallRootDir = "--dynamic-install-root-dir";
        public static readonly string DynamicInstallRootDirDescription = "Root directory path under which dynamically installed SDKs are created under.";

        public static readonly string[] IntermediateDir = new string[] { "-i", "--intermediate-dir" };
        public static readonly string IntermediateDirDescription = "The path to a temporary directory to be used by this tool.";
        public static readonly string[] Output = new string[] { "-o", "--output" };
        public static readonly string OutputDescription = "The destination directory.";

        public static readonly string EventName = "--event-name";
        public static readonly string ProcessingTime = "--processing-time";
        public static readonly string CallerId = "--caller-id";

        public static readonly string[] Source = new string[] { "-s", "--src" };

        public static readonly string LayersDir = "--layers-dir";
        public static readonly string PlatformDir = "--platform-dir";
        public static readonly string PlanPath = "--plan-path";
    }
}
