// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Tests.Common
{
    public class Settings
    {
        public const string BuildImageName = "oryxtests/build:debian-stretch";
        public const string WithRootAccessBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/build:debian-stretch";
        public const string LtsVersionsBuildImageName = "oryxtests/build:lts-versions-debian-stretch";
        public const string WithRootAccessLtsVersionsBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/build:lts-versions-debian-stretch";
        public const string GitHubActionsBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/build:github-actions-debian-stretch";
        public const string GitHubActionsBusterBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/build:github-actions-debian-buster";
        public const string GitHubActionsBullseyeBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/build:github-actions-debian-bullseye";
        public const string JamStackBullseyeBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/build:azfunc-jamstack-debian-bullseye";
        public const string JamStackBusterBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/build:azfunc-jamstack-debian-buster";
        public const string JamStackBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/build:azfunc-jamstack-debian-stretch";
        public const string CliBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/cli:debian-stretch";
        public const string CliBusterBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/cli:debian-buster";
        public const string CliBullseyeBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/cli:debian-bullseye";
        public const string LtsVerionsBusterBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/build:lts-versions-debian-buster";
        public const string VsoUbuntuBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/build:vso-ubuntu-focal";

        public const string RemoveTestContainersEnvironmentVariableName = "ORYX_REMOVE_TEST_CONTAINERS";

        public const string MySqlDbImageName = "mysql/mysql-server:5.7";
        public const string PostgresDbImageName = "postgres:alpine";
    }
}