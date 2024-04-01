// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using YamlDotNet.Serialization;

namespace Microsoft.Oryx.BuildScriptGenerator.Extensibility
{
    public class ExtensibleEnvironmentVariable
    {
        /// <summary>
        /// Gets or sets the name of the environment variable.
        /// </summary>
        [YamlMember(Alias = "name", ApplyNamingConventions = false)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the environment variable.
        /// </summary>
        [YamlMember(Alias = "value", ApplyNamingConventions = false)]
        public string Value { get; set; }

        /// <summary>
        /// Gets a build script snippet for the current environment variable.
        /// </summary>
        /// <returns>A build script snippet for the current environment variable.</returns>
        public string GetBuildScriptSnippet()
        {
            if (string.IsNullOrEmpty(this.Name) || string.IsNullOrEmpty(this.Value))
            {
                return string.Empty;
            }

            // Escape double quotes in the provided value
            var escapedValue = this.Value.Replace("\"", "\\\"");
            return $"export {this.Name}=\"{escapedValue}\"";
        }
    }
}
