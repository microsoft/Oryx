// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Oryx.BuildServer.Models;
using Microsoft.Oryx.BuildServer.Services.ArtifactBuilders;

namespace Microsoft.Oryx.BuildServer.Services
{
    public delegate Task<Build> Callback(Build build);

    public interface IBuildRunner
    {
        void RunInBackground(IArtifactBuilder builder, Build build, Callback successCallback, Callback failureCallback);
    }
}
