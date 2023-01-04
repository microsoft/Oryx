// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.Detector;
using Polly;
using Polly.Extensions.Http;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public static class BuildScriptGeneratorServiceCollectionExtensions
    {
        public static IServiceCollection AddBuildScriptGeneratorServices(this IServiceCollection services)
        {
            _ = services
                .AddPlatformDetectorServices()
                .AddNodeScriptGeneratorServices()
                .AddHugoScriptGeneratorServices()
                .AddPythonScriptGeneratorServices()
                .AddDotNetCoreScriptGeneratorServices()
                .AddPhpScriptGeneratorServices()
                .AddScriptGeneratorServicesRuby()
                .AddScriptGeneratorServicesGolang()
                .AddScriptGeneratorServicesForJava();

            _ = services.AddSingleton<IBuildScriptGenerator, DefaultBuildScriptGenerator>();
            _ = services.AddSingleton<ICompatiblePlatformDetector, DefaultCompatiblePlatformDetector>();
            _ = services.AddSingleton<IDockerfileGenerator, DefaultDockerfileGenerator>();
            _ = services.AddSingleton<IEnvironment, DefaultEnvironment>();
            _ = services.AddSingleton<ISourceRepoProvider, DefaultSourceRepoProvider>();
            _ = services.AddSingleton<ITempDirectoryProvider, DefaulTempDirectoryProvider>();
            _ = services.AddSingleton<IScriptExecutor, DefaultScriptExecutor>();
            _ = services.AddSingleton<IRunScriptGenerator, DefaultRunScriptGenerator>();
            _ = services.AddSingleton<DefaultPlatformsInformationProvider>();
            _ = services.AddSingleton<PlatformsInstallationScriptProvider>();
            _ = services.AddHttpClient("general", httpClient =>
            {
                // NOTE: Setting user agent is required to avoid receiving 403 Forbidden response.
                httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("oryx", "1.0"));
            }).AddPolicyHandler(GetRetryPolicy());

            // Add all checkers (platform-dependent + platform-independent)
            foreach (Type type in typeof(BuildScriptGeneratorServiceCollectionExtensions).Assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(CheckerAttribute), false).Length > 0)
                {
                    services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IChecker), type));
                }
            }

            return services;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryAsync(
                    retryCount: 6,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}
