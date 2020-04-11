// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    /// <summary>
    /// A custom <see cref="IConfigurationSource"/> for reading configuration from command line.
    /// </summary>
    public class CustomConfigurationSource : ConfigurationProvider, IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return this;
        }
    }
}
