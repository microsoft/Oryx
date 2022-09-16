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
            AddNewPlatformConstantsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            Console.WriteLine($"args.Length: {args.Length}");

            return 0;
        }

        /// <Summary>
        /// TODO: write summary.
        /// </Summary>
        public static async Task AddNewPlatformConstantsAsync()
        {
            DotNet dotNet = new DotNet();
            List<PlatformConstant> platformConstants = await dotNet.GetPlatformConstantsAsync().ConfigureAwait(true);
            await dotNet.UpdateConstantsAsync(platformConstants);

            // TODO: add functionality for other platforms (python, java, golang, etc).
        }

        /// <Summary>
        /// TODO: write summary.
        /// </Summary>
        public abstract Task<List<PlatformConstant>> GetPlatformConstantsAsync();
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
            /// Gets or sets initializes a new instance of the <see cref="PlatformConstant"/> class.
            /// </summary>

            /// <Summary>
            /// TODO: write summary.
            /// </Summary>
            public string Version { get; set; } = string.Empty;

            /// <Summary>
            /// TODO: write summary.
            /// </Summary>
            public string Sha { get; set; } = string.Empty;

            /// <Summary>
            /// TODO: write summary.
            /// </Summary>
            public string PlatformName { get; set; } = string.Empty;

            /// <Summary>
            /// TODO: write summary.
            /// </Summary>
            public string VersionType { get; set; } = string.Empty;

            // TODO: Add fields for other feilds.
            //  For example, python has GPG keys
        }
    }
}