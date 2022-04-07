// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Oryx.BuildServer.Models;
using Microsoft.Oryx.BuildServer.Repositories;
using Microsoft.Oryx.BuildServer.Services.ArtifactBuilders;

namespace Microsoft.Oryx.BuildServer.Services
{
    public class BuildService : IBuildService
    {
        private readonly IRepository buildRepository;
        private readonly IArtifactBuilderFactory artifactBuilderFactory;
        private readonly IBuildRunner buildRunner;

        public BuildService(IRepository buildRepository, IArtifactBuilderFactory artifactBuilderFactory, IBuildRunner buildRunner)
        {
            this.buildRepository = buildRepository;
            this.artifactBuilderFactory = artifactBuilderFactory;
            this.buildRunner = buildRunner;
        }

        public async Task<Build> StartBuildAsync(Build build)
        {
            build.Status = "IN_PROGRESS";
            await this.buildRepository.InsertAsync(build);
            var artifactBuilder = this.artifactBuilderFactory.CreateArtifactBuilder(build);
            this.buildRunner.RunInBackground(artifactBuilder, build, this.MarkCompletedAsync, this.MarkFailedAsync);
            return build;
        }

#pragma warning disable CS1998 // Keep asynchronous for backwards-compatibility
        public async Task<Build> GetBuildAsync(string id)
#pragma warning restore CS1998
        {
            var build = this.buildRepository.GetById(id);
            return build;
        }

        public async Task<Build> MarkCancelledAsync(Build build)
        {
            build.Status = "CANCELLED";
            await this.buildRepository.UpdateAsync(build);
            return build;
        }

        public async Task<Build> MarkCompletedAsync(Build build)
        {
            build.Status = "COMPLETED";
            await this.buildRepository.UpdateAsync(build);
            return build;
        }

        public async Task<Build> MarkFailedAsync(Build build)
        {
            build.Status = "FAILED";
            await this.buildRepository.UpdateAsync(build);
            return build;
        }
    }
}
