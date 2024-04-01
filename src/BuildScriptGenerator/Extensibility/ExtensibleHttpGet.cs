// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Linq;
using YamlDotNet.Serialization;

namespace Microsoft.Oryx.BuildScriptGenerator.Extensibility
{
    /// <summary>
    /// Class used to represent HTTP GET requests against a URL.
    ///
    public class ExtensibleHttpGet
    {
        /// <summary>
        /// Gets or sets the URL to make the HTTP GET request against.
        /// </summary>
        [YamlMember(Alias = "url", ApplyNamingConventions = false)]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the file name that the HTTP GET request should be saved to.
        /// </summary>
        [YamlMember(Alias = "file-name", ApplyNamingConventions = false)]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the list of headers provided for the HTTP GET request.
        /// </summary>
        [YamlMember(Alias = "headers", ApplyNamingConventions = false)]
        public string[] Headers { get; set; }

        /// <summary>
        /// Gets a build script snippet for the current HTTP GET request.
        /// </summary>
        /// <returns>A build script snippet for the current HTTP GET request.</returns>
        public string GetBuildScriptSnippet()
        {
            if (string.IsNullOrEmpty(this.Url))
            {
                return string.Empty;
            }

            var result = "curl";
            if (!string.IsNullOrEmpty(this.FileName))
            {
                result = $"{result} -o {this.FileName}";
            }

            if (this.Headers?.Any() == true)
            {
                foreach (var header in this.Headers)
                {
                    result = $"{result} -H {header}";
                }
            }

            result = $"{result} {this.Url}";
            return result;
        }
    }
}
