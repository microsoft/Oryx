// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Oryx.BuildScriptGenerator.SourceRepo
{
    public static class ISourceRepoJsonExtensions
    {
        public static dynamic ReadJsonObjectFromFile(this ISourceRepo sourceRepo, string fileName)
        {
            var jsonContent = sourceRepo.ReadFile(fileName);
            return JsonConvert.DeserializeObject(jsonContent);
        }
    }
}
