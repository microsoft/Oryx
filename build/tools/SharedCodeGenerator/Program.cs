// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                Console.WriteLine(
                    $"Usage: {AppDomain.CurrentDomain.FriendlyName} <input YAML path> <output base path>");
                return ExitFailure;
            }

            return GenerateSharedCode(args[ArgInput], args[ArgOutputBase]);
        }

        private static int GenerateSharedCode(string inputPath, string outputBasePath)
        {
            var inputFiles = inputPath.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(path => path.Trim());
            var errors = new StringBuilder();
            foreach (var inputFile in inputFiles)
            {
                if (!File.Exists(inputPath))
                {
                    errors.AppendLine($"File {inputFile} does not exist.");
                }
            }

            if (errors.Length > 0)
            {
                Console.Error.WriteLine(errors.ToString());
                return ExitFailure;
            }

            if (!Directory.Exists(outputBasePath))
            {
                Console.Error.WriteLine("Output path base is not an existing directory.");
                return ExitFailure;
            }

            var collections = new List<ConstantCollection>();
            foreach (var inputFile in inputFiles)
            {
                var inputFileContent = File.ReadAllText(inputFile);
                var deserializedInput = LoadFromString<List<ConstantCollection>>(inputFileContent);
                deserializedInput.ForEach(collection => collection.SourcePath = inputFile);
                collections.AddRange(deserializedInput);
            }

            foreach (var collection in collections)
            {
                ReplaceVariablesWithValues(collection.Constants);

                foreach (Dictionary<string, string> outputInfo in collection.Outputs)
                {
                    var output = OutputFactory.CreateByType(outputInfo, collection);
                    var filePath = Path.Combine(outputBasePath, output.GetPath());
                    using (var writer = new StreamWriter(filePath))
                    {
                        Console.WriteLine("Writing file '{0}'", filePath);
                        writer.Write(output.GetContent());
                    }
                }
            }

            return ExitSuccess;
        }

        public static string BuildAutogenDisclaimer(string inputFile)
        {
            inputFile = Path.GetFileName(inputFile);
            return $"This file was auto-generated from '{inputFile}'. Changes may be overridden.";
        }

        private static void ReplaceVariablesWithValues(IDictionary<string, string> dict)
        {
            var colReplacements = dict
                        .Where(e => e.Value.StartsWith(VarPrefix) && e.Value.EndsWith(VarSuffix))
                        .Select(e => KeyValuePair.Create(
                            e.Key,
                            e.Value.Substring(VarPrefix.Length, e.Value.Length - VarPrefix.Length - VarSuffix.Length)))
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
