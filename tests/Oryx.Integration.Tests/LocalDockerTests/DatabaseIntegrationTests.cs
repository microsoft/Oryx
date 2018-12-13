// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.BuildImage.Tests.LocalDockerTests
{
    public class DatabaseIntegrationTests
    {
        private const int HostPort = 9095;
        private const string startupCommand = "/opt/startupcmdgen/startupcmdgen";

        private readonly ITestOutputHelper _output;
        private readonly string _hostSamplesDir;
        private readonly HttpClient _httpClient;

        public DatabaseIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
            _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
            _httpClient = new HttpClient();
        }

        [Fact]
        public async Task NodeApp_MySqlDB()
        {
            await NodeApp_MySqlDBAsync("10.14");
        }

        [Fact]
        public async Task NodeApp_PostgresDB()
        {
            await NodeApp_PostgresDBAsync("10.14");
        }

        [Fact]
        public async Task NodeApp_MicrosoftSqlServerDB()
        {
            await NodeApp_MicrosoftSqlServerDBAsync("10.14");
        }

        [Fact]
        public async Task Python36App_SqlLiteDB()
        {
            await PythonApp_SqlLiteDBAsync("3.6");
        }

        [Fact]
        public async Task Python37App_SqlLiteDB()
        {
            await PythonApp_SqlLiteDBAsync("3.7");
        }

        [Fact]
        public async Task Python36App_PostgreSqlDB()
        {
            await PythonApp_PostgresDBAsync("3.6");
        }

        [Fact]
        public async Task Python37App_PostgreSqlDB()
        {
            await PythonApp_PostgresDBAsync("3.7");
        }

        [Fact]
        public async Task Python36App_MySqlDB()
        {
            await PythonApp_MySqlDBAsync("3.6");
        }

        [Fact]
        public async Task Python37App_MySqlDB()
        {
            await PythonApp_MySqlDBAsync("3.7");
        }

        [Fact]
        public async Task Python36App_MicrosoftSqlServerDB()
        {
            await PythonApp_MicrosoftSqlServerDBAsync("3.6");
        }

        [Fact]
        public async Task Python37App_MicrosoftSqlServerDB()
        {
            await PythonApp_MicrosoftSqlServerDBAsync("3.7");
        }

        [Fact]
        public async Task Python37App_MySqlDB_UsingMySqlClient()
        {
            await PythonApp_MySqlDB_UsingMySqlClientAsync("3.7");
        }

        private async Task NodeApp_MySqlDBAsync(string nodeVersion)
        {
            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", "node-mysql");
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:5000";
            var startupScriptFile = "/tmp/startup.sh";
            var startupScript = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"{startupCommand} -appPath {appDir} -output {startupScriptFile}")
                .AddCommand(startupScriptFile)
                .ToString();

            var dockerCli = new DockerCli((int)TimeSpan.FromMinutes(10).TotalSeconds);
            DockerRunCommandResult runDatabaseContainerResult = null;
            try
            {
                var internalDbLinkName = "dbserver";
                var databaseName = "oryxdb";
                var databaseUserName = "oryxuser";
                var databaseUserPwd = "Passw0rd";
                runDatabaseContainerResult = dockerCli.Run(
                    Settings.MySqlDbImageName,
                    environmentVariables: new List<EnvironmentVariable>
                    {
                        new EnvironmentVariable("MYSQL_RANDOM_ROOT_PASSWORD", "yes"),
                        new EnvironmentVariable("MYSQL_DATABASE", databaseName),
                        new EnvironmentVariable("MYSQL_USER", databaseUserName),
                        new EnvironmentVariable("MYSQL_PASSWORD", databaseUserPwd),
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

                await Task.Delay(TimeSpan.FromMinutes(1));

                // Setup user, database
                var dbSetupSql = "/tmp/databaseSetup.sql";
                var databaseSetupScript = new ShellScriptBuilder()
                    .AddCommand($"echo \"USE {databaseName};\" > {dbSetupSql}")
                    .AddCommand($"echo \"CREATE TABLE players (id int(5) NOT NULL AUTO_INCREMENT, " +
                    "first_name varchar(255) NOT NULL, last_name varchar(255) NOT NULL, position varchar(255)" +
                    " NOT NULL, number int(11) NOT NULL, user_name varchar(20) NOT NULL, PRIMARY KEY (\"id\")) " +
                    $"ENGINE=InnoDB DEFAULT CHARSET=latin1 AUTO_INCREMENT=1;\" >> {dbSetupSql}")
                    .AddCommand($"mysql -u {databaseUserName} -p{databaseUserPwd} < {dbSetupSql}")
                    .ToString();

                var setupDatabaseResult = dockerCli.Exec(
                    runDatabaseContainerResult.ContainerName,
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

                await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                    _output,
                    volume,
                    "oryx",
                    new[] { "build", appDir, "-l", "nodejs", "--language-version", nodeVersion },
                    $"oryxdevms/node-{nodeVersion}",
                    new List<EnvironmentVariable>(),
                    portMapping,
                    link: $"{runDatabaseContainerResult.ContainerName}:{internalDbLinkName}",
                    "/bin/sh",
                    new[]
                    {
                    "-c",
                    startupScript
                    },
                    async () =>
                    {
                        // Add a new player
                        var formData = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("first_name", "James"),
                            new KeyValuePair<string, string>("last_name", "Harden"),
                            new KeyValuePair<string, string>("username", "jharden"),
                            new KeyValuePair<string, string>("number", "10"),
                            new KeyValuePair<string, string>("position", "Striker")
                        };
                        var responseMessage = await _httpClient.PostAsync(
                            $"http://localhost:{HostPort}/add",
                            new FormUrlEncodedContent(formData));
                        var data = await responseMessage.Content.ReadAsStringAsync();
                        Assert.True(responseMessage.StatusCode == HttpStatusCode.OK, data);

                        data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                        Assert.Contains("James", data);
                    });
            }
            finally
            {
                if (runDatabaseContainerResult != null)
                {
                    dockerCli.StopContainer(runDatabaseContainerResult.ContainerName);
                }
            }
        }

        private async Task NodeApp_PostgresDBAsync(string nodeVersion)
        {
            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", "node-postgres");
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:5000";
            var startupScriptFile = "/tmp/startup.sh";
            var startupScript = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"{startupCommand} -appPath {appDir} -output {startupScriptFile}")
                .AddCommand(startupScriptFile)
                .ToString();

            var dockerCli = new DockerCli((int)TimeSpan.FromMinutes(10).TotalSeconds);
            DockerRunCommandResult runDatabaseContainerResult = null;
            try
            {
                var internalDbLinkName = "dbserver";
                var databaseName = "oryxdb";
                var databaseUserName = "oryxuser";
                var databaseUserPwd = "Passw0rd";
                runDatabaseContainerResult = dockerCli.Run(
                    Settings.PostgresDbImageName,
                    environmentVariables: new List<EnvironmentVariable>
                    {
                        new EnvironmentVariable("POSTGRES_DB", databaseName),
                        new EnvironmentVariable("POSTGRES_USER", databaseUserName),
                        new EnvironmentVariable("POSTGRES_PASSWORD", databaseUserPwd),
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

                await Task.Delay(TimeSpan.FromMinutes(1));

                // Setup user, database
                var dbSetupSql = "/tmp/databaseSetup.sql";
                var databaseSetupScript = new ShellScriptBuilder()
                    .AddCommand($"echo \"PGPASSWORD={databaseUserPwd}\" > {dbSetupSql}")
                    .AddCommand($"echo \"USE {databaseName};\" >> {dbSetupSql}")
                    .AddCommand($"echo \"CREATE TABLE numbers(age integer);\" >> {dbSetupSql}")
                    .AddCommand($"echo \"INSERT INTO numbers VALUES (732);\" >> {dbSetupSql}")
                    .AddCommand($"psql -h localhost -d {databaseName} -U{databaseUserName} < {dbSetupSql}")
                    .ToString();

                var setupDatabaseResult = dockerCli.Exec(
                    runDatabaseContainerResult.ContainerName,
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

                await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                    _output,
                    volume,
                    "oryx",
                    new[] { "build", appDir, "-l", "nodejs", "--language-version", nodeVersion },
                    $"oryxdevms/node-{nodeVersion}",
                    new List<EnvironmentVariable>(),
                    portMapping,
                    link: $"{runDatabaseContainerResult.ContainerName}:{internalDbLinkName}",
                    "/bin/sh",
                    new[]
                    {
                    "-c",
                    startupScript
                    },
                    async () =>
                    {
                        var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                        Assert.Contains("732", data);
                    });
            }
            finally
            {
                if (runDatabaseContainerResult != null)
                {
                    dockerCli.StopContainer(runDatabaseContainerResult.ContainerName);
                }
            }
        }

        private async Task NodeApp_MicrosoftSqlServerDBAsync(string nodeVersion)
        {
            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", "node-mssql");
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:5000";
            var startupScriptFile = "/tmp/startup.sh";
            var startupScript = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"{startupCommand} -appPath {appDir} -output {startupScriptFile}")
                .AddCommand(startupScriptFile)
                .ToString();

            var dockerCli = new DockerCli((int)TimeSpan.FromMinutes(10).TotalSeconds);
            DockerRunCommandResult runDatabaseContainerResult = null;
            try
            {
                var internalDbLinkName = "dbserver";
                var databaseName = "oryxdb";
                var databaseUserPwd = "Passw0rd";

                // Start database container
                runDatabaseContainerResult = dockerCli.Run(
                    Settings.MicrosoftSQLServerImageName,
                    environmentVariables: new List<EnvironmentVariable>
                    {
                        new EnvironmentVariable("ACCEPT_EULA", "Y"),
                        new EnvironmentVariable("SA_PASSWORD", databaseUserPwd),
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

                // Wait for database container to be up
                await Task.Delay(TimeSpan.FromSeconds(60));

                // Setup user, database
                var dbSetupSql = "/tmp/databaseSetup.sql";
                var databaseSetupScript = new ShellScriptBuilder()
                    .AddCommand($"echo \"CREATE DATABASE {databaseName};\" >> {dbSetupSql}")
                    .AddCommand($"echo GO >> {dbSetupSql}")
                    .AddCommand($"echo \"Use {databaseName};\" >> {dbSetupSql}")
                    .AddCommand($"echo GO >> {dbSetupSql}")
                    .AddCommand($"echo \"CREATE TABLE Products (ID int, ProductName nvarchar(max));\" >> {dbSetupSql}")
                    .AddCommand($"echo GO >> {dbSetupSql}")
                    .AddCommand($"echo \"INSERT INTO Products VALUES (1, 'Car');\" >> {dbSetupSql}")
                    .AddCommand($"echo GO >> {dbSetupSql}")
                    .AddCommand($"/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P {databaseUserPwd} -i {dbSetupSql}")
                    .ToString();

                var setupDatabaseResult = dockerCli.Exec(
                    runDatabaseContainerResult.ContainerName,
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

                // Act & Assert
                await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                    _output,
                    volume,
                    "oryx",
                    new[] { "build", appDir, "-l", "nodejs", "--language-version", nodeVersion },
                    $"oryxdevms/node-{nodeVersion}",
                    new List<EnvironmentVariable>(),
                    portMapping,
                    link: $"{runDatabaseContainerResult.ContainerName}:{internalDbLinkName}",
                    "/bin/sh",
                    new[]
                    {
                    "-c",
                    startupScript
                    },
                    async () =>
                    {
                        var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                        Assert.Contains("Car", data);
                    });
            }
            finally
            {
                if (runDatabaseContainerResult != null)
                {
                    dockerCli.StopContainer(runDatabaseContainerResult.ContainerName);
                }
            }
        }

        private async Task PythonApp_SqlLiteDBAsync(string pythonVersion)
        {
            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "python", "microblog");
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:5000";
            var entryPointFile = "./entryPoint.sh";
            var entrypointScript = "./start.sh";
            var entryPointGenCmd = $"{startupCommand} -userStartupCommand=\"{entryPointFile}\" -output {entrypointScript}";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .SetExecutePermissionOnFile(entryPointFile)
                .AddCommand(entryPointGenCmd)
                .AddCommand(entrypointScript)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                _output,
                volume,
                "oryx",
                new[] { "build", appDir, "-l", "python", "--language-version", pythonVersion },
                $"oryxdevms/python-{pythonVersion}",
                portMapping,
                "/bin/bash",
                new[]
                {
                        "-c",
                        script
                },
                async () =>
                {
                    // Make sure SQLite database is used
                    var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                    Assert.Contains("Microblog", data);
                });
        }

        private async Task PythonApp_MySqlDBAsync(string pythonVersion)
        {
            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "python", "microblog");
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:5000";
            var entryPointFile = "./entryPoint.sh";
            var entrypointScript = "./start.sh";
            var entryPointGenCmd = $"{startupCommand} -userStartupCommand=\"{entryPointFile}\" -output {entrypointScript}";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .SetExecutePermissionOnFile(entryPointFile)
                .AddCommand(entryPointGenCmd)
                .AddCommand(entrypointScript)
                .ToString();

            var dockerCli = new DockerCli((int)TimeSpan.FromMinutes(10).TotalSeconds);
            DockerRunCommandResult runDatabaseContainerResult = null;
            try
            {
                var internalDbLinkName = "dbserver";
                var databaseName = "microblog";
                var databaseUserName = "microblog";
                var databaseUserPwd = "Passw0rd!";
                runDatabaseContainerResult = dockerCli.Run(
                    Settings.MySqlDbImageName,
                    environmentVariables: new List<EnvironmentVariable>
                    {
                            new EnvironmentVariable("MYSQL_RANDOM_ROOT_PASSWORD", "yes"),
                            new EnvironmentVariable("MYSQL_DATABASE", databaseName),
                            new EnvironmentVariable("MYSQL_USER", databaseUserName),
                            new EnvironmentVariable("MYSQL_PASSWORD", databaseUserPwd),
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

                await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                    _output,
                    volume,
                    "oryx",
                    new[] { "build", appDir, "-l", "python", "--language-version", pythonVersion },
                    $"oryxdevms/python-{pythonVersion}",
                    new List<EnvironmentVariable>
                    {
                            new EnvironmentVariable(
                                "DATABASE_URL",
                                $"mysql+pymysql://{databaseUserName}:{databaseUserPwd}@{internalDbLinkName}/{databaseName}")
                    },
                    portMapping,
                    link: $"{runDatabaseContainerResult.ContainerName}:{internalDbLinkName}",
                    "/bin/bash",
                    new[]
                    {
                        "-c",
                        script
                    },
                    async () =>
                    {
                        var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                        Assert.Contains("Microblog", data);
                    });
            }
            finally
            {
                if (runDatabaseContainerResult != null)
                {
                    dockerCli.StopContainer(runDatabaseContainerResult.ContainerName);
                }
            }
        }

        private async Task PythonApp_MySqlDB_UsingMySqlClientAsync(string pythonVersion)
        {
            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "python", "mysqlclient-sample");
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:8000";
            var entrypointScript = "./start.sh";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"{startupCommand} -appPath {appDir} -output {entrypointScript}")
                .AddCommand(entrypointScript)
                .ToString();

            var dockerCli = new DockerCli((int)TimeSpan.FromMinutes(10).TotalSeconds);
            DockerRunCommandResult runDatabaseContainerResult = null;
            try
            {
                var internalDbLinkName = "dbserver";
                var databaseName = "oryxdb";
                var databaseUserName = "oryxuser";
                var databaseUserPwd = "Passw0rd";
                runDatabaseContainerResult = dockerCli.Run(
                    Settings.MySqlDbImageName,
                    environmentVariables: new List<EnvironmentVariable>
                    {
                            new EnvironmentVariable("MYSQL_RANDOM_ROOT_PASSWORD", "yes"),
                            new EnvironmentVariable("MYSQL_DATABASE", databaseName),
                            new EnvironmentVariable("MYSQL_USER", databaseUserName),
                            new EnvironmentVariable("MYSQL_PASSWORD", databaseUserPwd),
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

                await Task.Delay(TimeSpan.FromMinutes(1));

                // Setup user, database
                var dbSetupSql = "/tmp/databaseSetup.sql";
                var databaseSetupScript = new ShellScriptBuilder()
                    .AddCommand($"echo \"USE {databaseName};\" > {dbSetupSql}")
                    .AddCommand($"echo \"CREATE TABLE Products (Name varchar(255) NOT NULL);\" >> {dbSetupSql}")
                    .AddCommand($"echo \"INSERT INTO Products VALUES ('Lamp');\" >> {dbSetupSql}")
                    .AddCommand($"mysql -u {databaseUserName} -p{databaseUserPwd} < {dbSetupSql}")
                    .ToString();

                var setupDatabaseResult = dockerCli.Exec(
                    runDatabaseContainerResult.ContainerName,
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

                await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                    _output,
                    volume,
                    "oryx",
                    new[] { "build", appDir, "-l", "python", "--language-version", pythonVersion },
                    $"oryxdevms/python-{pythonVersion}",
                    new List<EnvironmentVariable>(),
                    portMapping,
                    link: $"{runDatabaseContainerResult.ContainerName}:{internalDbLinkName}",
                    "/bin/bash",
                    new[]
                    {
                        "-c",
                        script
                    },
                    async () =>
                    {
                        var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                        _output.WriteLine(data);
                        Assert.Contains("Lamp", data);
                    });
            }
            finally
            {
                if (runDatabaseContainerResult != null)
                {
                    dockerCli.StopContainer(runDatabaseContainerResult.ContainerName);
                }
            }
        }

        private async Task PythonApp_PostgresDBAsync(string pythonVersion)
        {
            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "python", "microblog");
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:5000";
            var entryPointFile = "./entryPoint.sh";
            var entrypointScript = "./start.sh";
            var entryPointGenCmd = $"{startupCommand} -userStartupCommand=\"{entryPointFile}\" -output {entrypointScript}";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .SetExecutePermissionOnFile(entryPointFile)
                .AddCommand(entryPointGenCmd)
                .AddCommand(entrypointScript)
                .ToString();

            var dockerCli = new DockerCli((int)TimeSpan.FromMinutes(10).TotalSeconds);
            DockerRunCommandResult runDatabaseContainerResult = null;
            try
            {
                var internalDbLinkName = "dbserver";
                var databaseName = "microblog";
                var databaseUserName = "microblog";
                var databaseUserPwd = "Passw0rd!";

                runDatabaseContainerResult = dockerCli.Run(
                    Settings.PostgresDbImageName,
                    environmentVariables: new List<EnvironmentVariable>
                    {
                            new EnvironmentVariable("POSTGRES_DB", databaseName),
                            new EnvironmentVariable("POSTGRES_USER", databaseUserName),
                            new EnvironmentVariable("POSTGRES_PASSWORD", databaseUserPwd),
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

                await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                    _output,
                    volume,
                    "oryx",
                    new[] { "build", appDir, "-l", "python", "--language-version", pythonVersion },
                    $"oryxdevms/python-{pythonVersion}",
                    new List<EnvironmentVariable>
                    {
                            new EnvironmentVariable(
                                "DATABASE_URL",
                                $"postgresql+psycopg2://{databaseUserName}:{databaseUserPwd}@{internalDbLinkName}/{databaseName}")
                    },
                    portMapping,
                    link: $"{runDatabaseContainerResult.ContainerName}:{internalDbLinkName}",
                    "/bin/bash",
                    new[]
                    {
                        "-c",
                        script
                    },
                    async () =>
                    {
                        var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                        Assert.Contains("Microblog", data);
                    });
            }
            finally
            {
                if (runDatabaseContainerResult != null)
                {
                    dockerCli.StopContainer(runDatabaseContainerResult.ContainerName);
                }
            }
        }

        private async Task PythonApp_MicrosoftSqlServerDBAsync(string pythonVersion)
        {
            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "python", "microblog");
            var volume = DockerVolume.Create(hostDir);
            var appDir = volume.ContainerDir;
            var portMapping = $"{HostPort}:5000";
            var entrypointScript = "./start.sh";
            var entryPointFile = "./entryPoint.sh";
            var entryPointGenCmd = $"{startupCommand} -userStartupCommand=\"{entryPointFile}\" -output {entrypointScript}";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .SetExecutePermissionOnFile(entryPointFile)
                .AddCommand(entryPointGenCmd)
                .AddCommand(entrypointScript)
                .ToString();

            var dockerCli = new DockerCli((int)TimeSpan.FromMinutes(10).TotalSeconds);
            DockerRunCommandResult runDatabaseContainerResult = null;
            try
            {
                var internalDbLinkName = "dbserver";
                var databaseName = "microblog";
                var databaseUserName = "microblog";
                var databaseUserPwd = "Passw0rd!";

                // Start database container
                runDatabaseContainerResult = dockerCli.Run(
                    Settings.MicrosoftSQLServerImageName,
                    environmentVariables: new List<EnvironmentVariable>
                    {
                            new EnvironmentVariable("ACCEPT_EULA", "Y"),
                            new EnvironmentVariable("SA_PASSWORD", databaseUserPwd),
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

                // Wait for database container to be up
                await Task.Delay(TimeSpan.FromSeconds(60));

                // Setup user, database
                var dbSetupSql = "/tmp/databaseSetup.sql";
                var databaseSetupScript = new ShellScriptBuilder()
                    .AddCommand($"echo \"CREATE LOGIN {databaseUserName} WITH PASSWORD = '{databaseUserPwd}';\" > {dbSetupSql}")
                    .AddCommand($"echo GO >> {dbSetupSql}")
                    .AddCommand($"echo \"CREATE DATABASE {databaseName};\" >> {dbSetupSql}")
                    .AddCommand($"echo GO >> {dbSetupSql}")
                    .AddCommand($"echo \"Use {databaseName};\" >> {dbSetupSql}")
                    .AddCommand($"echo GO >> {dbSetupSql}")
                    .AddCommand($"echo CREATE USER [{databaseName}] FOR LOGIN [{databaseName}] >> {dbSetupSql}")
                    .AddCommand($"echo \"EXEC sp_addrolemember N'db_owner', N'{databaseName}'\" >> {dbSetupSql}")
                    .AddCommand($"echo GO >> {dbSetupSql}")
                    .AddCommand($"/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P {databaseUserPwd} -i {dbSetupSql}")
                    .ToString();

                var setupDatabaseResult = dockerCli.Exec(
                    runDatabaseContainerResult.ContainerName,
                    "/bin/bash",
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

                // Act & Assert
                await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                    _output,
                    volume,
                    "oryx",
                    new[] { "build", appDir, "-l", "python", "--language-version", pythonVersion },
                    $"oryxdevms/python-{pythonVersion}",
                    new List<EnvironmentVariable>
                    {
                            new EnvironmentVariable(
                                "DATABASE_URL",
                                $"mssql+pyodbc://{databaseUserName}:{databaseUserPwd}@{internalDbLinkName}/{databaseName}?driver=ODBC+Driver+17+for+SQL+Server")
                    },
                    portMapping,
                    link: $"{runDatabaseContainerResult.ContainerName}:{internalDbLinkName}",
                    "/bin/bash",
                    new[]
                    {
                        "-c",
                        script
                    },
                    async () =>
                    {
                        var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                        Assert.Contains("Microblog", data);
                    });
            }
            finally
            {
                if (runDatabaseContainerResult != null)
                {
                    dockerCli.StopContainer(runDatabaseContainerResult.ContainerName);
                }
            }
        }

        void RunAsserts(Action action, string message)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
                _output.WriteLine(message);
                throw;
            }
        }
    }
}