using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class DefaultToolDetector
    {
        private readonly IEnumerable<ILanguageDetector> _detectors;
        private readonly ILogger<DefaultToolDetector> _logger;

        public DefaultToolDetector(
            IEnumerable<ILanguageDetector> detectors,
            ILogger<DefaultToolDetector> logger)
        {
            _detectors = detectors;
            _logger = logger;
        }

        public IList<KeyValuePair<string, string>> DetectTools(ISourceRepo sourceRepo)
        {
            var result = new List<KeyValuePair<string, string>>();

            foreach (var detector in _detectors)
            {
                var detectionResult = detector.Detect(new BuildScriptGeneratorContext()
                {
                    SourceRepo = sourceRepo
                });

                if (detectionResult != null)
                {
                    result.Add(new KeyValuePair<string, string>(detectionResult.Language, detectionResult.LanguageVersion));
                }
            }

            return result;
        }
    }
}
