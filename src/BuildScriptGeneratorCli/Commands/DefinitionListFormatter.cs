// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    /// <summary>
    /// Prints an ordered list of definitions with equal padding for headings.
    /// </summary>
    internal class DefinitionListFormatter
    {
        private const string HeadingSuffix = ": ";
        private List<Tuple<string, string>> _rows = new List<Tuple<string, string>>();

        public void AddDefinition(string title, string value)
        {
            _rows.Add(new Tuple<string, string>(title, value));
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
    }
}