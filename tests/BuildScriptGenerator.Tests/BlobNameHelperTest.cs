// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
  public class BlobNameHelperTest
  {
    [Theory]
    [InlineData("python", "2.7.15", "stretch", "python-2.7.15.tar.gz")]
    [InlineData("python", "3.10.0", "buster", "python-buster-3.10.0.tar.gz")]
    [InlineData("nodejs", "20.11.0", "bookworm", "nodejs-bookworm-20.11.0.tar.gz")]
    [InlineData("nodejs", "24.0.0", "trixie", "nodejs-trixie-24.0.0.tar.gz")]
    public void GetBlobName(string platformName, string version, string debianFlavor, string expectedBlobName)
    {
      // Arrange
      var blobName = BlobNameHelper.GetBlobNameForVersion(platformName, version, debianFlavor);

      // Act
      Assert.Equal(expectedBlobName, blobName);
    }
  }
}