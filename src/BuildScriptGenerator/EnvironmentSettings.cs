// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class EnvironmentSettings
    {
        // Note: These two keys exist so that we do not break existing users who might still be using them
        public string PreBuildScriptPath { get; set; }

        public string PostBuildScriptPath { get; set; }

        public string PreBuildScript { get; set; }

        public string PostBuildScript { get; set; }
    }
}
