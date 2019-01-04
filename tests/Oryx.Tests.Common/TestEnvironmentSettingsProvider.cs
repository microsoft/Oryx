// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator;

namespace Oryx.Tests.Common
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
