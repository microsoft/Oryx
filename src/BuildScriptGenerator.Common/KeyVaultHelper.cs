// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace Microsoft.Oryx.BuildScriptGenerator.Common
{
    public static class KeyVaultHelper
    {
        /// <summary>
        /// This method retrieves a secret from KeyVault given the KeyVaultUri and SecretName.
        /// If it fails to retrieve the token then throws an exception.
        /// </summary>
        /// <param name="keyVaultUri">Uri of the KeyVault. For Oryx it is: https://oryx.vault.azure.net</param>
        /// <param name="secretName">Name of the secret in KeyVault</param>
        /// <returns>the retrieved sasToken</returns>
        public static string GetKeyVaultSecretValue(string keyVaultUri, string secretName)
        {
            var client = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
            try
            {
                var sasToken = client.GetSecret(secretName).Value.Value;
                return sasToken;
            }
            catch (RequestFailedException e)
            {
                throw new InvalidOperationException($"Failed to get secret from KeyVault: {e.Message}. Status code: {e.Status}. KeyVault URI: {client.VaultUri}. Secret name: {secretName}. ", e);
            }
        }
    }
}