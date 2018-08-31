// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator
{
    using System;
    using System.Collections.Generic;
    using BuildScriptGenerator.SourceRepo;
    using Microsoft.Oryx.BuildScriptGenerator.Node;

    /// <summary>
    /// Detects the language used in a project.
    /// </summary>
    public class LanguageDetector
    {
        private IEnumerable<ILanguage> _supportedLanguages;
        private Lazy<Dictionary<string, ILanguage>> _languagesByName;

        public LanguageDetector(IEnumerable<ILanguage> languages)
        {
            _supportedLanguages = languages;
            _languagesByName = new Lazy<Dictionary<string, ILanguage>>(BuildNodeToLanguageMap);
        }

        public LanguageDetector()
        {
            // TODO - add dependency injection
            _supportedLanguages = new ILanguage[] { new NodeLanguage() };
            _languagesByName = new Lazy<Dictionary<string, ILanguage>>(BuildNodeToLanguageMap);
        }

        /// <summary>
        /// Tries to detect the language used in a particular source code folder,
        /// and returns a build script builder for that language.
        /// </summary>
        /// <param name="sourceRepo">
        /// The source code repo.
        /// </param>
        /// <returns>
        /// The build script builder for that project if it the language could be detected;
        /// null otherwise.
        /// </returns>
        public IBuildScriptBuilder GetBuildScriptBuilder(ISourceRepo sourceRepo)
        {
            IBuildScriptBuilder buildScriptBuilder = null;
            foreach (var l in _supportedLanguages)
            {
                if (l.TryGetBuildScriptBuilder(sourceRepo, out buildScriptBuilder))
                {
                    break;
                }
            }
            return buildScriptBuilder;
        }

        /// <summary>
        /// Gets a build script builder for a source folder targeting a particular programming
        /// language. If the provided language name is not compatible with the source repo,
        /// the method returns null.
        /// </summary>
        /// <param name="languageName">The name of the language being used.</param>
        /// <param name="sourceRepo">The source code repo.</param>
        /// <returns>The build script builder for the repo if it matches the provided language;
        /// null otherwise.</returns>
        public IBuildScriptBuilder GetBuildScriptBuilder(string languageName, ISourceRepo sourceRepo)
        {
            IBuildScriptBuilder buildScriptBuilder = null;
            ILanguage language;
            if (_languagesByName.Value.TryGetValue(languageName, out language))
            {
                language.TryGetBuildScriptBuilder(sourceRepo, out buildScriptBuilder);
            }
            return buildScriptBuilder;
        }

        private Dictionary<string, ILanguage> BuildNodeToLanguageMap()
        {
            var ret = new Dictionary<string, ILanguage>();
            foreach (var l in _supportedLanguages)
            {
                foreach (var name in l.Name)
                {
                    ret[name] = l;
                }
            }
            return ret;
        }
    }
}