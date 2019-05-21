// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public interface IEnvironmentSettingsProvider
    {
        bool TryGetAndLoadSettings(out EnvironmentSettings environmentSettings);
    }
}
