// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.Tests.Infrastructure
{
    public static class EndToEndTestHelper
    {
        private const int MaxRetryCount = 10;
        private const int DelayBetweenRetriesInSeconds = 1;

        //  The sequence of steps are:
        //  1.  Copies the sample app from host machine's git repo to a different folder (under 'tmp/<reserved-name>' folder
        //      in CI machine and in the 'bin\debug\' folder in non-CI machine). The copying is necessary to not muck
        //      with git tracked samples folder and also a single sample could be used for verification in multiple tests.
        //  2.  Volume mounts the directory to the build image and build it.
        //  3.  Volume mounts the same directory to runtime image and runs the application.
        //  4.  A func supplied by the user is retried to the max of 10 retries between a delay of 1 second.
        public static async Task BuildRunAndAssertAppAsync(
            ITestOutputHelper output,
            DockerVolume volume,
            string buildCmd,
            string[] buildArgs,
            string runtimeImageName,
            string portMapping,
            string runCmd,
            string[] runArgs,
            Func<Task> assertAction)
        {
            var dockerCli = new DockerCli(waitTimeInSeconds: (int)TimeSpan.FromMinutes(10).TotalSeconds);

            // Build
            var buildAppResult = dockerCli.Run(
                Settings.BuildImageName,
                volume,
                commandToExecuteOnRun: buildCmd,
                commandArguments: buildArgs);

            RunAsserts(
               () =>
               {
                   Assert.True(buildAppResult.IsSuccess);
               },
               buildAppResult.GetDebugInfo());

            // Run
            DockerRunCommandResult runAppResult = null;
            try
            {
                runAppResult = dockerCli.Run(
                    runtimeImageName,
                    environmentVariable: null,
                    volume,
                    portMapping,
                    runContainerInBackground: true,
                    commandToExecuteOnRun: runCmd,
                    commandArguments: runArgs);

                RunAsserts(
                    () =>
                    {
                        Assert.True(runAppResult.IsSuccess);
                    },
                    runAppResult.GetDebugInfo());

                var succeeded = false;
                for (var i = 0; i < MaxRetryCount && !succeeded; i++)
                {
                    try
                    {
                        await assertAction();
                        succeeded = true;
                    }
                    catch (Exception ex) when (ex.InnerException is IOException || ex.InnerException is SocketException)
                    {
                        if (i == MaxRetryCount - 1)
                        {
                            var logsResult = dockerCli.Logs(runAppResult.ContainerName);
                            output.WriteLine("Logs from the runtime container:");
                            output.WriteLine("StdOutput:" + logsResult.Output);
                            output.WriteLine("StdOutput:" + logsResult.Error);

                            throw;
                        }
                        else
                        {
                            await Task.Delay(TimeSpan.FromSeconds(DelayBetweenRetriesInSeconds));
                        }
                    }
                }
            }
            finally
            {
                // Stop the container so that shard resources (like ports) are disposed.
                dockerCli.StopContainer(runAppResult.ContainerName);
            }

            void RunAsserts(Action action, string message)
            {
                try
                {
                    action();
                }
                catch (Exception)
                {
                    output.WriteLine(message);
                    throw;
                }
            }
        }
    }
}
