// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal interface IEnvironmentSettingsProvider
    {
        bool TryGetAndLoadSettings(out EnvironmentSettings environmentSettings);
    }
}
