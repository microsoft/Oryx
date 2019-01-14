// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Oryx.BuildImage.Tests
{
    /// <summary>
    /// The methods here represent the tests that every supported language must have.
    /// </summary>
    public abstract class SampleAppsTestBase
    {
        public abstract void GeneratesScript_AndBuilds();
        public abstract void Builds_AndCopiesContentToOutputDirectory_Recursively();
        public abstract void Build_CopiesOutput_ToNestedOutputDirectory();
        public abstract void Build_ReplacesContentInDestinationDir_WhenDestinationDirIsNotEmpty();
        public abstract void ErrorDuringBuild_ResultsIn_NonSuccessfulExitCode();
        public abstract void GeneratesScript_AndBuilds_WhenExplicitLanguageAndVersion_AreProvided();
        public abstract void CanBuild_UsingScriptGeneratedBy_ScriptOnlyOption();
        public abstract void GeneratesScript_AndBuilds_UsingSuppliedIntermediateDir();
        public abstract void GeneratesScriptAndBuilds_WhenSourceAndDestinationFolders_AreSame();
        public abstract void GeneratesScriptAndBuilds_WhenDestination_IsSubDirectoryOfSource();
    }
}
