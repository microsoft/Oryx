// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator.Exceptions
{
    /// <summary>
    /// Target version of npm is not supported.
    /// </summary>
    public class UnsupportedNpmVersionException : InvalidUsageException
    {
        public string TargetNpmVersionRange { get; private set; }

        public UnsupportedNpmVersionException(string targetNpmVersionRange)
            : base($"The target npm version specification '{targetNpmVersionRange}' is not supported.")
        {
            TargetNpmVersionRange = targetNpmVersionRange;
        }
    }
}