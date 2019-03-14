// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using JetBrains.Annotations;
using Microsoft.Oryx.Tests.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Microsoft.Oryx.Integration.Tests.LocalDockerTests.Fixtures
{
    public abstract class DbContainerFixtureBase : IDisposable
    {
        public const string DbServerHostnameEnvVarName = "DATABASE_HOSTNAME";
        public const string DbServerUsernameEnvVarName = "DATABASE_USERNAME";
        public const string DbServerPasswordEnvVarName = "DATABASE_PASSWORD";
        public const string DbServerDatabaseEnvVarName = "DATABASE_NAME";

        protected readonly IList<KeyValuePair<string, string>> SampleData = new List<KeyValuePair<string, string>> {
            KeyValuePair.Create("name", "Car"),
            KeyValuePair.Create("name", "Camera"),
            KeyValuePair.Create("name", "Computer")
        };

        protected readonly DockerCli _dockerCli = new DockerCli();

        public DbContainerFixtureBase()
        {
            DbServerContainerName = RunDbServerContainer().ContainerName;
            WaitUntilDbServerIsUp();
            InsertSampleData();
        }

        public string DbServerContainerName { get; }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(DbServerContainerName))
            {
                _dockerCli.StopContainer(DbServerContainerName);
            }
        }

        [NotNull]
        public virtual List<EnvironmentVariable> GetCredentialsAsEnvVars()
        {
            return new List<EnvironmentVariable>
            {
                new EnvironmentVariable(DbServerHostnameEnvVarName, Constants.InternalDbLinkName),
                new EnvironmentVariable(DbServerUsernameEnvVarName, Constants.DatabaseUserName),
                new EnvironmentVariable(DbServerPasswordEnvVarName, Constants.DatabaseUserPwd),
                new EnvironmentVariable(DbServerDatabaseEnvVarName, Constants.DatabaseName),
            };
        }

        public string GetSampleDataAsJson()
        {
            return JsonConvert.SerializeObject(SampleData);
        }

        protected abstract DockerRunCommandResult RunDbServerContainer();

        protected virtual void WaitUntilDbServerIsUp()
        {
            // TODO: get rid of Sleep
            Thread.Sleep(TimeSpan.FromMinutes(1));
        }

        protected virtual string GetSampleDataInsertionSql()
        {
            var sb = new StringBuilder($"USE {Constants.DatabaseName}; CREATE TABLE Products (Name varchar(50) NOT NULL);");
            foreach (var record in SampleData)
            {
                sb.Append($" INSERT INTO Products VALUES('{record.Value}');");
            }
            return sb.ToString();
        }

        protected abstract void InsertSampleData();

        protected void RunAsserts(Action action, string message)
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
