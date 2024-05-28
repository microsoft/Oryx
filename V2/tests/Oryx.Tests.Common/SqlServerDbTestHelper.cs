// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Oryx.Tests.Common
{
    /// <summary>
    /// Helper class for operations involving SQL server integration tests.
    /// </summary>
    public static class SqlServerDbTestHelper
    {
        private const string DbServerHostnameEnvVarName = "SQLSERVER_DATABASE_HOST";
        private const string DbServerUsernameEnvVarName = "SQLSERVER_DATABASE_USERNAME";
        private const string DbServerPasswordEnvVarName = "SQLSERVER_DATABASE_PASSWORD";
        private const string DbServerDatabaseEnvVarName = "SQLSERVER_DATABASE_NAME";

        public static List<EnvironmentVariable> GetEnvironmentVariables()
        {
            return new List<EnvironmentVariable>
            {
                new EnvironmentVariable(
                    DbServerHostnameEnvVarName, Environment.GetEnvironmentVariable(DbServerHostnameEnvVarName)),
                new EnvironmentVariable(
                    DbServerDatabaseEnvVarName, Environment.GetEnvironmentVariable(DbServerDatabaseEnvVarName)),
                new EnvironmentVariable(
                    DbServerUsernameEnvVarName, Environment.GetEnvironmentVariable(DbServerUsernameEnvVarName)),
                new EnvironmentVariable(
                    DbServerPasswordEnvVarName, Environment.GetEnvironmentVariable(DbServerPasswordEnvVarName)),
            };
        }
    }
}
