// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Oryx.BuildScriptGenerator.DeploymentProgress
{
    internal sealed class DeploymentProgressWriterProcessFactory : IDeploymentProgressWriterProcessFactory
    {
        internal const string DeploymentProgressWriteCommandName = "__deployment-progress-write";

        public IDeploymentProgressWriterProcess Start(string endpointPath)
        {
            var processPath = Environment.ProcessPath;
            if (string.IsNullOrEmpty(processPath))
            {
                throw new InvalidOperationException("The current process path is unavailable.");
            }

            var startInfo = CreateStartInfo(
                processPath,
                Environment.GetCommandLineArgs(),
                endpointPath);
            return new DeploymentProgressWriterProcess(Process.Start(startInfo));
        }

        internal static ProcessStartInfo CreateStartInfo(
            string processPath,
            string[] commandLineArguments,
            string endpointPath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = processPath,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            startInfo.Environment[DeploymentProgressConstants.EndpointEnvironmentVariable] =
                $"{DeploymentProgressConstants.FileEndpointPrefix}{endpointPath}";

            if (IsDotNetHost(processPath))
            {
                if (commandLineArguments.Length == 0 ||
                    !commandLineArguments[0].EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("The managed entry assembly path is unavailable.");
                }

                startInfo.ArgumentList.Add(commandLineArguments[0]);
            }

            startInfo.ArgumentList.Add(DeploymentProgressWriteCommandName);
            return startInfo;
        }

        private static bool IsDotNetHost(string processPath)
        {
            var fileName = Path.GetFileNameWithoutExtension(processPath);
            return string.Equals(fileName, "dotnet", StringComparison.OrdinalIgnoreCase);
        }
    }
}
