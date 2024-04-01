// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Text;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Microsoft.Oryx.BuildScriptGenerator.Extensibility
{
    /// <summary>
    /// Class used to represent the properties exposed by the Oryx extensibility model.
    /// </summary>
    public class ExtensibleConfigurationFile
    {
        /// <summary>
        /// Gets or sets the base OS used for the runnable application image produced by Oryx++.
        /// </summary>
        [YamlMember(Alias = "base-os", ApplyNamingConventions = false)]
        public string BaseOs { get; set; }

        /// <summary>
        /// Gets or sets the runtime platform targeted for the application.
        /// </summary>
        [YamlMember(Alias = "platform", ApplyNamingConventions = false)]
        public string Platform { get; set; }

        /// <summary>
        /// Gets or sets the runtime platform version targeted for the application.
        /// </summary>
        [YamlMember(Alias = "platform-version", ApplyNamingConventions = false)]
        public string PlatformVersion { get; set; }

        /// <summary>
        /// Gets or sets the environment variables to be set during the build process.
        /// </summary>
        [YamlMember(Alias = "env", ApplyNamingConventions = false)]
        public ExtensibleEnvironmentVariable[] EnvironmentVariables { get; set; }

        /// <summary>
        /// Gets or sets the list of pre-build steps to execute.
        /// </summary>
        [YamlMember(Alias = "pre-build", ApplyNamingConventions = false)]
        public ExtensiblePreBuild[] PreBuild { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensibleConfigurationFile"/> class from a file.
        /// </summary>
        /// <param name="text">The contents of an extensible configuration file.</param>
        /// <returns>A new <see cref="ExtensibleConfigurationFile"/> object.</returns>
        public static ExtensibleConfigurationFile Create(string text)
        {
            var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();
            var configFile = deserializer.Deserialize<ExtensibleConfigurationFile>(text);
            return configFile;
        }

        /// <summary>
        /// Generates the build script snippet for the current extensible configuration file.
        /// </summary>
        /// <returns>A build script snippet for the current extensible configuration file.</returns>
        public string GetBuildScriptSnippet()
        {
            var result = new StringBuilder();
            if (this.EnvironmentVariables?.Any() == true)
            {
                foreach (var envVar in this.EnvironmentVariables)
                {
                    var envVarSnippet = envVar.GetBuildScriptSnippet();
                    result.AppendLine(envVarSnippet);
                }

                result.AppendLine();
            }

            if (this.PreBuild?.Any() == true)
            {
                foreach (var prebuildStep in this.PreBuild)
                {
                    var prebuildStepSnippet = prebuildStep.GetBuildScriptSnippet();
                    result.AppendLine(prebuildStepSnippet);
                    result.AppendLine();
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Generates a warning message to be printed whenever there's an issue generating the build script snippet.
        /// </summary>
        /// <returns>A warning message that notifies the caller how to use the extensible configuration file.</returns>
        public string GetWarningMessage()
        {
            var result = new StringBuilder();
            result.AppendLine();
            result.AppendLine($"\"{DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss")}\" | WARNING | Invalid ExtensibleConfigurationFile | Exit Code: 1 | Please review your {FilePaths.ExtensibleConfigurationFileName} | {Constants.BuildConfigurationFileHelp}");
            result.AppendLine($"The following is the structure of a valid {FilePaths.ExtensibleConfigurationFileName}:");
            result.AppendLine("-------------------------------------------");
            result.AppendLine("base-os: '<base OS for the image>'");
            result.AppendLine("platform: '<build and runtime platform for the application>'");
            result.AppendLine("platform-version: '<version of the runtime platform to use>'");
            result.AppendLine("env:");
            result.AppendLine("  - name: '<name of the environment variable to export>'");
            result.AppendLine("    value: '<value of the environment variable to export>'");
            result.AppendLine("pre-build:");
            result.AppendLine("  - description: '<description of first pre-build step>'");
            result.AppendLine("    scripts:");
            result.AppendLine("      - '<list of custom commands to run>'");
            result.AppendLine("    http-get:");
            result.AppendLine("      url: '<URL to make HTTP GET request against>'");
            result.AppendLine("      file-name: '<name of the file that the request should be saved to>'");
            result.AppendLine("      headers:");
            result.AppendLine("        - '<list of headers to send with the request>'");
            result.AppendLine("-------------------------------------------");
            return result.ToString();
        }
    }
}
