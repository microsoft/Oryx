// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.Integration.Tests.LocalDockerTests.Fixtures
{
    public class MySqlDbContainerFixture : DbContainerFixtureBase
    {
        protected override DockerRunCommandResult RunDbServerContainer()
        {
            var runDatabaseContainerResult = _dockerCli.Run(
                Settings.MySqlDbImageName,
                environmentVariables: new List<EnvironmentVariable>
                {
                        new EnvironmentVariable("MYSQL_RANDOM_ROOT_PASSWORD", "yes"),
                        new EnvironmentVariable("MYSQL_DATABASE", Constants.DatabaseName),
                        new EnvironmentVariable("MYSQL_USER", Constants.DatabaseUserName),
                        new EnvironmentVariable("MYSQL_PASSWORD", Constants.DatabaseUserPwd),
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

            // Setup user, database
            var dbSetupSql = "/tmp/databaseSetup.sql";
            var databaseSetupScript = new ShellScriptBuilder()
                .AddCommand($"echo \"USE {Constants.DatabaseName};\" > {dbSetupSql}")
                .AddCommand($"echo \"CREATE TABLE Products (Name varchar(50) NOT NULL);\" >> {dbSetupSql}")
                .AddCommand($"echo \"INSERT INTO Products VALUES('Car');\" >> {dbSetupSql}")
                .AddCommand($"echo \"INSERT INTO Products VALUES('Television');\" >> {dbSetupSql}")
                .AddCommand($"echo \"INSERT INTO Products VALUES('Table');\" >> {dbSetupSql}")
                .AddCommand($"mysql -u {Constants.DatabaseUserName} -p{Constants.DatabaseUserPwd} < {dbSetupSql}")
                .ToString();

            var setupDatabaseResult = _dockerCli.Exec(
                DbServerContainerName,
                "/bin/sh",
                new[]
                {
                        "-c",
                        databaseSetupScript
                });

            RunAsserts(
               () =>
               {
                   Assert.True(setupDatabaseResult.IsSuccess);
               },
               setupDatabaseResult.GetDebugInfo());
        }
    }
}
