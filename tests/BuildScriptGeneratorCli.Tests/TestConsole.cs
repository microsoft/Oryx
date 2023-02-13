// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;
using System.Text;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Tests
{
    internal class TestConsole : IConsole
    {
        public TestConsole()
            : this(newLineCharacter: null)
        {
        }

        public TestConsole(string newLineCharacter)
        {
            this.Out = new StandardStreamWriter(newLineCharacter);
            this.Error = new StandardStreamWriter(newLineCharacter);
        }

        public bool IsInputRedirected { get; protected set; }

        public bool IsOutputRedirected { get; protected set; }

        public bool IsErrorRedirected { get; protected set; }

        public IStandardStreamWriter Out { get; protected set; }

        // Legacy Property
        public string StdOutput => Out.ToString();

        public IStandardStreamWriter Error { get; protected set; }

        // Legacy Property
        public string StdError => Error.ToString();

        // From System.CommandLine.IO TestConsole
        // https://github.com/dotnet/command-line-api/blob/76437b04511d88543df5cde2c7910e8d40e30888/src/System.CommandLine/IO/TestConsole.cs
        internal class StandardStreamWriter : TextWriter, IStandardStreamWriter
        {
            private readonly StringBuilder _stringBuilder = new StringBuilder();

            public StandardStreamWriter()
                : this(newLineCharacter: null) { }

            public StandardStreamWriter(string newLineCharacter)
            {
                this.NewLine = newLineCharacter;
            }

            public override void Write(char value)
            {
                _stringBuilder.Append(value);
            }

            public override void Write(string value)
            {
                _stringBuilder.Append(value);
            }

            public override Encoding Encoding { get; } = Encoding.Unicode;

            public override string ToString() => _stringBuilder.ToString();
        }
    }
}
