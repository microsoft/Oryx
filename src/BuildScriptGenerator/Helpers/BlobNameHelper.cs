// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator
{
  internal static class BlobNameHelper
  {
    /// <summary>
    /// Returns the expected SDK binary blob name for the given platform name, version and debian flavor.
    /// </summary>
    public static string GetBlobNameForVersion(string platformName, string version, string debianFlavor)
    {
      if (debianFlavor.Equals(OsTypes.DebianStretch, StringComparison.OrdinalIgnoreCase))
      {
        return $"{platformName}-{version}.tar.gz";
      }
      else
      {
        return $"{platformName}-{debianFlavor}-{version}.tar.gz";
      }
    }
  }
}