// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace Microsoft.Oryx.BuildScriptGenerator.Common
{
    public static class KeyVaultHelper
    {
        public static string GetKeyVaultSecretValue(string keyVaultUri, string secretName)
        {
            var client = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
            var sasToken = client.GetSecret(secretName).Value.Value;
            return sasToken;
        }
    }
}