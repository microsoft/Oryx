// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Oryx.Tests.Common
{
    public static class EnvironmentVariableListExtensions
    {
        public static ICollection<EnvironmentVariable> AddTestStorageAccountEnvironmentVariables(this ICollection<EnvironmentVariable> envVarList)
        {
            var testStorageAccountUrl = Environment.GetEnvironmentVariable(SdkStorageConstants.TestingSdkStorageUrlKeyName);
            var sdkStorageUrl = string.IsNullOrEmpty(testStorageAccountUrl) ? SdkStorageConstants.PrivateStagingSdkStorageBaseUrl : testStorageAccountUrl;

            envVarList.Add(new EnvironmentVariable(SdkStorageConstants.SdkStorageBaseUrlKeyName, sdkStorageUrl));

            if (sdkStorageUrl == SdkStorageConstants.PrivateStagingSdkStorageBaseUrl)
            {
                string stagingStorageSasToken = Environment.GetEnvironmentVariable(SdkStorageConstants.PrivateStagingStorageSasTokenKey) ??
                KeyVaultHelper.GetKeyVaultSecretValue(SdkStorageConstants.OryxKeyvaultUri, SdkStorageConstants.StagingStorageSasTokenKeyvaultSecretName);
                envVarList.Add(new EnvironmentVariable(SdkStorageConstants.PrivateStagingStorageSasTokenKey, stagingStorageSasToken));
            }

            return envVarList;
        }
    }
}
