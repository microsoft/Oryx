// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.SharedCodeGenerator.Outputs
{
    internal interface IOutputFile
    {
        void Initialize(ConstantCollection constantCollection, Dictionary<string, string> typeInfo);

        string GetPath();

        string GetContent();
    }
}
