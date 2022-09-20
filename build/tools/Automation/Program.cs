using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Microsoft.Oryx.Automation
{
    /// <Summary>
    ///
    /// TODO:
    ///     - Replace Console.WriteLine with Logging
    ///     - Add unit tests
    ///
    /// Helps automate detecting and releasing new SDK versions for Oryx.
    /// </Summary>
    public abstract class Program
    {
        public static int Main(string[] args)
        {
            // TODO: use dotnet parameters instead and handle invalid date
            string dateTarget = args.Length > 0 ? args[0] : string.Empty;
            if (string.IsNullOrEmpty(dateTarget))
            {
                Console.WriteLine("No dateTarget provided.");
                dateTarget = DateTime.Today.ToString();
            }
            Console.WriteLine($"dateTarget: {dateTarget}");
            AddNewPlatformConstantsAsync(dateTarget).ConfigureAwait(false).GetAwaiter().GetResult();

            return 0;
        }

        /// <Summary>
        /// Adds new platform constants to Oryx repo
        /// </Summary>
        public static async Task AddNewPlatformConstantsAsync(string dateTarget)
        {
            DotNet dotNet = new DotNet();
            List<PlatformConstant> platformConstants = await dotNet.GetPlatformConstantsAsync(dateTarget).ConfigureAwait(true);
            List<Constant> yamlConstants = await DeserializeConstantsYamlAsync().ConfigureAwait(true);
            dotNet.UpdateConstants(platformConstants, yamlConstants);

            // TODO: add functionality for other platforms (python, java, golang, etc).
        }

        /// <Summary>
        /// Updates:
        ///     - Constants.ConstantsYaml
        ///
        /// Deserializes Constants.ConstantsYaml for platforms, that have new releases, can update.
        /// </Summary>
        public static async Task<List<Constant>> DeserializeConstantsYamlAsync()
        {
            string fileContents = await File.ReadAllTextAsync(Constants.ConstantsYaml).ConfigureAwait(true);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            var yamlContents = deserializer.Deserialize<List<Constant>>(fileContents);
            return yamlContents;
        }

        public static bool DatesMatch(string dateTarget, string dateReleased)
        {
            var releasedDate = DateTime.Parse(dateReleased);
            var targetDate = DateTime.Parse(dateTarget);
            int datesMatch = DateTime.Compare(releasedDate, targetDate);
            bool match = datesMatch == 0;
            Console.WriteLine($"releasedDate: {releasedDate} targetDate: {targetDate} " +
                $"datesMatch: {datesMatch} match: {match}");
            return match;
        }

        /// <Summary>
        /// Get PlatformConstants containing corresponding platform release information.
        /// Release information such as version, sha, etc.
        /// An empty list will be returned if there are no new releases.
        /// </Summary>
        /// <returns>PlatformConstants used later to update constants.yaml</returns>
        public abstract Task<List<PlatformConstant>> GetPlatformConstantsAsync(string dateTarget);

        /// <Summary>
        /// Updates:
        ///     - constants.yaml
        ///     - versionsToBuild.txt
        ///
        /// Use PlatformConstants to populate constants.yaml and versionsToBuild.txt files
        /// with relevant new platform release information. The constants.yaml file is populated
        /// so after build/generateConstants.sh is invoked, the contants.yaml is used to distribute changes
        /// across Oryx source code. Which allows tests to be automatically to be updated.
        ///
        /// <param name="platformConstants">List of PlatformConstant containing platform release information</param>
        /// <param name="yamlConstants">Deserialized Constants.ConstantsYaml which is ready for editing</param>
        /// </Summary>
        public abstract void UpdateConstants(List<PlatformConstant> platformConstants, List<Constant> yamlConstants);

        /// <Summary>
        /// Stores platform release information so it can be referenced when updating:
        /// constants.yaml and versionsToBuild.txt files
        /// </Summary>
        public class PlatformConstant
        {
            /// <Summary>
            /// The version of the platfom.
            /// Example: 1.2.3
            /// </Summary>
            public string Version { get; set; } = string.Empty;

            /// <Summary>
            /// The sha of the platform's version.
            /// Some platforms may not have a sha.
            /// </Summary>
            public string Sha { get; set; } = string.Empty;

            /// <Summary>
            /// The name of the platform.
            /// Example: dotnet, golang, etc.
            /// </Summary>
            public string PlatformName { get; set; } = string.Empty;

            /// <Summary>
            /// The type of version that is being represented.
            /// Example: sdk, aspnetcore, netcore, etc.
            /// </Summary>
            public string VersionType { get; set; } = string.Empty;

            // TODO: Add fields for other feilds.
            //  For example, python has GPG keys
        }


        /// <Summary>
        /// This is used to deserialize Constants.ConstantsYaml file
        /// </Summary>
        public class Constant
        {
            public string Name { get; set; } = string.Empty;

            public Dictionary<string, object> Constants { get; set; } = new Dictionary<string, object>();

            public List<object> Outputs { get; set; } = new List<object>();
        }
    }
}