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
    public class PostgresDatabaseSetupFixture : IDisposable
    {
        private readonly DockerCli _dockerCli;

        public PostgresDatabaseSetupFixture()
        {
            _dockerCli = new DockerCli();

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

        private void InsertSampleData(string databaseServerContainerName)
        {

            // Setup user, database
            var dbSetupSql = "/tmp/databaseSetup.sql";
            var databaseSetupScript = new ShellScriptBuilder()
                .AddCommand($"echo \"PGPASSWORD={Constants.DatabaseUserPwd}\" > {dbSetupSql}")
                .AddCommand($"echo \"USE {Constants.DatabaseName};\" > {dbSetupSql}")
                .AddCommand($"echo \"CREATE TABLE Products (Name varchar(50) NOT NULL);\" >> {dbSetupSql}")
                .AddCommand($"echo \"INSERT INTO Products VALUES('Car');\" >> {dbSetupSql}")
                .AddCommand($"echo \"INSERT INTO Products VALUES('Television');\" >> {dbSetupSql}")
                .AddCommand($"echo \"INSERT INTO Products VALUES('Table');\" >> {dbSetupSql}")
                .AddCommand($"psql -h localhost -d {Constants.DatabaseName} -U{Constants.DatabaseUserName} < {dbSetupSql}")
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
