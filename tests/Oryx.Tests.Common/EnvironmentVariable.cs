// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;

namespace Oryx.Tests.Common
{
    public class EnvironmentVariable
    {
        public EnvironmentVariable(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.");
            }

            Key = key;
            Value = value;
        }

        public string Key { get; }

        public string Value { get; }
    }
}
