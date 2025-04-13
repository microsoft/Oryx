// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Text;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using JetBrains.Annotations;

namespace Microsoft.Oryx.BuildScriptGenerator.Common
{
    /// <summary>
    /// Builds bash script commands in a single line. Note that this does not add the '#!/bin/bash'.
    /// </summary>
    public class ShellScriptBuilder
    {
        private const string DefaultCommandSeparator = " && ";
        private readonly StringBuilder scriptBuilder;
        private string commandSeparator = DefaultCommandSeparator;
        private bool contentPresent;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellScriptBuilder"/> class.
        /// Builds bash script commands in a single line. Note that this does not add the '#!/bin/bash'.
        /// </summary>
        public ShellScriptBuilder(string cmdSeparator = null, bool addDefaultTestEnvironmentVariables = true)
        {
            this.scriptBuilder = new StringBuilder();

            if (cmdSeparator != null)
            {
                this.commandSeparator = cmdSeparator;
            }

            if (addDefaultTestEnvironmentVariables)
            {
                this.AddDefaultTestEnvironmentVariables();
            }
        }

        public ShellScriptBuilder AddShebang([NotNull] string interpreterPath)
        {
            if (!interpreterPath.StartsWith('/'))
            {
                throw new ArgumentException("Interpreter path must be absolute");
            }

            return this.Append("#!" + interpreterPath);
        }

        public ShellScriptBuilder AddCommand(string command)
        {
            command = command.Trim(' ', '&');
            return this.Append(command);
        }

        public ShellScriptBuilder Source(string command)
        {
            return this.AddCommand(". " + command); // Dot is preferable to `source` as it's supported in more shells
        }

        /// <summary>
        /// Adds the 'oryx build' command with the supplied <paramref name="argumentsString"/>.
        /// </summary>
        /// <param name="argumentsString">Arguments to used during the 'oryx build' command.</param>
        /// <returns>A <see cref="ShellScriptBuilder"/> created for the 'oryx build' command with the provided arguments.</returns>
        public ShellScriptBuilder AddBuildCommand(string argumentsString)
        {
            return this.Append($"oryx build --debug {argumentsString}");
        }

        /// <summary>
        /// Adds the 'oryx build-script' command with the supplied <paramref name="argumentsString"/>.
        /// </summary>
        /// <param name="argumentsString">Arguments to used during the 'oryx build-script' command.</param>
        /// <returns>A <see cref="ShellScriptBuilder"/> created for the 'oryx build-script' command with the provided arguments.</returns>
        public ShellScriptBuilder AddScriptCommand(string argumentsString)
        {
            return this.Append($"oryx build-script {argumentsString}");
        }

        public ShellScriptBuilder SetEnvironmentVariable(string name, string value, bool isVariableSubstitutionNeeded = false)
        {
            if (isVariableSubstitutionNeeded)
            {
                // linux does not allow variable substitution in a single quoted string.
                // So, if we need to do variable substitution it has to be inside a double quoted string.
                // e.g. to add a new path in an existing env variable, variable substitution is required.
                return this.Append($"export {name}={value}");
            }
            else
            {
                return this.Append($"export {name}='{value}'");
            }
        }

        public ShellScriptBuilder CreateDirectory(string directory)
        {
            return this.Append($"mkdir -p \"{directory}\"");
        }

        public ShellScriptBuilder CreateFile(string file, string content)
        {
            return this.Append($"echo {content} > \"{file}\"");
        }

        public ShellScriptBuilder SetExecutePermissionOnFile(string file)
        {
            return this.Append($"chmod +x \"{file}\"");
        }

        public ShellScriptBuilder AddDirectoryDoesNotExistCheck(string directory)
        {
            return this.Append(
                $"if [ -d \"{directory}\" ]; then " +
                $"echo Directory '{directory}' is still present 1>&2 && " +
                "exit 1; fi");
        }

        public ShellScriptBuilder AddDirectoryExistsCheck(string directory)
        {
            return this.Append(
                $"if [ ! -d \"{directory}\" ]; then " +
                $"echo Directory '{directory}' not found 1>&2 && " +
                "exit 1; fi");
        }

        public ShellScriptBuilder AddFileDoesNotExistCheck(string file)
        {
            return this.Append(
                $"if [ -f \"{file}\" ]; then " +
                $"echo File '{file}' is still present 1>&2 && " +
                "exit 1; fi");
        }

        public ShellScriptBuilder AddFileExistsCheck(string file)
        {
            return this.Append(
                $"if [ ! -f \"{file}\" ]; then " +
                $"echo File '{file}' not found 1>&2 && " +
                "exit 1; fi");
        }

        public ShellScriptBuilder AddLinkExistsCheck(string file)
        {
            return this.Append(
                $"if [ ! -L \"{file}\" ]; then " +
                $"echo Link '{file}' is not present 1>&2 && " +
                "exit 1; fi");
        }

        public ShellScriptBuilder AddLinkDoesNotExistCheck(string file)
        {
            return this.Append(
                $"if [ -L \"{file}\" ]; then " +
                $"echo Link '{file}' is still present 1>&2 && " +
                "exit 1; fi");
        }

        public ShellScriptBuilder AddStringExistsInFileCheck(string searchString, string file)
        {
            return this.Append(
                $"if ! grep -q '{searchString}' '{file}'; then " +
                $"echo '{searchString}' not found 1>&2; " +
                "exit 1; fi");
        }

        public ShellScriptBuilder AddStringDoesNotExistInFileCheck(string searchString, string file)
        {
            return this.Append(
                $"if grep -q '{searchString}' '{file}'; then " +
                $"echo '{searchString}' still found 1>&2; " +
                "exit 1; fi");
        }

        /// <summary>
        /// Append a command to the shell script that sets the ORYX_SDK_STORAGE_BASE_URL to the value
        /// of ORYX_TEST_SDK_STORAGE_URL if ORYX_TEST_SDK_STORAGE_URL exists in the environment that is executing this code.
        /// Otherwise, use the Oryx staging sdk storage account for testing.
        /// This allows us to change the storage account that tests use without regenerating any images.
        /// </summary>
        public ShellScriptBuilder AddDefaultTestEnvironmentVariables()
        {
            return this;
        }

        public override string ToString()
        {
            return this.scriptBuilder.ToString();
        }

        private ShellScriptBuilder Append(string content)
        {
            // NOTE: do not use AppendLine as this script must be in one line
            if (this.contentPresent)
            {
                this.scriptBuilder.Append(this.commandSeparator);
            }

            this.scriptBuilder.Append(content);
            this.contentPresent = true;
            return this;
        }
    }
}
