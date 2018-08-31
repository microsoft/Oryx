// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodeEnvironmentVariableSettings : INodeSettings
    {
        private const string NodeJsDefaultVersionName = "NODE_DEFAULT_VERSION";
        private const string NpmDefaultVersionName = "NPM_DEFAULT_VERSION";

        /// <summary>
        /// Default version of node to be used if none is specified in the project file.
        /// </summary>
        public string NodeJsDefaultVersion => Environment.GetEnvironmentVariable(NodeJsDefaultVersionName);

        /// <summary>
        /// Default version of npm to be used if none is specified in the project file.
        /// </summary>
        public string NpmDefaultVersion => Environment.GetEnvironmentVariable(NpmDefaultVersionName);
    }
}