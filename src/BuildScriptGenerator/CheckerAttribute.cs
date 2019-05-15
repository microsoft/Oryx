using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CheckerAttribute : Attribute
    {
        private readonly string[] _targetToolNames;

        public CheckerAttribute(params string[] toolNames)
        {
            _targetToolNames = toolNames;
        }
    }
}
