// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

namespace Oryx.BuildImage.Tests
{
    //public class EndToEndTestsWithDatabase
    //{
    //    private const int HostPort = 9095;
    //    private const string startupCommand = "/opt/startupcmdgen/startupcmdgen";

    //    private readonly ITestOutputHelper _output;
    //    private readonly string _hostSamplesDir;
    //    private readonly HttpClient _httpClient;

    //    public EndToEndTestsWithDatabase(ITestOutputHelper output)
    //    {
    //        _output = output;
    //        _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
    //        _httpClient = new HttpClient();
    //    }

    //    [Fact]
    //    public async Task Python36App_SqlLiteDB()
    //    {
    //        await TestWith_SqlLiteDBAsync("3.6");
    //    }

    //    [Fact]
    //    public async Task Python37App_SqlLiteDB()
    //    {
    //        await TestWith_SqlLiteDBAsync("3.7");
    //    }

    //    [Fact]
    //    public async Task Python36App_PostgreSqlDB()
    //    {
    //        await TestWith_PostgresDBAsync("3.6");
    //    }

    //    [Fact]
    //    public async Task Python37App_PostgreSqlDB()
    //    {
    //        await TestWith_PostgresDBAsync("3.7");
    //    }

    //    [Fact]
    //    public async Task Python36App_MySqlDB()
    //    {
    //        await TestWith_MySqlDBAsync("3.6");
    //    }

    //    [Fact]
    //    public async Task Python37App_MySqlDB()
    //    {
    //        await TestWith_MySqlDBAsync("3.7");
    //    }

    //    [Fact]
    //    public async Task Python36App_MicrosoftSqlServerDB()
    //    {
    //        await TestWith_MicrosoftSqlServerDBAsync("3.6");
    //    }

    //    [Fact]
    //    public async Task Python37App_MicrosoftSqlServerDB()
    //    {
    //        await TestWith_MicrosoftSqlServerDBAsync("3.7");
    //    }

    //    private async Task TestWith_SqlLiteDBAsync(string pythonVersion)
    //    {
    //        // Arrange
    //        var hostDir = Path.Combine(_hostSamplesDir, "python", "microblog");
    //        var volume = DockerVolume.Create(hostDir);
    //        var appDir = volume.ContainerDir;
    //        var portMapping = $"{HostPort}:5000";
    //        var entryPointFile = "./entryPoint.sh";
    //        var entrypointScript = "./start.sh";
    //        var entryPointGenCmd = $"/opt/startupcmdgen/startupcmdgen -userStartupCommand=\"{entryPointFile}\" -output {entrypointScript}";
    //        var script = new ShellScriptBuilder()
    //            .AddCommand($"cd {appDir}")
    //            .SetExecutePermissionOnFile(entryPointFile)
    //            .AddCommand(entryPointGenCmd)
    //            .AddCommand(entrypointScript)
    //            .ToString();

    //        await EndToEndTestHelper.BuildRunAndAssertAppAsync(
    //            _output,
    //            volume,
    //            "oryx",
    //            new[] { "build", appDir, "-l", "python", "--language-version", pythonVersion },
    //            $"oryxdevms/python-{pythonVersion}",
    //            portMapping,
    //            "/bin/bash",
    //            new[]
    //            {
    //                "-c",
    //                script
    //            },
    //            async () =>
    //            {
    //                // Make sure SQLite database is used
    //                var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
    //                Assert.Contains("Microblog", data);
    //            });
    //    }

    //    private async Task TestWith_MySqlDBAsync(string pythonVersion)
    //    {
    //        // Arrange
    //        var hostDir = Path.Combine(_hostSamplesDir, "python", "microblog");
    //        var volume = DockerVolume.Create(hostDir);
    //        var appDir = volume.ContainerDir;
    //        var portMapping = $"{HostPort}:5000";
    //        var entryPointFile = "./entryPoint.sh";
    //        var entrypointScript = "./start.sh";
    //        var entryPointGenCmd = $"/opt/startupcmdgen/startupcmdgen -userStartupCommand=\"{entryPointFile}\" -output {entrypointScript}";
    //        var script = new ShellScriptBuilder()
    //            .AddCommand($"cd {appDir}")
    //            .SetExecutePermissionOnFile(entryPointFile)
    //            .AddCommand(entryPointGenCmd)
    //            .AddCommand(entrypointScript)
    //            .ToString();

    //        var dockerCli = new DockerCli((int)TimeSpan.FromMinutes(10).TotalSeconds);
    //        DockerRunCommandResult runDatabaseContainerResult = null;
    //        try
    //        {
    //            var internalDbLinkName = "dbserver";
    //            var databaseName = "microblog";
    //            var databaseUserName = "microblog";
    //            var databaseUserPwd = "Passw0rd!";
    //            runDatabaseContainerResult = dockerCli.Run(
    //                Settings.MySqlDbImageName,
    //                environmentVariables: new List<EnvironmentVariable>
    //                {
    //                    new EnvironmentVariable("MYSQL_RANDOM_ROOT_PASSWORD", "yes"),
    //                    new EnvironmentVariable("MYSQL_DATABASE", databaseName),
    //                    new EnvironmentVariable("MYSQL_USER", databaseUserName),
    //                    new EnvironmentVariable("MYSQL_PASSWORD", databaseUserPwd),
    //                },
    //                volumes: null,
    //                portMapping: null,
    //                link: null,
    //                runContainerInBackground: true,
    //                command: null,
    //                commandArguments: null);

