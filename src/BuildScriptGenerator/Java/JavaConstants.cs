// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Java
{
    internal static class JavaConstants
    {
        public const string JavaLtsVersion = JavaVersions.JavaVersion;
        public const string PlatformName = "java";
        public const string InstalledJavaVersionsDir = "/opt/java";

        public const string MavenName = "maven";
        public const string MavenVersion = JavaVersions.MavenVersion;
        public const string InstalledMavenVersionsDir = "/opt/maven";

        public const string CreatePackageCommandUsingMaven = "mvn clean package";
        public const string CompileCommandUsingMaven = "mvn clean compile";
        public const string CreatePackageCommandUsingMavenWrapper = "./mvnw clean package";
        public const string CompileCommandUsingMavenWrapper = "./mvnw clean compile";
        public static readonly SemanticVersioning.Version
            MinMavenVersionWithNoTransferProgressSupport = new SemanticVersioning.Version("3.6.1");
    }
}