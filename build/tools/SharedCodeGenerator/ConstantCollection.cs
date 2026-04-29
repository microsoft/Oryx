// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Oryx.SharedCodeGenerator
{
    internal class ConstantCollection
    {
        public const string NameSeparator = "-";

        public string SourcePath { get; set; }

        public string Name { get; set; }

        public Dictionary<string, object> Constants { get; set; }

        public List<Dictionary<string, string>> Outputs { get; set; }

        public Dictionary<string, string> StringConstants
        {
            get
            {
                return this.Constants?
                    .Where(pair => pair.Value is string)
                    .ToDictionary(pair => pair.Key, pair => pair.Value as string);
            }
        }

        public Dictionary<string, IList<object>> ListConstants
        {
            get
            {
                return this.Constants?
                    .Where(pair => pair.Value is IList<object>)
                    .ToDictionary(pair => pair.Key, pair => pair.Value as IList<object>);
            }
        }

        public Dictionary<string, IDictionary<object, object>> DictionaryConstants
        {
            get
            {
                return this.Constants?
                    .Where(pair => pair.Value is IDictionary<object, object>)
                    .ToDictionary(pair => pair.Key, pair => pair.Value as IDictionary<object, object>);
            }
        }
    }
}
