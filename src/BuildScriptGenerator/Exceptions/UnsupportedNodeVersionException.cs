// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator.Exceptions
{
    /// <summary>
    /// Target version of Node.JS is not supported.
    /// </summary>
    public class UnsupportedNodeVersionException : InvalidUsageException
    {
        public string TargetNodeJsVersionRange { get; private set; }

        public UnsupportedNodeVersionException(string targetNodeJsVersionRange)
            : base($"The target Node.JS version specification '{targetNodeJsVersionRange}' is not supported.")
        {
            TargetNodeJsVersionRange = targetNodeJsVersionRange;
        }
    }
}