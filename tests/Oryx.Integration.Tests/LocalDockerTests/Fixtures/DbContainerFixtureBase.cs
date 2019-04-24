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
        public const string DbServerHostnameEnvVarName = "DATABASE_HOST";
        public const string DbServerUsernameEnvVarName = "DATABASE_USERNAME";
        public const string DbServerPasswordEnvVarName = "DATABASE_PASSWORD";
        public const string DbServerDatabaseEnvVarName = "DATABASE_NAME";

        protected readonly IList<ProductRecord> SampleData = new List<ProductRecord> {
            new ProductRecord { Name = "Car" },
            new ProductRecord { Name = "Camera" },
            new ProductRecord { Name = "Computer" }
        };

        protected readonly DockerCli _dockerCli = new DockerCli();

        public DbContainerFixtureBase()
        {
            DbServerContainerName = RunDbServerContainer().ContainerName;
            if (!WaitUntilDbServerIsUp())
            {
                throw new Exception("Database not ready in time");
            }
            InsertSampleData();
        }

        public string DbServerContainerName { get; }

        protected virtual void StopContainer()
        {
            if (!string.IsNullOrEmpty(DbServerContainerName))
            {
                _dockerCli.StopContainer(DbServerContainerName);
            }
        }

        public void Dispose()
        {
            StopContainer();
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

        protected abstract bool WaitUntilDbServerIsUp();

        protected virtual string GetSampleDataInsertionSql()
        {
            var sb = new StringBuilder($"USE {Constants.DatabaseName}; CREATE TABLE Products (Name varchar(50) NOT NULL);");
            foreach (var record in SampleData)
            {
                sb.Append($" INSERT INTO Products VALUES('{record.Name}');");
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

    public class ProductRecord
    {
        public string Name;
    }
}
