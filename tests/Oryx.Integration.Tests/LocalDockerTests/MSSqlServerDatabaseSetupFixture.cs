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
    public class MSSqlServerDatabaseSetupFixture : IDisposable
    {
        private readonly DockerCli _dockerCli;

        public MSSqlServerDatabaseSetupFixture()
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
                .AddCommand($"echo \"CREATE DATABASE {Constants.DatabaseName};\" >> {dbSetupSql}")
                .AddCommand($"echo GO >> {dbSetupSql}")
                .AddCommand($"echo \"Use {Constants.DatabaseName};\" >> {dbSetupSql}")
                .AddCommand($"echo GO >> {dbSetupSql}")
                .AddCommand($"echo \"CREATE TABLE Products (Name nvarchar(50));\" >> {dbSetupSql}")
                .AddCommand($"echo GO >> {dbSetupSql}")
                .AddCommand($"echo \"INSERT INTO Products VALUES ('Car');\" >> {dbSetupSql}")
                .AddCommand($"echo \"INSERT INTO Products VALUES ('Television');\" >> {dbSetupSql}")
                .AddCommand($"echo \"INSERT INTO Products VALUES ('Table');\" >> {dbSetupSql}")
                .AddCommand($"echo GO >> {dbSetupSql}")
                .AddCommand($"/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P {Constants.DatabaseUserPwd} -i {dbSetupSql}")
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
