// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.Tests.Common
{
    public class TestEnvironment : IEnvironment
    {
        // Environment variables in Linux are case-sensitive
        public Dictionary<string, string> Variables { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

        public EnvironmentType Type
        {
            get
            {
                foreach (var entry in LoggingConstants.OperationNameSourceEnvVars)
                {
                    if (!string.IsNullOrEmpty(GetEnvironmentVariable(entry.Value)))
                    {
                        return entry.Key;
                    }
                }

                return EnvironmentType.Unknown;
            }
        }

        public bool? GetBoolEnvironmentVariable(string name)
        {
            var variable = GetEnvironmentVariable(name);
            if (!string.IsNullOrEmpty(variable))
            {
                if (variable.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                    return true;
                if (variable.Equals("false", StringComparison.CurrentCultureIgnoreCase))
                    return false;
            }
            return null;
        }

        public string GetEnvironmentVariable(string name, string defaultValue = null)
        {
            if (Variables.TryGetValue(name, out var value))
            {
                return value;
            }
            return defaultValue;
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
            return Variables;
        }

        public void SetEnvironmentVariable(string name, string value)
        {
            Variables[name] = value;
        }

        public string[] GetCommandLineArgs()
        {
            return new string[] { };
        }
    }
}