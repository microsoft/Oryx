// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Microsoft.Oryx.SharedCodeGenerator.Outputs
{
    internal class OutputFactory
    {
        private static readonly Dictionary<string, Type> _outputsByType;

        static OutputFactory()
        {
            var outputsWithType = from type in Assembly.GetExecutingAssembly().GetTypes()
                                  where typeof(IOutputFile).IsAssignableFrom(type)
                                  where !type.IsAbstract && !type.IsInterface
                                  let attr = type.GetCustomAttributes(typeof(OutputTypeAttribute), false).First() as OutputTypeAttribute
                                  select KeyValuePair.Create(attr.Type, type);

            _outputsByType = outputsWithType.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public static IOutputFile CreateByType(Dictionary<string, string> typeInfo, ConstantCollection constantCollection)
        {
            string typeName = typeInfo["type"];
            typeInfo.Remove("type");

            IOutputFile outputFile = Activator.CreateInstance(_outputsByType[typeName]) as IOutputFile;
            outputFile.Initialize(constantCollection, typeInfo);
            return outputFile;
        }
    }
}
