// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Microsoft.Oryx.Common
{
    /// <summary>
    /// Prints an ordered list of definitions with equal padding for headings.
    /// </summary>
    public class DefinitionListFormatter
    {
        private const string HeadingSuffix = ": ";

        private List<Tuple<string, string>> _rows = new List<Tuple<string, string>>();

        public void AddDefinition(string title, string value)
        {
            var tuple = CreateDefTuple(title, value);
            if (tuple != null)
            {
                _rows.Add(tuple);
            }
        }

        public void AddDefinitions([CanBeNull] IDictionary<string, string> values)
        {
            if (values != null)
            {
                _rows.AddRange(values
                    .Select(pair => CreateDefTuple(pair.Key, pair.Value))
                    .Where(tuple => tuple != null));
            }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            int headingWidth = _rows.Max(t => t.Item1.Length) + 1;
            foreach (var row in _rows)
            {
                result.Append(row.Item1.PadRight(headingWidth) + HeadingSuffix);

                if (string.IsNullOrWhiteSpace(row.Item2))
                {
                    continue;
                }

                string[] lines = row.Item2.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                result.AppendLine(lines[0]);
                foreach (string line in lines.Skip(1))
                {
                    result.Append(new string(' ', headingWidth + HeadingSuffix.Length)).AppendLine(line);
                }
            }

            return result.ToString();
        }

        [CanBeNull]
        private Tuple<string, string> CreateDefTuple([CanBeNull] string title, [CanBeNull] string value)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return Tuple.Create(title, value);
        }
    }
}