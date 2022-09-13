namespace Microsoft.Oryx.Automation
{
    /// <Summary>
    /// TODO: write summary.
    /// </Summary>
    public abstract class Program
    {
        /// <Summary>
        /// TODO: write summary.
        /// </Summary>
        public static int Main(string[] args)
        {
            AddNewSdkAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            Console.WriteLine($"args.Length: {args.Length}");

            return 0;
        }

        /// <Summary>
        /// TODO: write summary.
        /// </Summary>
        public static async Task AddNewSdkAsync()
        {
            DotNet dotNet = new DotNet();
            List<PlatformConstant> platformConstants = await dotNet.GetVersionShaAsync().ConfigureAwait(true);
            await dotNet.UpdateConstantsAsync(platformConstants);
        }

        /// <Summary>
        /// TODO: write summary.
        /// </Summary>
        public abstract Task<List<PlatformConstant>> GetVersionShaAsync();
        //public abstract Task<Dictionary<string, string>> GetVersionShaAsync();

        /// <Summary>
        /// TODO: write summary.
        /// </Summary>
        public abstract Task UpdateConstantsAsync(List<PlatformConstant> platformConstants);
        //public abstract Task UpdateConstantsAsync(Dictionary<string, string> versionShas);

        /// <Summary>
        /// TODO: write summary.
        /// </Summary>
        public class PlatformConstant
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PlatformConstant"/> class.
            /// </summary>
            /// <param name="version">Version of the platform.</param>
            /// <param name="sha">Sha of the platform.</param>
            /// <param name="platformName">Name of the platform.</param>
            /// <param name="versionType">Version type of the platform.</param>
            public PlatformConstant(string version, string sha, string platformName, string versionType)
            {
                this.Version = version;
                this.Sha = sha;
                this.PlatformName = platformName;
                this.VersionType = versionType;
            }

            /// <Summary>
            /// TODO: write summary.
            /// </Summary>
            public string Version { get; set; }

            /// <Summary>
            /// TODO: write summary.
            /// </Summary>
            public string Sha { get; set; }

            /// <Summary>
            /// TODO: write summary.
            /// </Summary>
            public string PlatformName { get; set; }

            /// <Summary>
            /// TODO: write summary.
            /// </Summary>
            public string VersionType { get; set; }
        }
    }
}