// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator.Exceptions
{
    /// <summary>
    /// Target version of Python is not supported.
    /// </summary>
    public class UnsupportedPythonVersionException : InvalidUsageException
    {
        public string TargetPythonVersionRange { get; private set; }

        public UnsupportedPythonVersionException(string targetPythonVersionRange)
            : base($"The target Python version specification '{targetPythonVersionRange}' is not supported.")
        {
            TargetPythonVersionRange = targetPythonVersionRange;
        }
    }
}