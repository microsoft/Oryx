// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal static class DotnetCoreConstants
    {
        internal const string LanguageName = "dotnet";
        internal const string ProjectFileExtensionName = "csproj";
        internal const string GlobalJsonFileName = "global.json";

        internal const string NetCoreApp10 = "netcoreapp1.0";
        internal const string NetCoreApp11 = "netcoreapp1.1";
        internal const string NetCoreApp20 = "netcoreapp2.0";
        internal const string NetCoreApp21 = "netcoreapp2.1";
        internal const string NetCoreApp22 = "netcoreapp2.2";

        internal const string OryxOutputPublishDirectory = "oryx_publish_output";

        internal const string AspNetCorePackageReference = "Microsoft.AspNetCore";
        internal const string AspNetCoreAllPackageReference = "Microsoft.AspNetCore.All";
        internal const string AspNetCoreAppPackageReference = "Microsoft.AspNetCore.App";

        internal const string ProjectFileLanguageDetectorProperty = "ProjectFile";
        internal const string StartupFileName = "startupFileName";
        internal const string PublishDir = "publishDir";
        internal const string ProjectFile = "projectFile";
    }
}