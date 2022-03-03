// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Oryx.BuildServer.Models;

namespace Microsoft.Oryx.BuildServer.Services
{
    public interface IBuildService
    {
        Task<Build> StartBuildAsync(Build build);

        Task<Build> MarkCompletedAsync(Build build);

        Task<Build> MarkCancelledAsync(Build build);

        Task<Build> MarkFailedAsync(Build build);

        Task<Build> GetBuildAsync(string id);
    }
}
