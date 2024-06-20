#!/usr/bin/env bash
cat >/etc/motd <<EOL
   _____                               
  /  _  \ __________ _________   ____  
 /  /_\  \\\___   /  |  \_  __ \_/ __ \ 
/    |    \/    /|  |  /|  | \/\  ___/ 
\____|__  /_____ \____/ |__|    \___  >
        \/      \/                  \/ 
A P P   S E R V I C E   O N   L I N U X

Documentation: http://aka.ms/webapp-linux
Dotnet quickstart: https://aka.ms/dotnet-qs
ASP .NETCore Version: `ls -X /usr/share/dotnet/shared/Microsoft.NETCore.App | tail -n 1`
Note: Any data outside '/home' is not persisted
EOL
cat /etc/motd

# Get environment variables to show up in SSH session
# This will replace any \ (backslash), " (double quote), $ (dollar sign) and ` (back quote) symbol by its escaped character to not allow any bash substitution.
(printenv | sed -n "s/^\([^=]\+\)=\(.*\)$/export \1=\2/p" | sed 's/\\/\\\\/g' | sed 's/"/\\\"/g' | sed 's/\$/\\\$/g' | sed 's/`/\\`/g' | sed '/=/s//="/' | sed 's/$/"/' >> /etc/profile)

# starting sshd process
source /opt/startup/startssh.sh

# Install ca-certificates
source /opt/startup/install_ca_certs.sh

containerName=`hostname`

if [ "$WEBSITE_ENABLE_AUTO_CRASH_DUMP" = true ]; then
    # Enable automatic creation of dumps when a process crashes
    export COMPlus_DbgEnableMiniDump="1"
    # Create a base directory for dumps under /home/LogFiles so that the
    # dumps are accessible from the build container too (since a runtime container might have already crashed)
    export DUMP_DIR="/home/LogFiles/crash_dumps"
    mkdir -p "$DUMP_DIR"
    chmod 777 "$DUMP_DIR"
    # Format : coredump.hostname.processid.time
    # Example: coredump.7d77b4ff1fea.15.1571222166
    export COMPlus_DbgMiniDumpName="$DUMP_DIR/coredump.$containerName.%d.$(date +%s)"
fi

appPath="/home/site/wwwroot"
runFromPath="/tmp/webapp"
startupCommandPath="/opt/startup/startup.sh"
defaultAppPath="/defaulthome/hostingstart/hostingstart.dll"
userStartupCommand="$@"

# When run from copy is enabled, Oryx tries to run the app from a different directory (local to the container),
# so sanitize any input arguments which still reference the wwwroot path. This is true for VS Publish scenarios.
# Even though VS Publish team might fix this on their end, end users might not have upgraded their extension, so
# this code needs to be present.
if [ "$APP_SVC_RUN_FROM_COPY" = true ]; then
    # Trim the ending '/'
    appPath=$(echo "${appPath%/}")
    runFromPath=$(echo "${runFromPath%/}")
    userStartupCommand=$(echo $userStartupCommand | sed "s!$appPath!$runFromPath!g")
    runFromPathArg="-runFromPath $runFromPath"
fi

echo '' > /etc/cron.d/diag-cron
if [ "$WEBSITE_USE_DIAGNOSTIC_SERVER" != false ]; then
    /run-diag.sh > /dev/null
    echo '*/5 * * * * bash -l -c "/run-diag.sh > /dev/null"' >> /etc/cron.d/diag-cron
fi

if [ "$USE_DOTNET_MONITOR" = true ]; then
    /run-dotnet-monitor.sh > /dev/null
    echo '*/5 * * * * bash -l -c "/run-dotnet-monitor.sh > /dev/null"' >> /etc/cron.d/diag-cron
fi

if [[ "$WEBSITE_USE_DIAGNOSTIC_SERVER" != false || "$USE_DIAG_SERVER" = true ]]; then
    chmod 0644 /etc/cron.d/diag-cron
    crontab /etc/cron.d/diag-cron
    /etc/init.d/cron start
    source install_vsdbg.sh
fi

# Dockerfile copies generate_and_execute_startup_script.sh from /templates/startup_scripts directory based on assetDirs in tuxib.yml
source /bin/generate_and_execute_startup_script.sh
