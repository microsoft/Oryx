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
            var runDbContainerResult = _dockerCli.Run(
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

            RunAsserts(() => Assert.True(runDbContainerResult.IsSuccess), runDbContainerResult.GetDebugInfo());
            return runDbContainerResult;
        }

        protected override bool WaitUntilDbServerIsUp()
        {
            // Try 33 times at most, with a constant 3s in between attempts
            var retry = Policy.HandleResult(result: false).WaitAndRetry(33, i => TimeSpan.FromSeconds(3));
            return retry.Execute(() => _dockerCli.GetContainerLogs(DbServerContainerName)
                                                 .Contains("SQL Server is now ready for client connections"));
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
