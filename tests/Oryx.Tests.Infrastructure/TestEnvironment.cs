// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Oryx.Tests.Infrastructure
{
    public class TestEnvironment : IEnvironment
    {
        public Dictionary<string, string> Variables { get; } = new Dictionary<string, string>();

        public string GetEnvironmentVariable(string name)
        {
            if (Variables.TryGetValue(name, out var value))
            {
                return value;
            }
            return null;
        }
    }
}
