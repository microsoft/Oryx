// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Text;

namespace Microsoft.Oryx.Tests.Common
{
    /// <summary>
    /// Builds bash script commands in a single line. Note that this does not add the '#!/bin/bash'.
    /// </summary>
    public class ShellScriptBuilder
    {
        private bool _contentPresent = false;
        private readonly StringBuilder _scriptBuilder;

        /// <summary>
        /// Builds bash script commands in a single line. Note that this does not add the '#!/bin/bash'.
        /// </summary>
        public ShellScriptBuilder()
        {
            _scriptBuilder = new StringBuilder();
        }

        public ShellScriptBuilder AddCommand(string command)
        {
            command = command.Trim(' ', '&');
            return Append(command);
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
            return Append($"echo \"{content}\" > \"{file}\"");
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
                $"grep '{searchString}' '{file}' && if [ $? -eq 1 ]; then " +
                $"echo '{searchString}' not found 1>&2 && " +
                "exit 1; fi");
        }

        public ShellScriptBuilder AddStringDoesNotExistInFileCheck(string searchString, string file)
        {
            return Append(
                $"grep '{searchString}' '{file}' && if [ $? -eq 0 ]; then " +
                $"echo '{searchString}' found 1>&2 && " +
                "exit 1; fi");
        }

        private ShellScriptBuilder Append(string content)
        {
            // NOTE: do not use AppendLine as this script must be in one line
            if (_contentPresent)
            {
                _scriptBuilder.Append(" && ");
            }

            _scriptBuilder.Append(content);
            _contentPresent = true;
            return this;
        }

        public override string ToString()
        {
            return _scriptBuilder.ToString();
        }
    }
}