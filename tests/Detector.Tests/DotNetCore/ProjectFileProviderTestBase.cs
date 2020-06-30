// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.Detector.DotNetCore
{
    public abstract class ProjectFileProviderTestBase : IClassFixture<TestTempDirTestFixture>
    {
        protected const string AzureFunctionsProjectFile = @"
        <Project Sdk=""Microsoft.NET.Sdk"">
          <PropertyGroup>
            <TargetFramework>netcoreapp2.1</TargetFramework>
            <AzureFunctionsVersion>v2</AzureFunctionsVersion>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""Microsoft.NET.Sdk.Functions"" Version=""1.0.28"" />
          </ItemGroup>
          <ItemGroup>
            <None Update=""host.json"">
              <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
            </None>
            <None Update=""local.settings.json"">
              <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
              <CopyToPublishDirectory>Never</CopyToPublishDirectory>
            </None>
          </ItemGroup>
        </Project>
        ";

        protected const string AzureFunctionsProjectFileWithoutAzureFunctionsVersionProperty = @"
        <Project Sdk=""Microsoft.NET.Sdk"">
          <PropertyGroup>
            <TargetFramework>netcoreapp2.1</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""Microsoft.NET.Sdk.Functions"" Version=""1.0.28"" />
          </ItemGroup>
          <ItemGroup>
            <None Update=""host.json"">
              <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
            </None>
            <None Update=""local.settings.json"">
              <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
              <CopyToPublishDirectory>Never</CopyToPublishDirectory>
            </None>
          </ItemGroup>
        </Project>
        ";

        protected const string NonWebSdkProjectFile = @"
        <Project Sdk=""Microsoft.NET.Sdk.Razor"">
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
            <TargetFramework>netcoreapp2.1</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""xunit"" Version=""2.3.1"" />
          </ItemGroup>
        </Project>";

        protected const string WebSdkProjectFile = @"
        <Project Sdk=""Microsoft.NET.Sdk.Web"">
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
            <TargetFramework>netcoreapp2.1</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""Microsoft.AspNetCore"" Version=""2.1.0"" />
          </ItemGroup>
        </Project>";

        protected const string WebSdkProjectFileWithVersion = @"
        <Project Sdk=""Microsoft.NET.Sdk.Web/1.0.0"">
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
            <TargetFramework>netcoreapp2.1</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""Microsoft.AspNetCore"" Version=""2.1.0"" />
          </ItemGroup>
        </Project>";

        protected const string NonWebSdkProjectFileWithVersion = @"
        <Project Sdk=""Microsoft.NET.Sdk/1.0.0"">
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
            <TargetFramework>netcoreapp2.1</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""Microsoft.AspNetCore"" Version=""2.1.0"" />
          </ItemGroup>
        </Project>";

        protected const string WebSdkProjectFileWithSdkInfoAsElement = @"
        <Project>
          <Sdk Name=""Microsoft.NET.Sdk.Web"" Version=""1.0.0"" />
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
            <TargetFramework>netcoreapp2.1</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""Microsoft.AspNetCore"" Version=""2.1.0"" />
          </ItemGroup>
        </Project>";

        protected const string NonWebSdkProjectFileWithSdkInfoAsElement = @"
        <Project>
          <Sdk Name=""Microsoft.NET.Sdk"" Version=""1.0.0"" />
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
            <TargetFramework>netcoreapp2.1</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""Microsoft.AspNetCore"" Version=""2.1.0"" />
          </ItemGroup>
        </Project>";

        protected const string NoSdkInformationProjectFile = @"
        <Project>
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
            <TargetFramework>netcoreapp2.1</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""Microsoft.AspNetCore"" Version=""2.1.0"" />
          </ItemGroup>
        </Project>";

        protected const string WebSdkProjectFileWithMultipleSdkInfoAsElement = @"
        <Project Sdk=""Microsoft.NET.Sdk/1.0.0"">
          <Sdk Name=""Microsoft.NET.Sdk.Web"" Version=""1.0.0"" />
          <PropertyGroup>
            <LangVersion>7.3</LangVersion>
            <TargetFramework>netcoreapp2.1</TargetFramework>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include=""Microsoft.AspNetCore"" Version=""2.1.0"" />
          </ItemGroup>
        </Project>";

        protected readonly string _tempDirRoot;

        public ProjectFileProviderTestBase(TestTempDirTestFixture testFixture)
        {
            _tempDirRoot = testFixture.RootDirPath;
        }

        protected DefaultProjectFileProvider GetProjectFileProvider(
            DetectorOptions options = null)
        {
            if (options == null)
            {
                options = new DetectorOptions();
            }

            var providers = new IProjectFileProvider[]
            {
                new ExplicitProjectFileProvider(
                    Options.Create(options),
                    NullLogger<ExplicitProjectFileProvider>.Instance),
                new RootDirectoryProjectFileProvider(NullLogger<RootDirectoryProjectFileProvider>.Instance),
                new ProbeAndFindProjectFileProvider(NullLogger<ProbeAndFindProjectFileProvider>.Instance),
            };

            return new DefaultProjectFileProvider(providers);
        }

        protected DetectorContext GetContext(ISourceRepo sourceRepo)
        {
            return new DetectorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        protected string CreateSourceRepoDir()
        {
            return Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString("N"))).FullName;
        }

        protected string CreateDir(string parentDir, string newDirName)
        {
            return Directory.CreateDirectory(Path.Combine(parentDir, newDirName)).FullName;
        }

        protected DetectorContext CreateContext(ISourceRepo sourceRepo)
        {
            return new DetectorContext
            {
                SourceRepo = sourceRepo
            };
        }

        protected LocalSourceRepo CreateSourceRepo(string sourceDir)
        {
            return new LocalSourceRepo(sourceDir, NullLoggerFactory.Instance);
        }
    }
}
