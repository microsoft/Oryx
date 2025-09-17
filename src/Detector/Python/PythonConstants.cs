// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.Detector.Python
{
    internal static class PythonConstants
    {
        public const string PlatformName = "python";
        public const string RequirementsFileName = "requirements.txt";
        public const string PyprojectTomlFileName = "uv.lock";
        public const string UvLockFileName = "uv.lock";
        public const string RuntimeFileName = "runtime.txt";
        public const string PythonFileNamePattern = "*.py";
        public const string ZipFileExtension = "tar.gz";
        public const string SetupDotPyFileName = "setup.py";
        public const string CondaEnvironmentYamlFileName = "environment.yml";
        public const string CondaEnvironmentYmlFileName = "environment.yaml";
        public const string JupyterNotebookFileExtensionName = "ipynb";
        public static readonly HashSet<string> DjangoFileNames = new HashSet<string> { "manage.py", "wsgi.py", "app.py" };
        public static readonly string[] CondaEnvironmentFileKeys = new[] { "channels", "dependencies" };
    }
}
