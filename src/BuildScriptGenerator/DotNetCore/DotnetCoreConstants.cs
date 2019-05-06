// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    public static class DotnetCoreConstants
    {
        public const string LanguageName = "dotnet";
        public const string ProjectFileExtensionName = "csproj";
        public const string GlobalJsonFileName = "global.json";

        public const string NetCoreApp10 = "netcoreapp1.0";
        public const string NetCoreApp11 = "netcoreapp1.1";
        public const string NetCoreApp20 = "netcoreapp2.0";
        public const string NetCoreApp21 = "netcoreapp2.1";
        public const string NetCoreApp22 = "netcoreapp2.2";
        public const string NetCoreApp30 = "netcoreapp3.0";

        public const string OryxOutputPublishDirectory = "oryx_publish_output";

        public const string AspNetCorePackageReference = "Microsoft.AspNetCore";
        public const string AspNetCoreAllPackageReference = "Microsoft.AspNetCore.All";
        public const string AspNetCoreAppPackageReference = "Microsoft.AspNetCore.App";

        public const string ProjectFileLanguageDetectorProperty = "ProjectFile";
        public const string StartupFileName = "startupFileName";

        public const string WebSdkProjectXPathExpression = "/Project[@Sdk='Microsoft.NET.Sdk.Web']";
        public const string ProjectReferenceXPathExpression = "/Project/ItemGroup/PackageReference";
        public const string AssemblyNameXPathExpression = "/Project/PropertyGroup/AssemblyName";
        public const string TargetFrameworkXPathExpression = "/Project/PropertyGroup/TargetFramework";
    }
}