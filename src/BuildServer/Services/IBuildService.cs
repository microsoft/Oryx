// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildServer.Models;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildServer.Services
{
    public interface IBuildService
    {
        Task<Build> StartBuild(Build build);
        Task<Build> MarkCompleted(Build build);
        Task<Build> MarkCancelled(Build build);
        Task<Build> MarkFailed(Build build);
        Task<Build> GetBuild(string id);
    }
}
