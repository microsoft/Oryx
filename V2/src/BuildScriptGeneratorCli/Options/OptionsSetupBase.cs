// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    public abstract class OptionsSetupBase
    {
        private readonly IConfiguration config;

        public OptionsSetupBase(IConfiguration configuration)
        {
            this.config = configuration;
        }

        protected string GetStringValue(string key)
        {
            return this.config.GetValue<string>(key);
        }

        protected bool GetBooleanValue(string key)
        {
            var value = this.GetStringValue(key);
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
                throw new InvalidOperationException(
                    $"Invalid value '{key}' for key '{value}'. Value can either be 'true' or 'false'.");
            }
        }
    }
}
