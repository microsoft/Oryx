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
        /// It adds the URL of the staging storage.
        /// </summary>
        /// <param name="envVarList"> A Collection of EnvironmentVariable objects. The storage environment variables are be added here</param>
        /// <returns>The method returns the collection with the newly added environment variables.</returns>
        public static ICollection<EnvironmentVariable> AddTestStorageAccountEnvironmentVariables(this ICollection<EnvironmentVariable> envVarList)
        {
            return envVarList;
        }
    }
}
