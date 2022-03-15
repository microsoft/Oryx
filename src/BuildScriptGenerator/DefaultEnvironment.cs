// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal class DefaultEnvironment : IEnvironment
    {
        private EnvironmentType? type; // Cache for the Type property

        public EnvironmentType Type
        {
            get
            {
                // Cache needs to be initialized
                if (!this.type.HasValue)
                {
                    foreach (var entry in LoggingConstants.OperationNameSourceEnvVars)
                    {
                        if (!string.IsNullOrEmpty(this.GetEnvironmentVariable(entry.Value)))
                        {
                            this.type = entry.Key;
                            return this.type.Value;
                        }
                    }

                    this.type = EnvironmentType.Unknown;
                }

                return this.type.Value;
            }
        }

        public bool? GetBoolEnvironmentVariable(string name)
        {
            var variable = this.GetEnvironmentVariable(name);
            if (variable.EqualsIgnoreCase(Constants.True))
            {
                return true;
            }
            else if (variable.EqualsIgnoreCase(Constants.False))
            {
                return false;
            }

            return null;
        }

        public string GetEnvironmentVariable(string name, string defaultValue = null)
        {
            return Environment.GetEnvironmentVariable(name) ?? defaultValue;
        }

        public IList<string> GetEnvironmentVariableAsList(string name)
        {
            IList<string> ret = null;
            var values = this.GetEnvironmentVariable(name);
            if (!string.IsNullOrWhiteSpace(values))
            {
                ret = values.Split(",");
                ret = ret.Select(s => s.Trim()).ToList();
            }

            return ret;
        }

        public IDictionary GetEnvironmentVariables()
        {
            return Environment.GetEnvironmentVariables();
        }

        public void SetEnvironmentVariable(string name, string value)
        {
            Environment.SetEnvironmentVariable(name, value);
        }

        public string[] GetCommandLineArgs()
        {
            return Environment.GetCommandLineArgs();
        }
    }
}
