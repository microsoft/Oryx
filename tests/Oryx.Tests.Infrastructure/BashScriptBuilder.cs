// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.Text;

namespace Oryx.Tests.Infrastructure
{
    /// <summary>
    /// Builds bash script commands in a single line. Note that this does not add the '#!/bin/bash'.
    /// </summary>
    public class BashScriptBuilder
    {
        private bool _contentPresent = false;
        private readonly StringBuilder _scriptBuilder;

        /// <summary>
        /// Builds bash script commands in a single line. Note that this does not add the '#!/bin/bash'.
        /// </summary>
        public BashScriptBuilder()
        {
            _scriptBuilder = new StringBuilder();
        }

        public BashScriptBuilder AddCommand(string command)
        {
            command = command.Trim(' ', '&');
            return Append(command);
        }

        /// <summary>
        /// Adds the 'oryx build' command with the supplied <paramref name="argumentsString"/>.
        /// </summary>
        /// <param name="argumentsString"></param>
        /// <returns></returns>
        public BashScriptBuilder AddBuildCommand(string argumentsString)
        {
            return Append($"oryx build {argumentsString}");
        }

        public BashScriptBuilder CreateDirectory(string directory)
        {
            return Append($"mkdir -p \"{directory}\"");
        }

        public BashScriptBuilder CreateFile(string file, string content)
        {
            return Append($"echo \"{content}\" > \"{file}\"");
        }

        public BashScriptBuilder SetExecutePermissionOnFile(string file)
        {
            return Append($"chmod +x \"{file}\"");
        }

        public BashScriptBuilder AddDirectoryDoesNotExistCheck(string directory)
        {
            return Append(
                $"if [ -d \"{directory}\" ]; then " +
                $"echo Directory '{directory}' is still prsent 1>&2 && " +
                "exit 1; fi");
        }

        public BashScriptBuilder AddDirectoryExistsCheck(string directory)
        {
            return Append(
                $"if [ ! -d \"{directory}\" ]; then " +
                $"echo Directory '{directory}' not found 1>&2 && " +
                "exit 1; fi");
        }

        public BashScriptBuilder AddFileDoesNotExistCheck(string file)
        {
            return Append(
                $"if [ -f \"{file}\" ]; then " +
                $"echo File '{file}' is still prsent 1>&2 && " +
                "exit 1; fi");
        }

        public BashScriptBuilder AddFileExistsCheck(string file)
        {
            return Append(
                $"if [ ! -f \"{file}\" ]; then " +
                $"echo File '{file}' not found 1>&2 && " +
                "exit 1; fi");
        }

        private BashScriptBuilder Append(string content)
        {
            // NOTE: do not use AppendLine as in the script must be in one line
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