    //            RunAsserts(
    //               () =>
    //               {
    //                   Assert.True(runDatabaseContainerResult.IsSuccess);
    //               },
    //               runDatabaseContainerResult.GetDebugInfo());

    //            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
    //                _output,
    //                volume,
    //                "oryx",
    //                new[] { "build", appDir, "-l", "python", "--language-version", pythonVersion },
    //                $"oryxdevms/python-{pythonVersion}",
    //                new List<EnvironmentVariable>
    //                {
    //                    new EnvironmentVariable(
    //                        "DATABASE_URL",
    //                        $"mysql+pymysql://{databaseUserName}:{databaseUserPwd}@{internalDbLinkName}/{databaseName}")
    //                },
    //                portMapping,
    //                link: $"{runDatabaseContainerResult.ContainerName}:{internalDbLinkName}",
    //                "/bin/bash",
    //                new[]
    //                {
    //                "-c",
    //                script
    //                },
    //                async () =>
    //                {
    //                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
    //                    Assert.Contains("Microblog", data);
    //                });
    //        }
    //        finally
    //        {
    //            if (runDatabaseContainerResult != null)
    //            {
    //                dockerCli.StopContainer(runDatabaseContainerResult.ContainerName);
    //            }
    //        }
    //    }

    //    private async Task TestWith_PostgresDBAsync(string pythonVersion)
    //    {
    //        // Arrange
    //        var hostDir = Path.Combine(_hostSamplesDir, "python", "microblog");
    //        var volume = DockerVolume.Create(hostDir);
    //        var appDir = volume.ContainerDir;
    //        var portMapping = $"{HostPort}:5000";
    //        var entryPointFile = "./entryPoint.sh";
    //        var entrypointScript = "./start.sh";
    //        var entryPointGenCmd = $"/opt/startupcmdgen/startupcmdgen -userStartupCommand=\"{entryPointFile}\" -output {entrypointScript}";
    //        var script = new ShellScriptBuilder()
    //            .AddCommand($"cd {appDir}")
    //            .SetExecutePermissionOnFile(entryPointFile)
    //            .AddCommand(entryPointGenCmd)
    //            .AddCommand(entrypointScript)
    //            .ToString();

    //        var dockerCli = new DockerCli((int)TimeSpan.FromMinutes(10).TotalSeconds);
    //        DockerRunCommandResult runDatabaseContainerResult = null;
    //        try
    //        {
    //            var internalDbLinkName = "dbserver";
    //            var databaseName = "microblog";
    //            var databaseUserName = "microblog";
    //            var databaseUserPwd = "Passw0rd!";

    //            runDatabaseContainerResult = dockerCli.Run(
    //                Settings.PostgresDbImageName,
    //                environmentVariables: new List<EnvironmentVariable>
    //                {
    //                    new EnvironmentVariable("POSTGRES_DB", databaseName),
    //                    new EnvironmentVariable("POSTGRES_USER", databaseUserName),
    //                    new EnvironmentVariable("POSTGRES_PASSWORD", databaseUserPwd),
    //                },
    //                volumes: null,
    //                portMapping: null,
    //                link: null,
    //                runContainerInBackground: true,
    //                command: null,
    //                commandArguments: null);

    //            RunAsserts(
    //               () =>
    //               {
    //                   Assert.True(runDatabaseContainerResult.IsSuccess);
    //               },
    //               runDatabaseContainerResult.GetDebugInfo());

    //            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
    //                _output,
    //                volume,
    //                "oryx",
    //                new[] { "build", appDir, "-l", "python", "--language-version", pythonVersion },
    //                $"oryxdevms/python-{pythonVersion}",
    //                new List<EnvironmentVariable>
    //                {
    //                    new EnvironmentVariable(
    //                        "DATABASE_URL",
    //                        $"postgresql+psycopg2://{databaseUserName}:{databaseUserPwd}@{internalDbLinkName}/{databaseName}")
    //                },
    //                portMapping,
    //                link: $"{runDatabaseContainerResult.ContainerName}:{internalDbLinkName}",
    //                "/bin/bash",
    //                new[]
    //                {
    //                "-c",
    //                script
    //                },
    //                async () =>
    //                {
    //                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
    //                    Assert.Contains("Microblog", data);
    //                });
    //        }
    //        finally
    //        {
    //            if (runDatabaseContainerResult != null)
    //            {
    //                dockerCli.StopContainer(runDatabaseContainerResult.ContainerName);
    //            }
    //        }
    //    }

