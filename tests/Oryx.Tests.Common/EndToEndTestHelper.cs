// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Oryx.Tests.Common
{
    public static class EndToEndTestHelper
    {
        private const int MaxRetryCount = 100;
        private static readonly TimeSpan DelayBetweenRetries = TimeSpan.FromSeconds(10);

        public static Task BuildRunAndAssertAppAsync(
            string appName,
            ITestOutputHelper output,
            DockerVolume volume,
            string buildCmd,
            string[] buildArgs,
            string runtimeImageName,
            int port,
            string runCmd,
            string[] runArgs,
            Func<int, Task> assertAction)
        {
            return BuildRunAndAssertAppAsync(
                appName,
                output,
                new[] { volume },
                buildCmd,
                buildArgs,
                runtimeImageName,
                port,
                runCmd,
                runArgs,
                assertAction);
        }

        public static Task BuildRunAndAssertAppAsync(
            string appName,
            ITestOutputHelper output,
            IEnumerable<DockerVolume> volumes,
            string buildCmd,
            string[] buildArgs,
            string runtimeImageName,
            int port,
            string runCmd,
            string[] runArgs,
            Func<int, Task> assertAction)
        {
            return BuildRunAndAssertAppAsync(
                appName,
                output,
                volumes,
                Settings.BuildImageName,
                buildCmd,
                buildArgs,
                runtimeImageName,
                port,
                runCmd,
                runArgs,
                assertAction);
        }

        public static Task BuildRunAndAssertAppAsync(
            string appName,
            ITestOutputHelper output,
            IEnumerable<DockerVolume> volumes,
            string buildImage,
            string buildCmd,
            string[] buildArgs,
            string runtimeImageName,
            List<EnvironmentVariable> environmentVariables,
            int port,
            string runCmd,
            string[] runArgs,
            Func<int, Task> assertAction)
        {
            var AppNameEnvVariable = new EnvironmentVariable(ExtVarNames.AppServiceAppNameEnvVarName, appName);
            environmentVariables.Add(AppNameEnvVariable);
            return BuildRunAndAssertAppAsync(
                output,
                volumes,
                buildImage,
                buildCmd,
                buildArgs,
                runtimeImageName,
                environmentVariables,
                port,
                link: null,
                runCmd,
                runArgs,
                assertAction);
        }

        public static Task BuildRunAndAssertAppAsync(
            string appName,
            ITestOutputHelper output,
            IEnumerable<DockerVolume> volumes,
            string buildImage,
            string buildCmd,
            string[] buildArgs,
            string runtimeImageName,
            int port,
            string runCmd,
            string[] runArgs,
            Func<int, Task> assertAction)
        {
            return BuildRunAndAssertAppAsync(
                output,
                volumes,
                buildImage,
                buildCmd,
                buildArgs,
                runtimeImageName,
                new List<EnvironmentVariable>()
                {
                    new EnvironmentVariable(ExtVarNames.AppServiceAppNameEnvVarName, appName)
                },
                port,
                link: null,
                runCmd,
                runArgs,
                assertAction);
        }

        //  The sequence of steps are:
        //  1.  Copies the sample app from host machine's git repo to a different folder (under 'tmp/<reserved-name>' folder
        //      in CI machine and in the 'bin\debug\' folder in non-CI machine). The copying is necessary to not muck
        //      with git tracked samples folder and also a single sample could be used for verification in multiple tests.
        //  2.  Volume mounts the directory to the build image and build it.
        //  3.  Volume mounts the same directory to runtime image and runs the application.
        //  4.  A func supplied by the user is retried to the max of 10 retries between a delay of 1 second.
        public static async Task BuildRunAndAssertAppAsync(
            ITestOutputHelper output,
            IEnumerable<DockerVolume> volumes,
            string buildImage,
            string buildCmd,
            string[] buildArgs,
            string runtimeImageName,
            List<EnvironmentVariable> environmentVariables,
            int port,
            string link,
            string runCmd,
            string[] runArgs,
            Func<int, Task> assertAction)
        {
            var dockerCli = new DockerCli();

            // Build
            var buildAppResult = dockerCli.Run(new DockerRunArguments
            {
                ImageId = buildImage,
                EnvironmentVariables = environmentVariables,
                Volumes = volumes,
                RunContainerInBackground = false,
                CommandToExecuteOnRun = buildCmd,
                CommandArguments = buildArgs,
            });

            await RunAssertsAsync(
               () =>
               {
                   Assert.True(buildAppResult.IsSuccess);
                   return Task.CompletedTask;
               },
               buildAppResult,
               output);

            // Run
            await RunAndAssertAppAsync(
                runtimeImageName,
                output,
                volumes,
                environmentVariables,
                port,
                link,
                runCmd,
                runArgs,
                assertAction,
                dockerCli);
        }

        //  The sequence of steps are:
        //  1.  Copies the sample app from host machine's git repo to a different folder (under 'tmp/<reserved-name>' folder
        //      in CI machine and in the 'bin\debug\' folder in non-CI machine). The copying is necessary to not muck
        //      with git tracked samples folder and also a single sample could be used for verification in multiple tests.
        //  2.  Volume mounts the directory to the build image and build it.
        //  3.  Volume mounts the same directory to runtime image and runs the application.
        //  4.  Waits a set period of time before forcibly exiting the container, if it has not already exited.
        //  5.  Asserts that the application fails, and returns the standard error output from the container.
        public static async Task<string> BuildRunAndAssertFailureAsync(
            ITestOutputHelper output,
            IEnumerable<DockerVolume> volumes,
            string buildImage,
            string buildCmd,
            string[] buildArgs,
            string runtimeImageName,
            List<EnvironmentVariable> environmentVariables,
            int port,
            string link,
            string runCmd,
            string[] runArgs,
            TimeSpan waitTimeForContainerExit)
        {
            var dockerCli = new DockerCli(waitTimeForContainerExit, null);

            // Build
            var buildAppResult = dockerCli.Run(new DockerRunArguments
            {
                ImageId = buildImage,
                EnvironmentVariables = environmentVariables,
                Volumes = volumes,
                RunContainerInBackground = false,
                CommandToExecuteOnRun = buildCmd,
                CommandArguments = buildArgs,
            });

            await RunAssertsAsync(
               () =>
               {
                   Assert.True(buildAppResult.IsSuccess);
                   return Task.CompletedTask;
               },
               buildAppResult,
               output);

            // Run and return debug output of failed container
            return RunAndAssertFailure(
                runtimeImageName,
                volumes,
                environmentVariables,
                port,
                link,
                runCmd,
                runArgs,
                dockerCli);
        }

        public static async Task RunAndAssertAppAsync(
            string imageName,
            ITestOutputHelper output,
            IEnumerable<DockerVolume> volumes,
            List<EnvironmentVariable> environmentVariables,
            int port,
            string link,
            string runCmd,
            string[] runArgs,
            Func<int, Task> assertAction,
            DockerCli dockerCli)
        {
            DockerRunCommandProcessResult runResult = null;
            var showDebugInfo = true;
            try
            {
                // Docker run the runtime container as a foreground process. This way we can catch any errors
                // that might occur when the application is being started.
                runResult = await dockerCli.RunAndWaitForContainerStartAsync(new DockerRunArguments
                {
                    ImageId = imageName,
                    EnvironmentVariables = environmentVariables,
                    Volumes = volumes,
                    PortInContainer = port,
                    Link = link,
                    CommandToExecuteOnRun = runCmd,
                    CommandArguments = runArgs,
                });

                // An exception could have occurred when a Docker process failed to start.
                Assert.Null(runResult.Exception);
                Assert.False(runResult.Process.HasExited);

                var hostPort = await GetHostPortAsync(dockerCli, runResult.ContainerName, portInContainer: port);

                for (var i = 0; i < MaxRetryCount; i++)
                {
                    try
                    {
                        // Make sure the process is still alive and fail fast if not.
                        Assert.False(runResult.Process.HasExited);
                        await assertAction(hostPort);

                        showDebugInfo = false;
                        break;
                    }
                    catch (Exception ex) when (ex.InnerException is IOException
                                            || ex.InnerException is SocketException)
                    {
                        if (i == MaxRetryCount - 1)
                        {
                            throw;
                        }

                        await Task.Delay(DelayBetweenRetries);
                    }
                }
            }
            finally
            {
                if (runResult != null)
                {
                    // Stop the container so that shared resources (like ports) are disposed.
                    dockerCli.StopContainer(runResult.ContainerName);

                    // Access the output and error streams after the process has exited
                    if (showDebugInfo)
                    {
                        output.WriteLine(runResult.GetDebugInfo());
                    }
                }
            }
        }

        public static string RunAndAssertFailure(
            string imageName,
            IEnumerable<DockerVolume> volumes,
            List<EnvironmentVariable> environmentVariables,
            int port,
            string link,
            string runCmd,
            string[] runArgs,
            DockerCli dockerCli)
        {
            DockerRunCommandResult runResult = null;
            try
            {
                // Docker run the runtime container as a foreground process. This way we can catch any errors
                // that might occur when the application is being started.
                runResult = dockerCli.Run(new DockerRunArguments
                {
                    ImageId = imageName,
                    EnvironmentVariables = environmentVariables,
                    Volumes = volumes,
                    PortInContainer = port,
                    Link = link,
                    CommandToExecuteOnRun = runCmd,
                    CommandArguments = runArgs,
                });

                // An exception should have occurred while the process started,
                // causing it to fail and the container to exit
                Assert.NotEqual(0, runResult.ExitCode);
                Assert.True(runResult.HasExited);
            }
            finally
            {
                if (runResult != null)
                {
                    // Stop the container so that shared resources (like ports) are disposed.
                    dockerCli.StopContainer(runResult.ContainerName);
                }
            }
            return runResult?.GetDebugInfo();
        }

        public static Task RunPackAndAssertAppAsync(
            ITestOutputHelper output,
            string appName,
            DockerVolume appVolume,
            string appImageName,
            string builderImageName,
            Func<int, Task> assertAction)
        {
            const int port = 8080;
            var imageHelper = new ImageTestHelper(output);

            return BuildRunAndAssertAppAsync(
                output,
                new[] { appVolume, DockerVolume.DockerDaemonSocket },
                imageHelper.GetPackImage(),
                buildCmd: null, // `pack` is already in the image's ENTRYPOINT
                new[]
                {
                    "build", appImageName,
                    "--no-pull", "--no-color",
                    "--path", appVolume.ContainerDir,
                    "--builder", builderImageName
                },
                appImageName,
                new List<EnvironmentVariable>()
                {
                    new EnvironmentVariable("PORT", port.ToString()) // Used by some of the apps or their run scripts
                },
                port,
                link: null,
                runCmd: null, // It should already be embedded in the image as the ENTRYPOINT
                runArgs: null,
                assertAction);
        }

        private static async Task<int> GetHostPortAsync(DockerCli dockerCli, string containerName, int portInContainer)
        {
            // We are depending on Docker to open ports in the host dynamically in order for our tests to be able to
            // run in parallel without worrying about port collision and flaky tests. However it appears that Docker
            // opens up a port in the host only after an attempt was made to open up a port in the container. Since a
            // port in the container could open up late because of the way a web application works (like it might be
            // initializing something which could take time), we are introducing delay for the application to be up
            // before trying to invoke the 'docker port' command.
            for (var i = 0; i < MaxRetryCount; i++)
            {
                await Task.Delay(DelayBetweenRetries);

                // Example output from `docker port <container-name> <container-port>`:
                // "0.0.0.0:32785:::32785"
                var getPortMappingResult = dockerCli.GetPortMapping(containerName, portInContainer);
                if (getPortMappingResult.ExitCode == 0)
                {
                    var stdOut = getPortMappingResult.StdOut?.Trim().ReplaceNewLine();
                    Console.WriteLine("stdOut: " + stdOut);
                    var portMapping = stdOut?.Split(":");
                    Console.WriteLine("portMapping: " + portMapping);
                    Assert.NotNull(portMapping);
                    Assert.True(
                        (portMapping.Length > 1),
                        "Did not get the port mapping in expected format. StdOut: " + getPortMappingResult.StdOut);
                    var hostPort = Convert.ToInt32(portMapping.Last());
                    return hostPort;
                }
                else if (getPortMappingResult.StdErr.Contains("No such container") || (getPortMappingResult.HasExited && getPortMappingResult.ExitCode != 0))
                {
                    throw new InvalidOperationException($"Could not retreive the host port of the container {containerName}:{portInContainer}. " +
                        $"{getPortMappingResult.StdErr}");
                }
            }

            throw new InvalidOperationException($"Could not retreive the host port of the container {containerName}:{portInContainer}. " +
                $"Timed out while attempting to retrieve the port.");
        }

        private static async Task RunAssertsAsync(Func<Task> action, DockerResultBase res, ITestOutputHelper output)
        {
            try
            {
                await action();
            }
            catch (EqualException exc)
            {
                output.WriteLine(res.GetDebugInfo(new Dictionary<string, string>
                {
                    { "Actual value", exc.Actual },
                    { "Expected value", exc.Expected },
                }));
                throw;
            }
            catch (Exception)
            {
                output.WriteLine(res.GetDebugInfo());
                throw;
            }
        }
    }
}
