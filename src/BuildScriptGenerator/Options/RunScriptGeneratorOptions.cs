// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Types of debugging.
    /// </summary>
    public enum DebuggingMode
    {
        /// <summary>
        /// Debugging is disabled.
        /// </summary>
        None,

        /// <summary>
        /// Debugging is enabled, stopping at any breakpoints set by the developer.
        /// </summary>
        Standard,

        /// <summary>
        /// Debugging is enabled, stopping before user code starts and at any
        /// breakpoints set by the developer.
        /// </summary>
        Break
    }

    /// <summary>
    /// Options to create the entrypoint scipt.
    /// </summary>
    public class RunScriptGeneratorOptions
    {
        /// <summary>
        /// Gets or sets the source repo where the application is stored.
        /// </summary>
        public ISourceRepo SourceRepo { get; set; }

        /// <summary>
        /// Gets or sets the platform version the application expects.
        /// </summary>
        public string PlatformVersion { get; set; }

        /// <summary>
        /// Gets to sets the port where the application will bind to.
        /// </summary>
        public int BindPort { get; set; }

        /// <summary>
        /// Gets or sets the user's custom startup command.
        /// </summary>
        public string UserStartupCommand { get; set; }

        /// <summary>
        /// Gets or sets the path to the default application, in case we can't find the user's.
        /// </summary>
        public string DefaultAppPath { get; set; }

        /// <summary>
        /// Gets or sets a custom server, e.g. pm2, in case the user wants to override the default.
        /// </summary>
        public string CustomServerCommand { get; set; }

        /// <summary>
        /// Gets or sets the debugging mode.
        /// </summary>
        public DebuggingMode DebuggingMode { get; set; }

        /// <summary>
        /// Gets or sets the debugging port. If no value is provided, the default port is used.
        /// </summary>
        public int? DebugPort { get; set; }
    }
}