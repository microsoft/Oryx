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
        /// <summary>
        /// This method adds environment variables for the staging storage to a collection. 
        /// It adds the URL of the staging storage and the sasToken to access the storage.
        /// </summary>
        /// <param name="envVarList"> A Collection of EnvironmentVariable objects. The storage environment variables are be added here</param>
        /// <returns>The method returns the collection with the newly added environment variables.</returns>
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
