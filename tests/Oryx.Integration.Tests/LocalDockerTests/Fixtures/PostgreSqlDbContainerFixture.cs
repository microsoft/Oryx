// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.Integration.Tests.LocalDockerTests.Fixtures
{
    public class PostgreSqlDbContainerFixture : DbContainerFixtureBase
    {
        protected override DockerRunCommandResult RunDbServerContainer()
        {
            var runDatabaseContainerResult = _dockerCli.Run(
                    Settings.PostgresDbImageName,
                    environmentVariables: new List<EnvironmentVariable>
                    {
                        new EnvironmentVariable("POSTGRES_DB", Constants.DatabaseName),
                        new EnvironmentVariable("POSTGRES_USER", Constants.DatabaseUserName),
                        new EnvironmentVariable("POSTGRES_PASSWORD", Constants.DatabaseUserPwd),
                    },
                    volumes: null,
                    portMapping: null,
                    link: null,
                    runContainerInBackground: true,
                    command: null,
                    commandArguments: null);

            RunAsserts(
               () =>
               {
                   Assert.True(runDatabaseContainerResult.IsSuccess);
               },
               runDatabaseContainerResult.GetDebugInfo());

            return runDatabaseContainerResult;
        }

        protected override void InsertSampleData()
        {
            var sqlFile = "/tmp/setup.sql";
            var dbSetupScript = new ShellScriptBuilder()
                .CreateFile(sqlFile, GetSampleDataInsertionSql())
                .AddCommand($"PGPASSWORD={Constants.DatabaseUserPwd} psql -h localhost -d {Constants.DatabaseName} -U{Constants.DatabaseUserName} < {sqlFile}")
                .ToString();

            var setupDatabaseResult = _dockerCli.Exec(DbServerContainerName, "/bin/sh", new[] { "-c", dbSetupScript });
            RunAsserts(() => Assert.True(setupDatabaseResult.IsSuccess), setupDatabaseResult.GetDebugInfo());
        }
    }
}
