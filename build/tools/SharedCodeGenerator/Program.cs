// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Oryx.SharedCodeGenerator.Outputs;

namespace Microsoft.Oryx.SharedCodeGenerator
{
    public class Program
    {
        private const int ArgInput = 0;
        private const int ArgOutputBase = 1;
        private const int ExitSuccess = 0;
        private const int ExitFailure = 1;

        private const string VarPrefix = "${";
        private const string VarSuffix = "}";

        public static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine($"Usage: {AppDomain.CurrentDomain.FriendlyName} <input YAML path> <output base path>");
                return ExitFailure;
            }

            return GenerateSharedCode(args[ArgInput], args[ArgOutputBase]);
        }

        public static string BuildAutogenDisclaimer(string inputFile)
        {
            inputFile = Path.GetFileName(inputFile);
            return $"This file was auto-generated from '{inputFile}'. Changes may be overridden.";
        }

        private static int GenerateSharedCode(string inputPath, string outputBasePath)
        {
            if (!File.Exists(inputPath))
            {
                Console.Error.WriteLine("Input path is not an existing file.");
                return ExitFailure;
            }

            if (!Directory.Exists(outputBasePath))
            {
                Console.Error.WriteLine("Output path base is not an existing directory.");
                return ExitFailure;
            }

            var input = LoadFromString<List<ConstantCollection>>(File.ReadAllText(inputPath));

            foreach (ConstantCollection col in input)
            {
                ReplaceVariablesWithValues(col.Constants);
                col.SourcePath = inputPath; // This isn't set by YamlDotNet, so it's added manually

                foreach (Dictionary<string, string> outputInfo in col.Outputs)
                {
                    IOutputFile output = OutputFactory.CreateByType(outputInfo, col);
                    string filePath = Path.Combine(outputBasePath, output.GetPath());
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        Console.WriteLine("Writing file '{0}'", filePath);
                        writer.Write(output.GetContent());
                    }
                }
            }

            return ExitSuccess;
        }

        private static void ReplaceVariablesWithValues(IDictionary<string, string> dict)
        {
            var colReplacements = dict
                        .Where(e => e.Value.StartsWith(VarPrefix) && e.Value.EndsWith(VarSuffix))
                        .Select(e => KeyValuePair.Create(e.Key, e.Value.Substring(VarPrefix.Length, e.Value.Length - VarPrefix.Length - VarSuffix.Length)))
                        .ToList();
            foreach (var entry in colReplacements)
            {
                dict[entry.Key] = dict[entry.Value];
            }
        }

        private static T LoadFromString<T>(string content)
        {
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithNamingConvention(new YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention())
                .Build();
            var obj = deserializer.Deserialize<T>(content);
            return obj;
        }
    }
}
