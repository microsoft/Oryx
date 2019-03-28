// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal class DefaultEnvironment : IEnvironment
    {
        public bool? GetBoolEnvironmentVariable(string name)
        {
            var variable = GetEnvironmentVariable(name);
            if (!string.IsNullOrEmpty(variable))
            {
                if (variable.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
                else if (variable.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
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
            var values = Environment.GetEnvironmentVariable(name);
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