    //    private async Task TestWith_MicrosoftSqlServerDBAsync(string pythonVersion)
    //    {
    //        // Arrange
    //        var hostDir = Path.Combine(_hostSamplesDir, "python", "microblog");
    //        var volume = DockerVolume.Create(hostDir);
    //        var appDir = volume.ContainerDir;
    //        var portMapping = $"{HostPort}:5000";
    //        var entrypointScript = "./start.sh";
    //        var entryPointFile = "./entryPoint.sh";
    //        var entryPointGenCmd = $"/opt/startupcmdgen/startupcmdgen -userStartupCommand=\"{entryPointFile}\" -output {entrypointScript}";
    //        var script = new ShellScriptBuilder()
    //            .AddCommand($"cd {appDir}")
    //            .SetExecutePermissionOnFile(entryPointFile)
    //            .AddCommand(entryPointGenCmd)
    //            .AddCommand(entrypointScript)
    //            .ToString();

    //        var dockerCli = new DockerCli((int)TimeSpan.FromMinutes(10).TotalSeconds);
    //        DockerRunCommandResult runDatabaseContainerResult = null;
    //        try
    //        {
    //            var internalDbLinkName = "dbserver";
    //            var databaseName = "microblog";
    //            var databaseUserName = "microblog";
    //            var databaseUserPwd = "Passw0rd!";

    //            // Start database container
    //            runDatabaseContainerResult = dockerCli.Run(
    //                Settings.MicrosoftSQLServerImageName,
    //                environmentVariables: new List<EnvironmentVariable>
    //                {
    //                    new EnvironmentVariable("ACCEPT_EULA", "Y"),
    //                    new EnvironmentVariable("SA_PASSWORD", databaseUserPwd),
    //                },
    //                volumes: null,
    //                portMapping: null,
    //                link: null,
    //                runContainerInBackground: true,
    //                command: null,
    //                commandArguments: null);

    //            RunAsserts(
    //               () =>
    //               {
    //                   Assert.True(runDatabaseContainerResult.IsSuccess);
    //               },
    //               runDatabaseContainerResult.GetDebugInfo());

    //            // Wait for database container to be up
    //            await Task.Delay(TimeSpan.FromSeconds(60));

    //            // Setup user, database
    //            var dbSetupSql = "/tmp/databaseSetup.sql";
    //            var databaseSetupScript = new ShellScriptBuilder()
    //                .AddCommand($"echo \"CREATE LOGIN {databaseUserName} WITH PASSWORD = '{databaseUserPwd}';\" > {dbSetupSql}")
    //                .AddCommand($"echo GO >> {dbSetupSql}")
    //                .AddCommand($"echo \"CREATE DATABASE {databaseName};\" >> {dbSetupSql}")
    //                .AddCommand($"echo GO >> {dbSetupSql}")
    //                .AddCommand($"echo \"Use {databaseName};\" >> {dbSetupSql}")
    //                .AddCommand($"echo GO >> {dbSetupSql}")
    //                .AddCommand($"echo CREATE USER [{databaseName}] FOR LOGIN [{databaseName}] >> {dbSetupSql}")
    //                .AddCommand($"echo \"EXEC sp_addrolemember N'db_owner', N'{databaseName}'\" >> {dbSetupSql}")
    //                .AddCommand($"echo GO >> {dbSetupSql}")
    //                .AddCommand($"/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P {databaseUserPwd} -i {dbSetupSql}")
    //                .ToString();

    //            var setupDatabaseResult = dockerCli.Exec(
    //                runDatabaseContainerResult.ContainerName,
    //                "/bin/bash",
    //                new[]
    //                {
    //                    "-c",
    //                    databaseSetupScript
    //                });

    //            RunAsserts(
    //               () =>
    //               {
    //                   Assert.True(setupDatabaseResult.IsSuccess);
    //               },
    //               setupDatabaseResult.GetDebugInfo());

    //            // Act & Assert
    //            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
    //                _output,
    //                volume,
    //                "oryx",
    //                new[] { "build", appDir, "-l", "python", "--language-version", pythonVersion },
    //                $"oryxdevms/python-{pythonVersion}",
    //                new List<EnvironmentVariable>
    //                {
    //                    new EnvironmentVariable(
    //                        "DATABASE_URL",
    //                        $"mssql+pyodbc://{databaseUserName}:{databaseUserPwd}@{internalDbLinkName}/{databaseName}?driver=ODBC+Driver+17+for+SQL+Server")
    //                },
    //                portMapping,
    //                link: $"{runDatabaseContainerResult.ContainerName}:{internalDbLinkName}",
    //                "/bin/bash",
    //                new[]
    //                {
    //                "-c",
    //                script
    //                },
    //                async () =>
    //                {
    //                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
    //                    Assert.Contains("Microblog", data);
    //                });
    //        }
    //        finally
    //        {
    //            if (runDatabaseContainerResult != null)
    //            {
    //                dockerCli.StopContainer(runDatabaseContainerResult.ContainerName);
    //            }
    //        }
    //    }

    //    private void RunAsserts(Action action, string message)
    //    {
    //        try
    //        {
    //            action();
    //        }
    //        catch (Exception)
    //        {
    //            _output.WriteLine(message);
    //            throw;
    //        }
    //    }
    //}
}