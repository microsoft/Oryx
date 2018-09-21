// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator
{
    public interface ISourceRepoProvider
    {
        ISourceRepo GetSourceRepo();
    }
}
