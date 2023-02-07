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
        private readonly StringBuilder _stdOutStringBuilder;
        private readonly StringWriter _stdOutStringWriter;
        private readonly StringBuilder _stdErrStringBuilder;
        private readonly StringWriter _stdErrStringWriter;
        private string _stdOutput;
        private string _stdError;
        private bool _stdOutputCalled;
        private bool _stdErrorCalled;

        public TestConsole()
            : this(newLineCharacter: null)
        {
        }

        public TestConsole(string newLineCharacter)
        {
            _stdOutStringBuilder = new StringBuilder();
            _stdOutStringWriter = new StringWriter(_stdOutStringBuilder);
            _stdOutStringWriter.NewLine = newLineCharacter;
            _stdErrStringBuilder = new StringBuilder();
            _stdErrStringWriter = new StringWriter(_stdErrStringBuilder);
            _stdErrStringWriter.NewLine = newLineCharacter;
        }

        public string StdOutput
        {
            get
            {
                if (!_stdOutputCalled)
                {
                    _stdOutStringWriter.Flush();
                    _stdOutput = _stdOutStringBuilder.ToString();
                    _stdOutputCalled = true;
                }
                return _stdOutput;
            }
        }

        public string StdError
        {
            get
            {
                if (!_stdErrorCalled)
                {
                    _stdErrStringWriter.Flush();
                    _stdError = _stdErrStringBuilder.ToString();
                    _stdErrorCalled = true;
                }
                return _stdError;
            }
        }

        public TextWriter Out => _stdOutStringWriter;

        public TextWriter Error => _stdErrStringWriter;

        public TextReader In => throw new NotImplementedException();

        public bool IsInputRedirected => throw new NotImplementedException();

        public bool IsOutputRedirected => true;

        public bool IsErrorRedirected => true;

        public ConsoleColor ForegroundColor
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public ConsoleColor BackgroundColor
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        IStandardStreamWriter IStandardOut.Out => throw new NotImplementedException();

        IStandardStreamWriter IStandardError.Error => throw new NotImplementedException();

#pragma warning disable 0067
        public event ConsoleCancelEventHandler CancelKeyPress;
#pragma warning restore 0067

        public void ResetColor()
        {
            throw new NotImplementedException();
        }
    }
}
