// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Oryx.Tests.Common;
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

            RunAsserts(
               () =>
               {
                   Assert.True(runDatabaseContainerResult.IsSuccess);
               },
               runDatabaseContainerResult.GetDebugInfo());

            return runDatabaseContainerResult;
        }

        protected override string GetSampleDataInsertionSql()
        {
            throw new NotImplementedException();
        }

        protected override void InsertSampleData()
        {
            // Setup user, database
            var dbSetupSql = "/tmp/databaseSetup.sql";
            var dbSetupScript = new ShellScriptBuilder()
                .AddCommand($"echo \"CREATE DATABASE {Constants.DatabaseName};\" >> {dbSetupSql}")
                .AddCommand($"echo GO >> {dbSetupSql}")
                .AddCommand($"echo \"Use {Constants.DatabaseName};\" >> {dbSetupSql}")
                .AddCommand($"echo GO >> {dbSetupSql}")
                .AddCommand($"echo \"CREATE TABLE Products (Name nvarchar(50));\" >> {dbSetupSql}")
                .AddCommand($"echo GO >> {dbSetupSql}");

            foreach (var product in SampleData)
            {
                dbSetupScript.AddCommand($"echo \"INSERT INTO Products VALUES ('{product.Value}');\" >> {dbSetupSql}");
            }

            dbSetupScript
                .AddCommand($"echo GO >> {dbSetupSql}")
                .AddCommand($"/opt/mssql-tools/bin/sqlcmd -S localhost -U {DatabaseUsername} - P {Constants.DatabaseUserPwd} -i {dbSetupSql}");

            DockerCommandResult setupDatabaseResult;
            var maxRetries = 10;
            do
            {
                // Wait for the database server to be up
                Thread.Sleep(TimeSpan.FromSeconds(30));
                setupDatabaseResult = _dockerCli.Exec(DbServerContainerName, "/bin/sh", new[] { "-c", dbSetupScript.ToString() });
                maxRetries--;
            } while (maxRetries > 0 && setupDatabaseResult.IsSuccess == false);

            if (setupDatabaseResult.IsSuccess == false)
            {
                Console.WriteLine(setupDatabaseResult.GetDebugInfo());
                throw new Exception("Couldn't setup MS SQL Server on time");
            }
        }
    }
}