// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Oryx.Tests.Common;
using Polly;
using Xunit;

namespace Microsoft.Oryx.Integration.Tests.LocalDockerTests.Fixtures
{
    public class SqlServerDbContainerFixture : DbContainerFixtureBase
    {
        private const string DatabaseUsername = "sa";

        public override List<EnvironmentVariable> GetCredentialsAsEnvVars()
        {
            return new List<EnvironmentVariable>
            {
                new EnvironmentVariable(DbServerHostnameEnvVarName, Constants.InternalDbLinkName),
                new EnvironmentVariable(DbServerUsernameEnvVarName, DatabaseUsername),
                new EnvironmentVariable(DbServerPasswordEnvVarName, Constants.DatabaseUserPwd),
                new EnvironmentVariable(DbServerDatabaseEnvVarName, Constants.DatabaseName),
            };
        }

        protected override DockerRunCommandResult RunDbServerContainer()
        {
            var runDatabaseContainerResult = _dockerCli.Run(
                    Settings.MicrosoftSQLServerImageName,
                    environmentVariables: new List<EnvironmentVariable>
                    {
                        new EnvironmentVariable("ACCEPT_EULA", "Y"),
                        new EnvironmentVariable("SA_PASSWORD", Constants.DatabaseUserPwd),
                    },
                    volumes: null,
                    portMapping: null,
                    link: null,
                    runContainerInBackground: true,
                    command: null,
                    commandArguments: null);

            RunAsserts(() => Assert.True(runDatabaseContainerResult.IsSuccess), runDatabaseContainerResult.GetDebugInfo());
            return runDatabaseContainerResult;
        }

        protected override void WaitUntilDbServerIsUp()
        {
            var interval = TimeSpan.FromSeconds(3);
            // Try 30 times at most, with a constant 3s in between attempts
            var retry = Policy.HandleResult(result: false).WaitAndRetry(30, i => interval);
            retry.Execute(() => _dockerCli.GetContainerLogs(DbServerContainerName).Contains("SQL Server is now ready for client connections"));
            Thread.Sleep(interval);
        }

        protected override void InsertSampleData()
        {
            const string sqlFile = "/tmp/setup.sql";
            string baseSqlCmd = $"/opt/mssql-tools/bin/sqlcmd -S localhost -U {DatabaseUsername} -P {Constants.DatabaseUserPwd}";
            var dbSetupScript = new ShellScriptBuilder()
                .CreateFile(sqlFile, GetSampleDataInsertionSql())
                .AddCommand($"{baseSqlCmd} -Q \"CREATE DATABASE {Constants.DatabaseName};\"")
                .AddCommand($"{baseSqlCmd} -i {sqlFile}")
                .ToString();

            var result = _dockerCli.Exec(DbServerContainerName, "/bin/sh", new[] { "-c", dbSetupScript });
            RunAsserts(() => Assert.True(result.IsSuccess), result.GetDebugInfo());
        }
    }
}
