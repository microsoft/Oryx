// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    /// <summary>
    /// Settings for NodeJS build script generation.
    /// </summary>
    public interface INodeSettings
    {
        /// <summary>
        /// Default version of node to be used if none is specified in the project file.
        /// </summary>
        string NodeJsDefaultVersion { get; }

        /// <summary>
        /// Default version of npm to be used if none is specified in the project file.
        /// </summary>
        string NpmDefaultVersion { get; }
    }
}