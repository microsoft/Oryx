// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Tests.Common
{
    public class Settings
    {
        public const string BuildImageName = "oryxtests/build:latest";
        public const string WithRootAccessBuildImageName = "oryx/build:latest";
        public const string LtsVersionsBuildImageName = "oryxtests/build:lts-versions";
        public const string WithRootAccessLtsVersionsBuildImageName = "oryx/build:lts-versions";
        public const string GitHubActionsBuildImageName = "oryx/build:github-actions";
        public const string JamStackBuildImageName = "oryx/build:azfunc-jamstack";
        public const string CliBuildImageName = "oryx/build:cli";
        public const string VsoBuildImageName = "oryx/build:vso";

        public const string RemoveTestContainersEnvironmentVariableName = "ORYX_REMOVE_TEST_CONTAINERS";

        public const string MySqlDbImageName = "mysql/mysql-server:5.7";
        public const string PostgresDbImageName = "postgres:alpine";
    }
}