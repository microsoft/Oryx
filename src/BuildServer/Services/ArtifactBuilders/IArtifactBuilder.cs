// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildServer.Models;

namespace Microsoft.Oryx.BuildServer.Services.ArtifactBuilders
{
    public interface IArtifactBuilder
    {
        bool Build(Build build);
    }
}
