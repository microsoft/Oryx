// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    using System;

    //TODO: CHECK IF WE WANT TO USE https://www.nuget.org/packages/Microsoft.Extensions.CommandLineUtils/ 
    
        /// <summary>
    /// Runtime options that can be passed to the command line application.
    /// </summary>
    internal class CommandLineArgs
    {
        private const string LanguageOptionName = "language";

        public string SourceCodeFolder { get; private set; }

        public string TargetScriptPath { get; private set; }

        public string Language { get; private set; }

        internal CommandLineArgs(string[] args)
        {
            if (args.Length < 2)
            {
                ThrowUsageMessage();
            }
            int unnamedOptionIdx = 0;
            for (int i = 0; i < args.Length; i++)
            {
                var currArg = args[i];
                if (currArg.StartsWith('-'))
                {
                    var optionName = currArg.Substring(1);
                    string optionValue = null;
                    var equalSignIdx = optionName.IndexOf('=');
                    if (equalSignIdx != -1)
                    {
                        optionValue = optionName.Substring(equalSignIdx + 1);
                    }
                    else if (i < args.Length - 1)
                    {
                        var candidateOption = args[i + 1];
                        if (!candidateOption.StartsWith('-'))
                        {
                            optionValue = candidateOption;
                            i++;
                        }
                    }

                    if (optionName.Equals(LanguageOptionName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Language = optionValue;
                    }
                }
                else
                {
                    switch (unnamedOptionIdx)
                    {
                        case 0:
                            SourceCodeFolder = args[i];
                            break;

                        case 1:
                            TargetScriptPath = args[i];
                            break;

                        default:
                            ThrowUsageMessage();
                            break;
                    }
                    unnamedOptionIdx++;
                }
            }
        }

        private void ThrowUsageMessage()
        {
            throw new InvalidUsageException("Usage: [options] <source folder> <target build script>");
        }
    }
}