// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Tests.Common
{
    public class Settings
    {
        public const string BuildImageName = "oryxtests/build:latest";
        public const string PackImageName  = "oryxdevms/pack:latest";
        
        public const string RemoveTestContainersEnvironmentVariableName = "ORYX_REMOVE_TEST_CONTAINERS";

        public const string MySqlDbImageName = "mysql/mysql-server:5.7";
        public const string PostgresDbImageName = "postgres:alpine";
    }
}