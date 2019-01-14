// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Oryx.Tests.Common
{
    public class TestEnvironment : IEnvironment
    {
        // Environment variables in Linux are case-sensitive
        public Dictionary<string, string> Variables { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

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