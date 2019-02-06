// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.Tests.Common
{
    public class TestEnvironment : IEnvironment
    {
        // Environment variables in Linux are case-sensitive
        public Dictionary<string, string> Variables { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

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

        public string GetEnvironmentVariable(string name)
        {
            if (Variables.TryGetValue(name, out var value))
            {
                return value;
            }
            return null;
        }

        public IList<string> GetEnvironmentVariableAsList(string name)
        {
            return null;
        }

        public IDictionary GetEnvironmentVariables()
        {
            return Variables;
        }

        public void SetEnvironmentVariable(string name, string value)
        {
            Variables[name] = value;
        }
    }
}