// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Text;
using JetBrains.Annotations;

namespace Microsoft.Oryx.Common
{
    /// <summary>
    /// Builds bash script commands in a single line. Note that this does not add the '#!/bin/bash'.
    /// </summary>
    public class ShellScriptBuilder
    {
        private const string DefaultCommandSeparator = " && ";
        private readonly StringBuilder _scriptBuilder;
        private string _commandSeparator = DefaultCommandSeparator;
        private bool _contentPresent = false;

        /// <summary>
        /// Builds bash script commands in a single line. Note that this does not add the '#!/bin/bash'.
        /// </summary>
        public ShellScriptBuilder(string cmdSeparator = null)
        {
            _scriptBuilder = new StringBuilder();

            if (cmdSeparator != null)
            {
                _commandSeparator = cmdSeparator;
            }
        }

        public ShellScriptBuilder AddShebang([NotNull] string interpreterPath)
        {
            if (!interpreterPath.StartsWith('/'))
            {
                throw new ArgumentException("Interpreter path must be absolute");
            }

            return Append("#!" + interpreterPath);
        }

        public ShellScriptBuilder AddCommand(string command)
        {
            command = command.Trim(' ', '&');
            return Append(command);
        }

        public ShellScriptBuilder Source(string command)
        {
            return AddCommand(". " + command); // Dot is preferable to `source` as it's supported in more shells
        }

        /// <summary>
        /// Adds the 'oryx build' command with the supplied <paramref name="argumentsString"/>.
        /// </summary>
        /// <param name="argumentsString"></param>
        /// <returns></returns>
        public ShellScriptBuilder AddBuildCommand(string argumentsString)
        {
            return Append($"oryx build --debug {argumentsString}");
        }

        /// <summary>
        /// Adds the 'oryx build-script' command with the supplied <paramref name="argumentsString"/>.
        /// </summary>
        /// <param name="argumentsString"></param>
        /// <returns></returns>
        public ShellScriptBuilder AddScriptCommand(string argumentsString)
        {
            return Append($"oryx build-script {argumentsString}");
        }

        public ShellScriptBuilder SetEnvironmentVariable(string name, string value)
        {
            return Append($"export {name}={value}");
        }

        public ShellScriptBuilder CreateDirectory(string directory)
        {
            return Append($"mkdir -p \"{directory}\"");
        }

        public ShellScriptBuilder CreateFile(string file, string content)
        {
            return Append($"echo {content} > \"{file}\"");
        }

        public ShellScriptBuilder SetExecutePermissionOnFile(string file)
        {
            return Append($"chmod +x \"{file}\"");
        }

        public ShellScriptBuilder AddDirectoryDoesNotExistCheck(string directory)
        {
            return Append(
                $"if [ -d \"{directory}\" ]; then " +
                $"echo Directory '{directory}' is still present 1>&2 && " +
                "exit 1; fi");
        }

        public ShellScriptBuilder AddDirectoryExistsCheck(string directory)
        {
            return Append(
                $"if [ ! -d \"{directory}\" ]; then " +
                $"echo Directory '{directory}' not found 1>&2 && " +
                "exit 1; fi");
        }

        public ShellScriptBuilder AddFileDoesNotExistCheck(string file)
        {
            return Append(
                $"if [ -f \"{file}\" ]; then " +
                $"echo File '{file}' is still present 1>&2 && " +
                "exit 1; fi");
        }

        public ShellScriptBuilder AddFileExistsCheck(string file)
        {
            return Append(
                $"if [ ! -f \"{file}\" ]; then " +
                $"echo File '{file}' not found 1>&2 && " +
                "exit 1; fi");
        }

        public ShellScriptBuilder AddStringExistsInFileCheck(string searchString, string file)
        {
            return Append(
                $"if ! grep -q '{searchString}' '{file}'; then " +
                $"echo '{searchString}' not found 1>&2; " +
                "exit 1; fi");
        }

        public ShellScriptBuilder AddStringDoesNotExistInFileCheck(string searchString, string file)
        {
            return Append(
                $"if grep -q '{searchString}' '{file}'; then " +
                $"echo '{searchString}' still found 1>&2; " +
                "exit 1; fi");
        }

        public override string ToString()
        {
            return _scriptBuilder.ToString();
        }

        private ShellScriptBuilder Append(string content)
        {
            // NOTE: do not use AppendLine as this script must be in one line
            if (_contentPresent)
            {
                _scriptBuilder.Append(_commandSeparator);
            }

            _scriptBuilder.Append(content);
            _contentPresent = true;
            return this;
        }
    }
}