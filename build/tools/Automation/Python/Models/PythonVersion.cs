// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;

namespace Microsoft.Oryx.Automation.Python.Models
{
    public class PythonVersion : IComparable
    {
        public string Version { get; set; } = string.Empty;

        public string GpgKey { get; set; } = string.Empty;

        public int CompareTo(object obj)
        {
            var objPythonVersion = obj as PythonVersion;
            if (objPythonVersion == null)
            {
                return 1;
            }

            var thisVersion = new Version(this.Version);
            var objVersion = new Version(objPythonVersion.Version);
            return thisVersion.CompareTo(objVersion);
        }
    }
}
