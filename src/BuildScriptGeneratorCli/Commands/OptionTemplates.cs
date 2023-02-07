// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Linq;

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
        public const string Language = "--language";
        public const string LanguageDescription = "The name of the programming platform used in the provided source directory.";
        [Obsolete("Previous name for --platform-version")]
        public const string LanguageVersion = "--language-version";
        public const string LanguageVersionDescription = "The version of the programming platform used in the provided source directory.";

        public const string RuntimePlatform = "--runtime-platform";
        public const string RuntimePlatformDescription = "The runtime platform to use in the Dockerfile. If not provided, the value for --platform will be used, otherwise the runtime platform will be auto-detected.";
        public const string RuntimePlatformVersion = "--runtime-platform-version";
        public const string RuntimePlatformVersionDescription = "The version of the runtime to use in the Dockerfile. If not provided, an attempt will be made to determine the runtime version to use based on the detected platform version, otherwise the 'dynamic' runtime image will be used.";

        public const string Package = "--package";
        public const string PackageDescription = "Package the built sources into a platform-specific format.";
        public const string OsRequirements = "--os-requirements";
        public const string OsRequirementsDescription = "Comma-separated list of operating system packages that will be installed (using apt-get) before building the application.";
        public const string Property = "--property";
        public const string PropertyDescription = "Additional information used by this tool to generate and run build scripts.";

        public const string ManifestDir = "--manifest-dir";
        public const string ManifestDirDescription = "The path to a directory into which the build manifest file should be written.";
        public const string BuildCommandsFileName = "--buildcommands-file";
        public const string BuildCommandsFileNameDescription = "Name of the file where list of build commands will be printed for node and python applications.";
        public const string AppType = "--apptype";
        public const string AppTypeDescription = "Type of application that the source directory has, for example: 'functions' or 'static-sites' etc.";
        public const string CompressDestinationDir = "--compress-destination-dir";
        public const string CompressDestinationDirDescription = "Compresses the destination directory(excluding the manifest file) into a tarball.";
        public const string DynamicInstallRootDir = "--dynamic-install-root-dir";
        public const string DynamicInstallRootDirDescription = "Root directory path under which dynamically installed SDKs are created under.";

        public const string IntermediateDir = "--intermediate-dir";
        public const string IntermediateDirDescription = "The path to a temporary directory to be used by this tool.";
        public const string Output = "--output";
        public const string OutputDescription = "The destination directory.";

    }
}
