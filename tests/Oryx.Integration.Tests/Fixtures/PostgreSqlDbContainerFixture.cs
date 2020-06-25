// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using Polly;
using Xunit;

namespace Microsoft.Oryx.Integration.Tests.Fixtures
{
    public class PostgreSqlDbContainerFixture : DbContainerFixtureBase
    {
        protected override DockerRunCommandResult RunDbServerContainer()
        {
            var runDbContainerResult = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.PostgresDbImageName,
                EnvironmentVariables = new List<EnvironmentVariable>
                {
                    new EnvironmentVariable("POSTGRES_DB", Constants.DatabaseName),
                    new EnvironmentVariable("POSTGRES_USER", Constants.DatabaseUserName),
                    new EnvironmentVariable("POSTGRES_PASSWORD", Constants.DatabaseUserPwd),
                },
                RunContainerInBackground = true,
            });

            RunAsserts(() => Assert.True(runDbContainerResult.IsSuccess), runDbContainerResult.GetDebugInfo());
            return runDbContainerResult;
        }

        protected override bool WaitUntilDbServerIsUp()
        {
            var retry = Policy.HandleResult(result: false).WaitAndRetry(retryCount: 36, i => TimeSpan.FromSeconds(5));
            return retry.Execute(() =>
            {
                try
                {
                    var lookUpText = "listening on IPv4 address";
                    (var stdOut, var stdErr) = _dockerCli.GetContainerLogs(DbServerContainerName);
                    return stdOut.Contains(lookUpText) || stdErr.Contains(lookUpText);
                }
                catch
                {
                    // In case of any exception, we consider this retry as a failure and will retry again.
                    return false;
                }
            });
        }

        protected override void InsertSampleData()
        {
            const string sqlFile = "/tmp/postgres_setup.sql";
            var sqlQuery = GetSampleDataInsertionSql();
            var dbSetupScript = new ShellScriptBuilder()
                .CreateFile(sqlFile, $"\"{sqlQuery}\"")
                .AddCommand(
                $"PGPASSWORD={Constants.DatabaseUserPwd} psql -h localhost " +
                $"-d {Constants.DatabaseName} -U{Constants.DatabaseUserName} < {sqlFile}")
                .ToString();

            var result = _dockerCli.Exec(DbServerContainerName, "/bin/sh", new[] { "-c", dbSetupScript });
            RunAsserts(() => Assert.True(result.IsSuccess), result.GetDebugInfo());
        }
    }
}
