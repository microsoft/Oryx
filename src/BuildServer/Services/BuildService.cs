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
        private readonly IRepository _buildRepository;
        private readonly IArtifactBuilderFactory _artifactBuilderFactory;
        private readonly IBuildRunner _buildRunner;

        public BuildService(IRepository buildRepository, IArtifactBuilderFactory artifactBuilderFactory, IBuildRunner buildRunner)
        {
            _buildRepository = buildRepository;
            _artifactBuilderFactory = artifactBuilderFactory;
            _buildRunner = buildRunner;
        }

        public async Task<Build> StartBuildAsync(Build build)
        {
            build.Status = "IN_PROGRESS";
            await _buildRepository.InsertAsync(build);
            var artifactBuilder = _artifactBuilderFactory.CreateArtifactBuilder(build);
            _buildRunner.RunInBackground(artifactBuilder, build, MarkCompletedAsync, MarkFailedAsync);
            return build;
        }

#pragma warning disable CS1998 // Keep asynchronous for backwards-compatibility
        public async Task<Build> GetBuildAsync(string id)
#pragma warning restore CS1998
        {
            var build = _buildRepository.GetById(id);
            return build;
        }

        public async Task<Build> MarkCancelledAsync(Build build)
        {
            build.Status = "CANCELLED";
            await _buildRepository.UpdateAsync(build);
            return build;
        }

        public async Task<Build> MarkCompletedAsync(Build build)
        {
            build.Status = "COMPLETED";
            await _buildRepository.UpdateAsync(build);
            return build;
        }

        public async Task<Build> MarkFailedAsync(Build build)
        {
            build.Status = "FAILED";
            await _buildRepository.UpdateAsync(build);
            return build;
        }
    }
}
