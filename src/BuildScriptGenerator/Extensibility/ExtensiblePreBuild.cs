// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace Microsoft.Oryx.BuildScriptGenerator.Extensibility
{
    /// <summary>
    /// Class used to represent the various ways to run pre-build commands using the extensibility model.
    /// </summary>
    public class ExtensiblePreBuild
    {
        /// <summary>
        /// Gets or sets the description of the pre-build step.
        /// </summary>
        [YamlMember(Alias = "description", ApplyNamingConventions = false)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the list of scripts to run during the pre-build step.
        /// </summary>
        [YamlMember(Alias = "scripts", ApplyNamingConventions = false)]
        public string[] Scripts { get; set; }

        /// <summary>
        /// Gets or sets the HTTP GET request to make during the pre-build step.
        /// </summary>
        [YamlMember(Alias = "http-get", ApplyNamingConventions = false)]
        public ExtensibleHttpGet HttpGet { get; set; }

        /// <summary>
        /// Generates a build script snippet for the current pre-build step.
        /// </summary>
        /// <returns>A build script snippet for the current pre-build step.</returns>
        public string GetBuildScriptSnippet()
        {
            var result = new StringBuilder();

            // Display the description of the step in the script
            if (!string.IsNullOrEmpty(this.Description))
            {
                result.AppendLine($"echo \"Running installing step: '{this.Description}'\"");
                result.AppendLine();
            }

            // Process scripts field
            if (this.Scripts?.Any() == true)
            {
                foreach (var script in this.Scripts)
                {
                    result.AppendLine(script);
                }

                result.AppendLine();
            }

            // Process http-get field
            if (this.HttpGet != null)
            {
                var httpGetScript = this.HttpGet.GetBuildScriptSnippet();
                result.AppendLine(httpGetScript);
                result.AppendLine();
            }

            return result.ToString();
        }
    }
}
