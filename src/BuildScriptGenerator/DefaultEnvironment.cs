// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal class DefaultEnvironment : IEnvironment
    {
        private EnvironmentType? _type; // Cache for the Type property

        public EnvironmentType Type
        {
            get
            {
                if (!_type.HasValue) // Cache needs to be initialized
                {
                    foreach (var entry in LoggingConstants.OperationNameSourceEnvVars)
                    {
                        if (!string.IsNullOrEmpty(GetEnvironmentVariable(entry.Value)))
                        {
                            _type = entry.Key;
                            return _type.Value;
                        }
                    }

                    _type = EnvironmentType.Unknown;
                }

                return _type.Value;
            }
        }

        public bool? GetBoolEnvironmentVariable(string name)
        {
            var variable = GetEnvironmentVariable(name);
            if (StringExtensions.EqualsIgnoreCase(variable, "true"))
            {
                return true;
            }
            else if (StringExtensions.EqualsIgnoreCase(variable, "false"))
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
            var values = GetEnvironmentVariable(name);
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