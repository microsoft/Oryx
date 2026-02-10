// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Oryx.Detector
{
    /// <summary>
    /// Contains extensions to add services to <see cref="IServiceCollection"/>.
    /// </summary>
    public static class PlatformDetectorServiceCollection
    {
        /// <summary>
        /// Adds services related to detection of platforms.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>An instance of <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddPlatformDetectorServices(this IServiceCollection services)
        {
            services.AddSingleton<IDetector, DefaultPlatformDetector>();

            services
                .AddLogging()
                .AddOptions()
                .AddDotNetCoreServices()
                .AddGolangServices()
                .AddNodeServices()
                .AddPythonServices()
                .AddPhpServices()
                .AddHugoServices()
                .AddRubyServices();

            return services;
        }
    }
}
