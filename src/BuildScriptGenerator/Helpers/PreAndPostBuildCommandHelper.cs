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
        public static (string preBuildCommand, string postBuildCommand) GetPreAndPostBuildCommands(
            ISourceRepo sourceRepo,
            BuildScriptGeneratorOptions options)
        {
            if (options == null)
            {
                return (null, null);
            }

            var preBuildCommandOrScriptPath = string.IsNullOrEmpty(options.PreBuildScriptPath)
                ? options.PreBuildCommand : options.PreBuildScriptPath;
            if (!string.IsNullOrEmpty(preBuildCommandOrScriptPath))
            {
                preBuildCommandOrScriptPath = GetCommandOrFilePath(sourceRepo, preBuildCommandOrScriptPath);
            }

            var postBuildCommandOrScriptPath = string.IsNullOrEmpty(options.PostBuildScriptPath)
                ? options.PostBuildCommand : options.PostBuildScriptPath;
            if (!string.IsNullOrEmpty(postBuildCommandOrScriptPath))
            {
                postBuildCommandOrScriptPath = GetCommandOrFilePath(sourceRepo, postBuildCommandOrScriptPath);
            }

            return (preBuildCommand: preBuildCommandOrScriptPath, postBuildCommand: postBuildCommandOrScriptPath);
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
