// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Oryx.BuildScriptGenerator.SourceRepo
{
    internal static class SourceRepoFileHelpers
    {
        internal static dynamic ReadJsonObjectFromFile(ISourceRepo sourceRepo, string fileName)
        {
            var jsonContent = sourceRepo.ReadFile(fileName);
            return JsonConvert.DeserializeObject(jsonContent);
        }
    }
}
