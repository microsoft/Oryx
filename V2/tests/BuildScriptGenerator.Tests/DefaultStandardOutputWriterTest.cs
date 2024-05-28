// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class DefaultStandardOutputWriterTest
    {
        private string _output;
        private readonly Action<string> _write;
        private readonly Action<string> _writeLine;

        public DefaultStandardOutputWriterTest()
        {
            _output = string.Empty;
            _write = (message) => { _output += message; };
            _writeLine = (message) => { _output += string.Format("{0}\n", message); };
        }

        [Fact]
        public void DefaultStandardOutputWriter_EmptyConstructor_ValidateWriteMethods()
        {
            var writer = new DefaultStandardOutputWriter();
            writer.Write("Hello world!");
            writer.Write("Hello world!");
            Assert.Equal(string.Empty, _output);

            writer.WriteLine("Hello world!");
            writer.Write("Hello world!");
            Assert.Equal(string.Empty, _output);
        }

        [Fact]
        public void DefaultStandardOutputWriter_SingleParameterConstructor_ValidateWriteMethods()
        {
            var writer = new DefaultStandardOutputWriter(_write);
            writer.Write("Hello world!");
            writer.Write("Hello world!");
            Assert.Equal("Hello world!Hello world!", _output);

            _output = string.Empty;
            writer.WriteLine("Hello world!");
            writer.Write("Hello world!");
            Assert.Equal("Hello world!\nHello world!", _output);
        }

        [Fact]
        public void DefaultStandardOutputWriter_BothParametersConstructor_ValidateWriteMethods()
        {
            var writer = new DefaultStandardOutputWriter(_write, _writeLine);
            writer.Write("Hello world!");
            writer.Write("Hello world!");
            Assert.Equal("Hello world!Hello world!", _output);

            _output = string.Empty;
            writer.WriteLine("Hello world!");
            writer.Write("Hello world!");
            Assert.Equal("Hello world!\nHello world!", _output);
        }
    }
}