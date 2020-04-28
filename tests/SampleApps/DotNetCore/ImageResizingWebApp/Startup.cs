using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace dotnetapp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Run(httpContext =>
            {
                var resizeStatus = ResizeImage();

                var statusMessage = "Resizing image failed";
                if (resizeStatus)
                {
                    statusMessage = "Resizing image succeeded";
                }

                return httpContext.Response.WriteAsync(statusMessage);
            });
        }

        private bool ResizeImage()
        {
            var width = 128;
            var height = 128;
            var originalImageFileName = "msft_logo.png";
            var resizedImageFileName = $"{Guid.NewGuid()}.png";
            using (var pngStream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), originalImageFileName)))
            using (var image = new Bitmap(pngStream))
            {
                var resized = new Bitmap(width, height);
                using (var graphics = Graphics.FromImage(resized))
                {
                    graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.DrawImage(image, x: 0, y: 0, width, height);
                    resized.Save(resizedImageFileName, ImageFormat.Png);
                }
            }

            var resizedImageFileExists = File.Exists(
                Path.Combine(Directory.GetCurrentDirectory(), resizedImageFileName));

            return resizedImageFileExists;
        }
    }
}
