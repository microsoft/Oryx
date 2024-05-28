// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal static class OptionArgumentTemplates
    {
        // Arranged in alphabeticaal order
        // Shared option templates
        public static readonly string AppType = "--apptype";
        public static readonly string AppTypeDescription = "Type of application that the source directory has, for example: 'functions' or 'static-sites' etc.";
        public static readonly string BuildCommandsFileName = "--buildcommands-file";
        public static readonly string BuildCommandsFileNameDescription = "Name of the file where list of build commands will be printed for node and python applications.";
        public static readonly string CallerId = "--caller-id";
        public static readonly string CompressDestinationDir = "--compress-destination-dir";
        public static readonly string CompressDestinationDirDescription = "Compresses the destination directory(excluding the manifest file) into a tarball.";
        public static readonly string Debug = "--debug";
        public static readonly string DebugDescription = "Print stack traces for exceptions.";
        public static readonly string DynamicInstallRootDir = "--dynamic-install-root-dir";
        public static readonly string DynamicInstallRootDirDescription = "Root directory path under which dynamically installed SDKs are created under.";
        public static readonly string EventName = "--event-name";
        public static readonly string[] IntermediateDir = new string[] { "-i", "--intermediate-dir" };
        public static readonly string IntermediateDirDescription = "The path to a temporary directory to be used by this tool.";
        public static readonly string LayersDir = "--layers-dir";
        public static readonly string LayersDirDescription = "Layers directory path.";
        public static readonly string Log = "--log-file";
        public static readonly string LogDescription = "The file to which the log will be written.";
        public static readonly string ManifestDir = "--manifest-dir";
        public static readonly string ManifestDirDescription = "The path to a directory into which the build manifest file should be written.";
        public static readonly string OsRequirements = "--os-requirements";
        public static readonly string OsRequirementsDescription = "Comma-separated list of operating system packages that will be installed (using apt-get) before building the application.";
        public static readonly string[] Output = new string[] { "-o", "--output" };
        public static readonly string OutputDescription = "The destination directory.";
        public static readonly string Package = "--package";
        public static readonly string PackageDescription = "Package the built sources into a platform-specific format.";
        public static readonly string PlanPath = "--plan-path";
        public static readonly string PlanPathDescription = "Build plan TOML path.";
        public static readonly string Platform = "--platform";
        public static readonly string PlatformDescription = "The name of the programming platform used in the provided source directory.";
        public static readonly string PlatformDir = "--platform-dir";
        public static readonly string PlatformDirDescription = "Platform directory path.";
        public static readonly string PlatformVersion = "--platform-version";
        public static readonly string PlatformVersionDescription = "The version of the programming platform used in the provided source directory.";
        public static readonly string ProcessingTime = "--processing-time";
        public static readonly string[] Property = new string[] { "-p", "--property" };
        public static readonly string PropertyDescription = "Additional information used by this tool to generate and run build scripts.";
        public static readonly string RuntimePlatform = "--runtime-platform";
        public static readonly string RuntimePlatformDescription = "The runtime platform to use in the Dockerfile. If not provided, the value for --platform will be used, otherwise the runtime platform will be auto-detected.";
        public static readonly string RuntimePlatformVersion = "--runtime-platform-version";
        public static readonly string RuntimePlatformVersionDescription = "The version of the runtime to use in the Dockerfile. If not provided, an attempt will be made to determine the runtime version to use based on the detected platform version, otherwise the 'dynamic' runtime image will be used.";
        public static readonly string[] Source = new string[] { "-s", "--src" };

        // Shared argument tempaltes
        public static readonly string AppDir = "appDir";
        public static readonly string AppDirDescription = "The application directory.";
        public static readonly string SourceDir = "SourceDir";
        public static readonly string SourceDirDescription = "The source directory.";

        // Option, argument, and description templates for specific commands
        // BuildScriptCommand
        public static readonly string BuildScriptOutputDescription = "The path that the build script will be written to. If not specified, the result will be written to STDOUT.";

        // BuildpackDetectCommand
        public static readonly string BuildpackDetectPlatformDirDescription = "Platform directory path.";

        // ExecCommand
        public static readonly string ExecSourceDescription = "Source directory.";

        // DetectCommand
        public static readonly string DetectSourceDirDescription = "The source directory. If no value is provided, the current directory is used.";
        public static readonly string DetectOutputDescription = "Output the detected platform data in chosen format. Example: json, table. If not set, by default output will print out as a table.";

        // DockerfileCommand
        public static readonly string DockerfileBindPort = "--bind-port";
        public static readonly string DockerfileBindPortDescription = "The port where the application will bind to in the runtime image. If not provided, the default port will vary based on the platform used; Python uses 80, all other platforms used 8080.";
        public static readonly string DockerfileBuildImage = "--build-image";
        public static readonly string DockerfileBuildImageDescription = "The mcr.microsoft.com/oryx build image to use in the dockerfile, provided in the format '<image>:<tag>'. If no value is provided, the 'cli:stable' Oryx image will be used.";
        public static readonly string DockerfileOutputDescription = "The path that the dockerfile will be written to. If not specified, the contents of the dockerfile will be written to STDOUT.";
        public static readonly string DockerfileSourceDirDescription = "The source directory. If no value is provided, the current directory is used.";

        // PlatformsCommand
        public static readonly string PlatformsJsonOutput = "--json";
        public static readonly string PlatformsJsonOutputDescription = "Output the supported platform data in JSON format.";

        // PrepareEnvironmentCommand
        public static readonly string PrepareEnvironmentPlatformsAndVersions = "--platforms-and-versions";
        public static readonly string PrepareEnvironmentPlatformsAndVersionsDescription = "Comma separated values of platforms and versions to be installed. Example: dotnet=3.1.200,php=7.4.5,node=2.3";
        public static readonly string PrepareEnvironmentPlatformsAndVersionsFile = "--platforms-and-versions-file";
        public static readonly string PrepareEnvironmentPlatformsAndVersionsFileDescription = "A .env file which contains list of platforms and the versions that need to be installed. Example: \ndotnet=3.1.200\nphp=7.4.5\nnode=2.3";
        public static readonly string PrepareEnvironmentSkipDetection = "--skip-detection";
        public static readonly string PrepareEnvioronmentSkipDetectionDescription = "Skip detection of platforms and install the requested platforms.";

        // RunScriptCommand
        public static readonly string RunScriptOutputDescription = "[Optional] Path to which the script will be written. If not specified, will default to STDOUT.";
        public static readonly string RunScriptPlatformDescription = "The name of the programming platform, e.g. 'nodejs'.";
        public static readonly string RunScriptPlatformVersionDescription = "The version of the platform to run the application on, e.g. '10' for node js.";

        // Obselete options
        [Obsolete("Previous name for --platform")]
        public static readonly string[] Language = new string[] { "-l", "--language" };
        public static readonly string LanguageDescription = "The name of the programming platform used in the provided source directory.";
        [Obsolete("Previous name for --platform-version")]
        public static readonly string LanguageVersion = "--language-version";
        public static readonly string LanguageVersionDescription = "The version of the programming platform used in the provided source directory.";
    }
}
