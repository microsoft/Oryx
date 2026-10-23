// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.RuntimeImage.Tests
{
    public class PhpApacheImageTest : TestBase
    {
        public PhpApacheImageTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        [Trait("category", "runtime-bullseye")]
        [Trait(TestConstants.Category, TestConstants.Release)]
        public void Php82ApacheServesPhpWithRewriteAndExtensions()
        {
            VerifyApache("8.2", PhpVersions.Php82Version, ImageTestHelperConstants.OsTypeDebianBullseye);
        }

        [Fact]
        [Trait("category", "runtime-bookworm")]
        [Trait(TestConstants.Category, TestConstants.Release)]
        public void Php83ApacheServesPhpWithRewriteAndExtensions()
        {
            VerifyApache("8.3", PhpVersions.Php83Version, ImageTestHelperConstants.OsTypeDebianBookworm);
        }

        private void VerifyApache(string version, string expectedVersion, string osType)
        {
            const string script = @"set -euo pipefail
php -r 'foreach (array(""gd"", ""mysqli"", ""Zend OPcache"", ""pdo_mysql"", ""pdo_pgsql"", ""pgsql"", ""ldap"", ""intl"", ""gmp"", ""zip"", ""bcmath"", ""mbstring"", ""pcntl"", ""calendar"", ""exif"", ""gettext"", ""imap"", ""redis"", ""mongodb"", ""tidy"", ""shmop"", ""soap"", ""sockets"", ""sysvmsg"", ""sysvsem"", ""sysvshm"", ""pdo_odbc"", ""odbc"", ""xsl"", ""sodium"", ""sqlsrv"", ""pdo_sqlsrv"") as $ext) { if (!extension_loaded($ext)) { fwrite(STDERR, ""Missing extension: "" . $ext); exit(1); } }'
mkdir -p /tmp/apache-app/.git
echo 'not public' > /tmp/apache-app/.git/config
cat > /tmp/apache-app/index.php <<'PHP'
<?php echo PHP_SAPI, '|', PHP_VERSION, '|', $_GET['rewritten'] ?? 'no-rewrite';
PHP
printf 'RewriteEngine On\nRewriteRule ^health$ index.php?rewritten=yes [L]\n' > /tmp/apache-app/.htaccess
oryx create-script -appPath /tmp/apache-app -bindPort 8091 -output /tmp/apache-startup.sh
bash /tmp/apache-startup.sh &
server=$!
trap 'apache2ctl -k graceful-stop; wait $server || test $? -eq 143' EXIT
for attempt in {1..30}; do
    if curl -fsS http://127.0.0.1:8091/health > /tmp/apache-response; then
        break
    fi
    kill -0 $server
    sleep 1
done
cat /tmp/apache-response
test ""$(curl -sS -o /dev/null -w '%{http_code}' http://127.0.0.1:8091/.git/config)"" = 403
";
            var result = _dockerCli.Run(
                _imageHelper.GetRuntimeImage("php", version, osType),
                "/bin/bash",
                "-c",
                script.Replace("\r\n", "\n"));

            RunAsserts(() =>
            {
                Assert.True(result.IsSuccess);
                Assert.Contains("apache2handler|" + expectedVersion + "|yes", result.StdOut);
            }, result.GetDebugInfo());
        }
    }
}
