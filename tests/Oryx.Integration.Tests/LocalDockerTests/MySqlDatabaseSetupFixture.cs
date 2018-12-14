// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using Oryx.Tests.Common;
using Xunit;

namespace Oryx.Integration.Tests.LocalDockerTests
{
    public class MySqlDatabaseSetupFixture : IDisposable
    {
        private readonly DockerCli _dockerCli;

        public MySqlDatabaseSetupFixture()
        {
            _dockerCli = new DockerCli((int)TimeSpan.FromMinutes(10).TotalSeconds);

            var runResult = StartDatabaseContainer();
            DatabaseServerContainerName = runResult.ContainerName;

            // Wait for the database server to be up
            Thread.Sleep(TimeSpan.FromMinutes(1));

            InsertSampleData(runResult.ContainerName);
        }

        public string DatabaseServerContainerName { get; }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(DatabaseServerContainerName))
            {
                _dockerCli.StopContainer(DatabaseServerContainerName);
            }
        }

        private DockerRunCommandResult StartDatabaseContainer()
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

        private void InsertSampleData(string databaseServerContainerName)
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
                databaseServerContainerName,
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

        private void RunAsserts(Action action, string message)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
                Console.WriteLine(message);
                throw;
            }
        }
    }
}
