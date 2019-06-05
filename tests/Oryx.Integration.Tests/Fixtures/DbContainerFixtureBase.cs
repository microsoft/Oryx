// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Oryx.Tests.Common;
using Newtonsoft.Json;

namespace Microsoft.Oryx.Integration.Tests.Fixtures
{
    public abstract class DbContainerFixtureBase : IDisposable
    {
        public const string DbServerHostnameEnvVarName = "DATABASE_HOST";
        public const string DbServerUsernameEnvVarName = "DATABASE_USERNAME";
        public const string DbServerPasswordEnvVarName = "DATABASE_PASSWORD";
        public const string DbServerDatabaseEnvVarName = "DATABASE_NAME";

        private static readonly IList<ProductRecord> SampleData = new List<ProductRecord> {
            new ProductRecord { Name = "Car" },
            new ProductRecord { Name = "Camera" },
            new ProductRecord { Name = "Computer" }
        };

        protected readonly DockerCli _dockerCli = new DockerCli();

        public DbContainerFixtureBase()
        {
            Console.WriteLine("Starting database container...");
            DbServerContainerName = RunDbServerContainer().ContainerName;
            Console.WriteLine("Done.");

            bool insertedSampleData = false;
            try
            {
                Console.WriteLine("Waiting for database server to be up...");
                var isDbServerUp = WaitUntilDbServerIsUp();
                if (isDbServerUp)
                {
                    Console.WriteLine("Database server is up.");
                    Console.WriteLine("Inserting sample data...");
                    InsertSampleData();
                    insertedSampleData = true;
                    Console.WriteLine("Done.");
                }
                else
                {
                    Console.WriteLine("Database server was not up in time.");
                }
            }
            catch (Exception ex)
            {
                // Since Dispose method does not get called in case of exceptions, catch the exception here, log it
                // and stop the container.
                Console.WriteLine("Exception occurred while setting up the database container: " + ex.ToString());
            }

            if (!insertedSampleData)
            {
                Console.WriteLine("Stopping the container...");
                StopContainer();
                Console.WriteLine("Done.");

                // Throw exception here so that tests that depend on this fixture do not run.
                throw new InvalidOperationException("Failed to setup database container for tests.");
            }
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

        public static string GetSampleDataAsJson()
        {
            return JsonConvert.SerializeObject(SampleData);
        }

        protected abstract DockerRunCommandResult RunDbServerContainer();

        protected abstract bool WaitUntilDbServerIsUp();

        protected virtual string GetSampleDataInsertionSql()
        {
            var sb = new StringBuilder(
                $"USE {Constants.DatabaseName}; CREATE TABLE Products (Name varchar(50) NOT NULL);");
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
