using Microsoft.Oryx.BuildServer.Models;
using Microsoft.Oryx.BuildServer.Repositories;
using Microsoft.Oryx.BuildServer.Services.ArtifactBuilders;
using System.Threading.Tasks;

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

        public async Task<Build> StartBuild(Build build)
        {
            build.Status = "IN_PROGRESS";
            await _buildRepository.Insert(build);
            var artifactBuilder = _artifactBuilderFactory.CreateArtifactBuilder(build);
            _buildRunner.RunInBackground(artifactBuilder, build, MarkCompleted, MarkFailed);
            return build;
        }

        public async Task<Build> GetBuild(string id)
        {
            var build = _buildRepository.GetById(id);
            return build;
        }

        public async Task<Build> MarkCancelled(Build build)
        {
            build.Status = "CANCELLED";
            await _buildRepository.Update(build);
            return build;
        }

        public async Task<Build> MarkCompleted(Build build)
        {
            build.Status = "COMPLETED";
            await _buildRepository.Update(build);
            return build;
        }

        public async Task<Build> MarkFailed(Build build)
        {
            build.Status = "FAILED";
            await _buildRepository.Update(build);
            return build;
        }
    }
}
