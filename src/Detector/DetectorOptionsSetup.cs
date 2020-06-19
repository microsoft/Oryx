// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;

namespace Microsoft.Oryx.Detector
{
    public class DetectorOptionsSetup : IConfigureOptions<DetectorOptions>
    {
        private readonly IConfiguration _config;

        public DetectorOptionsSetup(IConfiguration configuration)
        {
            _config = configuration;
        }

        public void Configure(DetectorOptions options)
        {
            options.Project = GetStringValue("PROJECT");
        }

        protected string GetStringValue(string key)
        {
            return _config.GetValue<string>(key);
        }

        protected bool GetBooleanValue(string key)
        {
            var value = GetStringValue(key);
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            if (bool.TryParse(value, out var result))
            {
                return result;
            }
            else
            {
                throw new Exception(
                    $"Invalid value '{key}' for key '{value}'. Value can either be 'true' or 'false'.");
            }
        }
    }
}
