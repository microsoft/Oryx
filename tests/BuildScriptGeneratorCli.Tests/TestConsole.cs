// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Text;
using McMaster.Extensions.CommandLineUtils;

namespace BuildScriptGeneratorCli.Tests
{
    internal class TestConsole : IConsole
    {
        private readonly StringBuilder _stringBuilder;
        private readonly StringWriter _stringWriter;
        private string _output;
        private bool outputCalled;

        public TestConsole()
        {
            _stringBuilder = new StringBuilder();
            _stringWriter = new StringWriter(_stringBuilder);
        }

        public string Output
        {
            get
            {
                if (!outputCalled)
                {
                    _stringWriter.Flush();
                    _output = _stringBuilder.ToString();
                    outputCalled = true;
                }
                return _output;
            }
        }

        public TextWriter Out => _stringWriter;

        public TextWriter Error => _stringWriter;

        public TextReader In => throw new NotImplementedException();

        public bool IsInputRedirected => throw new NotImplementedException();

        public bool IsOutputRedirected => throw new NotImplementedException();

        public bool IsErrorRedirected => throw new NotImplementedException();

        public ConsoleColor ForegroundColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ConsoleColor BackgroundColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

#pragma warning disable 0067
        public event ConsoleCancelEventHandler CancelKeyPress;
#pragma warning restore 0067

        public void ResetColor()
        {
            throw new NotImplementedException();
        }
    }
}
