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

        private readonly Predicate<string> isValidTitle = title => !string.IsNullOrWhiteSpace(title);

        private List<Tuple<string, string>> _rows = new List<Tuple<string, string>>();

        public void AddDefinition([CanBeNull] string title, [CanBeNull] string value)
        {
            if (isValidTitle(title))
            {
                _rows.Add(CreateDefTuple(title, value));
            }
        }

        public void AddDefinitions([CanBeNull] IDictionary<string, string> values)
        {
            if (values != null)
            {
                _rows.AddRange(values
                    .Where(pair => isValidTitle(pair.Key))
                    .Select(pair => CreateDefTuple(pair.Key, pair.Value)));
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

                string[] lines = row.Item2.Split(Environment.NewLine);

                result.AppendLine(lines[0]);
                foreach (string line in lines.Skip(1))
                {
                    result.Append(new string(' ', headingWidth + HeadingSuffix.Length));
                    result.AppendLine(line);
                }
            }

            return result.ToString();
        }

        private Tuple<string, string> CreateDefTuple(string title, [CanBeNull] string value)
        {
            return Tuple.Create(title, value ?? string.Empty);
        }
    }
}