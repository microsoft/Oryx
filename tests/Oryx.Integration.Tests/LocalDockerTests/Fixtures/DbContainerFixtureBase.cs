// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using JetBrains.Annotations;
using Microsoft.Oryx.Tests.Common;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Oryx.Integration.Tests.LocalDockerTests.Fixtures
{
    public abstract class DbContainerFixtureBase : IDisposable
    {
        public const string DbServerHostnameEnvVarName = "DATABASE_HOST";
        public const string DbServerUsernameEnvVarName = "DATABASE_USERNAME";
        public const string DbServerPasswordEnvVarName = "DATABASE_PASSWORD";
        public const string DbServerDatabaseEnvVarName = "DATABASE_NAME";

        protected readonly DockerCli _dockerCli = new DockerCli();

        public DbContainerFixtureBase()
        {
            DbServerContainerName = RunDbServerContainer().ContainerName;

            // Wait for the database server to be up
            // TODO: get rid of Sleep
            Thread.Sleep(TimeSpan.FromMinutes(1));

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

        protected abstract DockerRunCommandResult RunDbServerContainer();

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
