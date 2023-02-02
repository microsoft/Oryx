using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace Microsoft.Oryx.BuildScriptGenerator.Common.Extensions
{
    public interface ITelemetryClientExtension
    {
        TelemetryClient GetTelemetryClient();
    }

    public class TelemetryClientExtension : ITelemetryClientExtension
    {
        private string connectionString;

        public TelemetryClientExtension(string connectionString)
        {
            this.connectionString = connectionString;
        }

        TelemetryClient ITelemetryClientExtension.GetTelemetryClient()
        {
            var config = new TelemetryConfiguration()
            {
                ConnectionString = this.connectionString,
            };
            return new TelemetryClient(config);
        }
    }
}