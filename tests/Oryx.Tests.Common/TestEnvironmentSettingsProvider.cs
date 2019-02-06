// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.Tests.Common
{
    public class TestEnvironmentSettingsProvider : IEnvironmentSettingsProvider
    {
        public bool TryGetAndLoadSettings(out EnvironmentSettings environmentSettings)
        {
            environmentSettings = null;
            return true;
        }
    }
}
