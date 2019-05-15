using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CheckerAttribute : Attribute
    {
        private readonly Type _targetPlatform;

        public CheckerAttribute(Type targetPlatform)
        {
            _targetPlatform = targetPlatform;
        }
    }
}
