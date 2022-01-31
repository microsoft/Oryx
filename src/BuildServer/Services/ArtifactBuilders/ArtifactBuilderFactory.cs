using Microsoft.Oryx.BuildServer.Models;

namespace Microsoft.Oryx.BuildServer.Services.ArtifactBuilders
{
    public class ArtifactBuilderFactory : IArtifactBuilderFactory
    {
        private IArtifactBuilder _builder;

        public ArtifactBuilderFactory(IArtifactBuilder builder)
        {
            _builder = builder;
        }
        public IArtifactBuilder CreateArtifactBuilder(Build build)
        {
            return _builder;
        }
    }
}
