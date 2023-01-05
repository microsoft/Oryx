// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Oryx.BuildServer.Models;
using Microsoft.Oryx.BuildServer.Repositories;
using Microsoft.Oryx.BuildServer.Services;
using Microsoft.Oryx.BuildServer.Services.ArtifactBuilders;
using Moq;
using Xunit;

namespace Microsoft.Oryx.BuildServer.Tests
{
    public class BuildServiceTests
    {
        [Fact]
        public async Task Test_CorrectBuildStatus_OnStartBuildAsync()
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
            var buildTest = await testBuildService.StartBuildAsync(build);

            // Assert
            Assert.NotNull(buildTest);
            Assert.Equal("IN_PROGRESS", buildTest.Status);
        }

        [Fact]
        public async Task Test_GetById_CorrectBuildStatusAsync()
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
            var buildTest = await testBuildService.GetBuildAsync(build.Id);

            // Assert
            Assert.NotNull(buildTest);
            Assert.Equal("testId", buildTest.Id);
            Assert.Equal("testStatus", buildTest.Status);
        }

        [Fact]
        public async Task Test_MarkedCancelled_GetsCorrectBuildStatusAsync()
        {
            // Arrange
            var build = TestBuild();
            var mockedBuildRepository = new Mock<IRepository>();
            var mockedArtifactBuilderFactory = new Mock<IArtifactBuilderFactory>();
            var mockedBuildRunner = new Mock<IBuildRunner>();
            var testBuildService = TestBuildService(
                mockedBuildRepository.Object, mockedArtifactBuilderFactory.Object, mockedBuildRunner.Object);

            // Act
            var buildTest = await testBuildService.MarkCancelledAsync(build);

            // Assert
            Assert.NotNull(buildTest);
            Assert.Equal("CANCELLED", buildTest.Status);
        }

        [Fact]
        public async Task Test_MarkedCompleted_GetsCorrectBuildStatusAsync()
        {
            // Arrange
            var build = TestBuild();
            var mockedBuildRepository = new Mock<IRepository>();
            var mockedArtifactBuilderFactory = new Mock<IArtifactBuilderFactory>();
            var mockedBuildRunner = new Mock<IBuildRunner>();
            var testBuildService = TestBuildService(
                mockedBuildRepository.Object, mockedArtifactBuilderFactory.Object, mockedBuildRunner.Object);

            // Act
            var buildTest = await testBuildService.MarkCompletedAsync(build);

            // Assert
            Assert.NotNull(buildTest);
            Assert.Equal("COMPLETED", buildTest.Status);
        }

        [Fact]
        public async Task Test_MarkedFailed_GetsCorrectBuildStatusAsync()
        {
            // Arrange
            var build = TestBuild();
            var mockedBuildRepository = new Mock<IRepository>();
            var mockedArtifactBuilderFactory = new Mock<IArtifactBuilderFactory>();
            var mockedBuildRunner = new Mock<IBuildRunner>();
            var testBuildService = TestBuildService(
                mockedBuildRepository.Object, mockedArtifactBuilderFactory.Object, mockedBuildRunner.Object);

            // Act
            var buildTest = await testBuildService.MarkFailedAsync(build);

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
