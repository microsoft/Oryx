// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal static class PreAndPostBuildCommandHelper
    {
        public static (string preBuildCommandOrScriptPath, string postBuildCommandOrScriptPath)
            GetPreAndPostBuildScriptsOrCommands(ISourceRepo sourceRepo, IEnvironment environment)
        {
            var preBuildScriptPath = environment.GetEnvironmentVariable(EnvironmentSettingsKeys.PreBuildScriptPath);
            var preBuildCmd = environment.GetEnvironmentVariable(EnvironmentSettingsKeys.PreBuildCommand);
            var postBuildScriptPath = environment.GetEnvironmentVariable(EnvironmentSettingsKeys.PostBuildScriptPath);
            var postBuildCmd = environment.GetEnvironmentVariable(EnvironmentSettingsKeys.PostBuildCommand);

            var preBuildCommandOrScriptPath = string.IsNullOrEmpty(preBuildScriptPath)
                ? preBuildCmd : preBuildScriptPath;
            if (!string.IsNullOrEmpty(preBuildCommandOrScriptPath))
            {
                preBuildCommandOrScriptPath = GetCommandOrFilePath(sourceRepo, preBuildCommandOrScriptPath);
            }

            var postBuildCommandOrScriptPath = string.IsNullOrEmpty(postBuildScriptPath)
                ? postBuildCmd : postBuildScriptPath;
            if (!string.IsNullOrEmpty(postBuildCommandOrScriptPath))
            {
                postBuildCommandOrScriptPath = GetCommandOrFilePath(sourceRepo, postBuildCommandOrScriptPath);
            }

            return (preBuildCommandOrScriptPath: preBuildCommandOrScriptPath,
                postBuildCommandOrScriptPath: postBuildCommandOrScriptPath);
        }

        private static string GetCommandOrFilePath(ISourceRepo sourceRepo, string commandOrScriptPath)
        {
            if (string.IsNullOrEmpty(commandOrScriptPath))
            {
                return null;
            }

            string fullyQualifiedPath = null;
            if (Path.IsPathFullyQualified(commandOrScriptPath))
            {
                fullyQualifiedPath = commandOrScriptPath;
            }
            else
            {
                fullyQualifiedPath = Path.Combine(sourceRepo.RootPath, commandOrScriptPath);
            }

            if (fullyQualifiedPath != null)
            {
                var fullPath = Path.GetFullPath(fullyQualifiedPath);
                if (File.Exists(Path.GetFullPath(fullPath)))
                {
                    ProcessHelper.TrySetExecutableMode(fullPath);

                    return $"\"{fullPath}\"";
                }
            }

            return commandOrScriptPath;
        }
    }
}
