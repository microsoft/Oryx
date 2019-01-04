// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public interface IEnvironmentSettingsProvider
    {
        bool TryGetAndLoadSettings(out EnvironmentSettings environmentSettings);
    }
}
