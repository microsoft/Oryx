// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Tests.Common
{
    public class Settings
    {
        public const string BuildImageName = "oryxtests/build:stretch";
        public const string WithRootAccessBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/build:stretch";
        public const string LtsVersionsBuildImageName = "oryxtests/build:lts-versions-stretch";
        public const string WithRootAccessLtsVersionsBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/build:lts-versions-stretch";
        public const string GitHubActionsBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/build:github-actions-stretch";
        public const string GitHubActionsBusterBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/build:github-actions-buster";
        public const string GitHubActionsBullseyeBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/build:github-actions-bullseye";
        public const string JamStackBullseyeBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/build:azfunc-jamstack-bullseye";
        public const string JamStackBusterBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/build:azfunc-jamstack-buster";
        public const string JamStackBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/build:azfunc-jamstack-stretch";
        public const string CliBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/cli:stretch";
        public const string CliBusterBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/cli-buster:buster";
        public const string LtsVerionsBusterBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/build:lts-versions-buster";
        public const string VsoUbuntuBuildImageName = "oryxdevmcr.azurecr.io/public/oryx/build:vso-focal";

        public const string RemoveTestContainersEnvironmentVariableName = "ORYX_REMOVE_TEST_CONTAINERS";

        public const string MySqlDbImageName = "mysql/mysql-server:5.7";
        public const string PostgresDbImageName = "postgres:alpine";
    }
}