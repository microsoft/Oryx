// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal static class PreAndPostBuildScriptHelper
    {
        public static (string preBuildScript, string postBuildScript) GetPreAndPostBuildScript(
            ISourceRepo sourceRepo,
            EnvironmentSettings settings)
        {
            if (settings == null)
            {
                return (null, null);
            }

            string preBuildScript = null;
            string postBuildScript = null;

            if (!string.IsNullOrEmpty(settings.PreBuildScriptPath))
            {
                preBuildScript = $"\"{settings.PreBuildScriptPath}\"";
            }
            else if (!string.IsNullOrEmpty(settings.PreBuildScript))
            {
                preBuildScript = GetCommandOrScriptPath(sourceRepo, settings.PreBuildScript);
            }

            if (!string.IsNullOrEmpty(settings.PostBuildScriptPath))
            {
                postBuildScript = $"\"{settings.PostBuildScriptPath}\"";
            }
            else if (!string.IsNullOrEmpty(settings.PreBuildScript))
            {
                postBuildScript = GetCommandOrScriptPath(sourceRepo, settings.PostBuildScript);
            }

            return (preBuildScript: preBuildScript, postBuildScript: postBuildScript);
        }

        private static string GetCommandOrScriptPath(ISourceRepo sourceRepo, string commandOrScriptPath)
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
