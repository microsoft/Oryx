// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal static class PreAndPostBuildCommandHelper
    {
        public static (string preBuildCommand, string postBuildCommand) GetPreAndPostBuildCommands(
            ISourceRepo sourceRepo,
            EnvironmentSettings settings)
        {
            if (settings == null)
            {
                return (null, null);
            }

            string preBuildCommand = null;
            string postBuildCommand = null;

            if (!string.IsNullOrEmpty(settings.PreBuildScriptPath))
            {
                preBuildCommand = $"\"{settings.PreBuildScriptPath}\"";
            }
            else if (!string.IsNullOrEmpty(settings.PreBuildCommand))
            {
                preBuildCommand = GetCommandOrFilePath(sourceRepo, settings.PreBuildCommand);
            }

            if (!string.IsNullOrEmpty(settings.PostBuildScriptPath))
            {
                postBuildCommand = $"\"{settings.PostBuildScriptPath}\"";
            }
            else if (!string.IsNullOrEmpty(settings.PostBuildCommand))
            {
                postBuildCommand = GetCommandOrFilePath(sourceRepo, settings.PostBuildCommand);
            }

            return (preBuildCommand: preBuildCommand, postBuildCommand: postBuildCommand);
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
                    return $"\"{fullPath}\"";
                }
            }

            return commandOrScriptPath;
        }
    }
}
