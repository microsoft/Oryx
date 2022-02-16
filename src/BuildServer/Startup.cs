// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using JsonFlatFileDataStore;
using Microsoft.Oryx.BuildServer.Repositories;
using Microsoft.Oryx.BuildServer.Services;
using Microsoft.Oryx.BuildServer.Services.ArtifactBuilders;
using System.IO;

namespace Microsoft.Oryx.BuildServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string folderName = "/store";
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            var store = new DataStore("/store/builds.json", keyProperty: "id");
            services.AddHttpContextAccessor();
            services.AddMvc();
            services.AddSingleton<IRepository>(x => new BuildRepository(store));
            services.AddScoped<IArtifactBuilder, Builder>();
            services.AddScoped<IArtifactBuilderFactory, ArtifactBuilderFactory>();
            services.AddScoped<IBuildRunner, BuildRunner>();
            services.AddScoped<IBuildService, BuildService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddControllers();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            //    endpoints.MapControllerRoute(
            //        "default",
            //        pattern: "{controller=Build}/{action=Index}/{id?}");
            });
        }
    }
}
