// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildServer.Models;

namespace Microsoft.Oryx.BuildServer.Services.ArtifactBuilders
{
    public class ArtifactBuilderFactory : IArtifactBuilderFactory
    {
        private IArtifactBuilder builder;

        public ArtifactBuilderFactory(IArtifactBuilder builder)
        {
            this.builder = builder;
        }

        public IArtifactBuilder CreateArtifactBuilder(Build build)
        {
            return this.builder;
        }
    }
}
