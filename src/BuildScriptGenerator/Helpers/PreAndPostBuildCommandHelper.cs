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
            BuildScriptGeneratorOptions options)
        {
            if (options == null)
            {
                return (null, null);
            }

            string preBuildCommand = null;
            string postBuildCommand = null;

            if (!string.IsNullOrEmpty(options.PreBuildScriptPath))
            {
                preBuildCommand = $"\"{options.PreBuildScriptPath}\"";
            }
            else if (!string.IsNullOrEmpty(options.PreBuildCommand))
            {
                preBuildCommand = GetCommandOrFilePath(sourceRepo, options.PreBuildCommand);
            }

            if (!string.IsNullOrEmpty(options.PostBuildScriptPath))
            {
                postBuildCommand = $"\"{options.PostBuildScriptPath}\"";
            }
            else if (!string.IsNullOrEmpty(options.PostBuildCommand))
            {
                postBuildCommand = GetCommandOrFilePath(sourceRepo, options.PostBuildCommand);
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
