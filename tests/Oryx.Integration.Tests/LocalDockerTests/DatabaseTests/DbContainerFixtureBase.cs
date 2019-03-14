// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Microsoft.Oryx.Integration.Tests.LocalDockerTests.DatabaseTests
{
    abstract class DbContainerFixtureBase : IDisposable
    {
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
