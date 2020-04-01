// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    public class BuildStepNotProvidedException : InvalidUsageException
    {
        public BuildStepNotProvidedException(string message)
            : base(message)
        {
        }
    }
}
