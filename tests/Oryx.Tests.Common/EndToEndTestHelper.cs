// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.Tests.Common
{
    public static class EndToEndTestHelper
    {
        private const int MaxRetryCount = 20;
        private const int DelayBetweenRetriesInSeconds = 3;

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
            DockerRunCommandProcessResult runResult = null;
            try
            {
                // Docker run the runtime container as a foreground process. This way we can catch any errors
                // that might occur when the application is being started.
                runResult = dockerCli.RunAndDoNotWaitForProcessExit(
                    runtimeImageName,
                    environmentVariables: null,
                    volumes: new List<DockerVolume> { volume },
                    portMapping,
                    runCmd,
                    runArgs);

                // An exception could have occurred when a docker process failed to start.
                Assert.Null(runResult.Exception);

                for (var i = 0; i < MaxRetryCount; i++)
                {
                    await Task.Delay(TimeSpan.FromSeconds(DelayBetweenRetriesInSeconds));

                    try
                    {
                        // Make sure the process is still alive and fail fast if not alive.
                        Assert.False(runResult.Process.HasExited);

                        await assertAction();
                        break;
                    }
                    catch (Exception ex) when (ex.InnerException is IOException || ex.InnerException is SocketException)
                    {
                        if (i == MaxRetryCount - 1)
                        {
                            output.WriteLine(runResult.GetDebugInfo());
                            throw;
                        }
                    }
                }
            }
            finally
            {
                if (runResult != null && runResult.Exception == null)
                {
                    // Stop the container so that shared resources (like ports) are disposed.
                    dockerCli.StopContainer(runResult.ContainerName);
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
                    output.WriteLine(message);
                    throw;
                }
            }
        }
    }
}
