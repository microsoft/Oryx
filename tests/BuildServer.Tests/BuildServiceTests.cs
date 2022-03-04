// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildServer.Models;
using Microsoft.Oryx.BuildServer.Repositories;
using Microsoft.Oryx.BuildServer.Services;
using Microsoft.Oryx.BuildServer.Services.ArtifactBuilders;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Oryx.BuildServer.Tests
{
    public class BuildServiceTests
    {
        [Fact]
        public async Task Test_CorrectBuildStatus_OnStartBuild()
        {
            // Arrange
            var build = TestBuild();
            var mockedBuildRepository = new Mock<IRepository>();
            var mockedArtifactBuilderFactory = new Mock<IArtifactBuilderFactory>();
            var mockedArtifactBuilder = new Mock<IArtifactBuilder>();
            var mockedBuildRunner = new Mock<IBuildRunner>();
            mockedArtifactBuilder.Setup(x => x.Build(build)).Returns(true);
            mockedArtifactBuilderFactory.Setup(x => x.CreateArtifactBuilder(build)).Returns(mockedArtifactBuilder.Object);
            mockedBuildRunner.Setup(x => x.RunInBackground(mockedArtifactBuilder.Object, build, null, null));
            var testBuildService = TestBuildService(
                mockedBuildRepository.Object, mockedArtifactBuilderFactory.Object, mockedBuildRunner.Object);

            // Act
            var buildTest = await testBuildService.StartBuild(build);

            // Assert
            Assert.NotNull(buildTest);
            Assert.Equal("IN_PROGRESS", buildTest.Status);
        }

        [Fact]
        public async Task Test_GetById_CorrectBuildStatus()
        {
            // Arrange
            var build = TestBuild();
            var mockedBuildRepository = new Mock<IRepository>();
            mockedBuildRepository.Setup(x => x.GetById(build.Id)).Returns(build);
            var mockedArtifactBuilderFactory = new Mock<IArtifactBuilderFactory>();
            var mockedBuildRunner = new Mock<IBuildRunner>();
            var testBuildService = TestBuildService(
                mockedBuildRepository.Object, mockedArtifactBuilderFactory.Object, mockedBuildRunner.Object);

            // Act
            var buildTest = await testBuildService.GetBuild(build.Id);

            // Assert
            Assert.NotNull(buildTest);
            Assert.Equal("testId", buildTest.Id);
            Assert.Equal("testStatus", buildTest.Status);
        }

        [Fact]
        public async Task Test_MarkedCancelled_GetsCorrectBuildStatus()
        {
            // Arrange
            var build = TestBuild();
            var mockedBuildRepository = new Mock<IRepository>();
            var mockedArtifactBuilderFactory = new Mock<IArtifactBuilderFactory>();
            var mockedBuildRunner = new Mock<IBuildRunner>();
            var testBuildService = TestBuildService(
                mockedBuildRepository.Object, mockedArtifactBuilderFactory.Object, mockedBuildRunner.Object);

            // Act
            var buildTest = await testBuildService.MarkCancelled(build);

            // Assert
            Assert.NotNull(buildTest);
            Assert.Equal("CANCELLED", buildTest.Status);
        }

        [Fact]
        public async Task Test_MarkedCompleted_GetsCorrectBuildStatus()
        {
            // Arrange
            var build = TestBuild();
            var mockedBuildRepository = new Mock<IRepository>();
            var mockedArtifactBuilderFactory = new Mock<IArtifactBuilderFactory>();
            var mockedBuildRunner = new Mock<IBuildRunner>();
            var testBuildService = TestBuildService(
                mockedBuildRepository.Object, mockedArtifactBuilderFactory.Object, mockedBuildRunner.Object);

            // Act
            var buildTest = await testBuildService.MarkCompleted(build);

            // Assert
            Assert.NotNull(buildTest);
            Assert.Equal("COMPLETED", buildTest.Status);
        }

        [Fact]
        public async Task Test_MarkedFailed_GetsCorrectBuildStatus()
        {
            // Arrange
            var build = TestBuild();
            var mockedBuildRepository = new Mock<IRepository>();
            var mockedArtifactBuilderFactory = new Mock<IArtifactBuilderFactory>();
            var mockedBuildRunner = new Mock<IBuildRunner>();
            var testBuildService = TestBuildService(
                mockedBuildRepository.Object, mockedArtifactBuilderFactory.Object, mockedBuildRunner.Object);

            // Act
            var buildTest = await testBuildService.MarkFailed(build);

            // Assert
            Assert.NotNull(buildTest);
            Assert.Equal("FAILED", buildTest.Status);
        }

        private static Build TestBuild()
        {
            var build = new Build();
            build.Id = "testId";
            build.Status = "testStatus";
            build.Platform = "testPlatform";
            build.Version = "1.2.3";
            build.SourcePath = "/test/sourcePath";
            build.OutputPath = "/test/outputPath";
            build.LogPath = "/test/logPath";

            return build;
        }

        private static BuildService TestBuildService(
            IRepository testIRepository,
            IArtifactBuilderFactory testIArtifactBuilderFactory,
            IBuildRunner testIBuildRunner)
        {
            return new BuildService(testIRepository, testIArtifactBuilderFactory, testIBuildRunner);
        }
    }
}
