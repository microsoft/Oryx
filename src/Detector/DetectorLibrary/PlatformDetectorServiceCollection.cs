// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.DotNetCore;
using Microsoft.Oryx.Detector.Node;
using Microsoft.Oryx.Detector.Php;
using Microsoft.Oryx.Detector.Python;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public static class PlatformDetectorServiceCollection
    {
        public static IServiceCollection AddPlatformDetectorServices(this IServiceCollection services)
        {
            services.AddSingleton<IDetector, DefaultPlatformDetector>();
            services.AddSingleton<IPlatformDetectorProvider, PlatformDetectorProvider>();

            services.AddSingleton<NodePlatformDetector>();
            services.AddSingleton<NodeVersionProvider>();
            services.AddSingleton<NodeOnDiskVersionProvider>();
            services.AddSingleton<NodeSdkStorageVersionProvider>();

            services.AddSingleton<DotNetCorePlatformDetector>();
            services.AddSingleton<DotNetCoreVersionProvider>();
            services.AddSingleton<DotNetCoreOnDiskVersionProvider>();
            services.AddSingleton<DotNetCoreSdkStorageVersionProvider>();
            services.AddSingleton<DefaultProjectFileProvider>();
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProjectFileProvider, ExplicitProjectFileProvider>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProjectFileProvider, RootDirectoryProjectFileProvider>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProjectFileProvider, ProbeAndFindProjectFileProvider>());


            services.AddSingleton<PhpPlatformDetector>();
            services.AddSingleton<PhpVersionProvider>();

            services.AddSingleton<PythonPlatformDetector>();
            services.AddSingleton<PythonVersionProvider>();
            services.AddSingleton<PythonOnDiskVersionProvider>();
            services.AddSingleton<PythonSdkStorageVersionProvider>();

            return services;
        }
    }
}