// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    public static class OptionsServiceCollectionExtensions
    {
        public static IServiceCollection AddOptionsServices(this IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<BuildScriptGeneratorOptions>, BuildScriptGeneratorOptionsSetup>());
            return services;
        }
    }
}
