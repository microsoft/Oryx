// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Oryx.SharedCodeGenerator.Outputs
{
    internal class OutputFactory
    {
        private static readonly Dictionary<string, Type> OutputsByType = CreateOutputsDictionary();

        public static IOutputFile CreateByType(Dictionary<string, string> typeInfo, ConstantCollection constantCollection)
        {
            string typeName = typeInfo["type"];
            typeInfo.Remove("type");

            IOutputFile outputFile = Activator.CreateInstance(OutputsByType[typeName]) as IOutputFile;
            outputFile.Initialize(constantCollection, typeInfo);
            return outputFile;
        }

        private static Dictionary<string, Type> CreateOutputsDictionary()
        {
            var outputsWithType = from type in Assembly.GetExecutingAssembly().GetTypes()
                                  where typeof(IOutputFile).IsAssignableFrom(type)
                                  where !type.IsAbstract && !type.IsInterface
                                  let attr = type.GetCustomAttributes(typeof(OutputTypeAttribute), false).First() as OutputTypeAttribute
                                  select KeyValuePair.Create(attr.Type, type);

            return outputsWithType.ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}
