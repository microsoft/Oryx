#!/usr/bin/env -S -i HOME=${HOME} PATH=${PATH} bash

RESULT1=`pgrep dotnet-monitor`

if [ "${RESULT1:-null}" = null ]; then
    if [ -f "/dotnet_monitor_config.json" ]; then
        if [ ! -f "/etc/dotnet-monitor/settings.json" ]; then
            mkdir -p /etc/dotnet-monitor
            cp /dotnet_monitor_config.json /etc/dotnet-monitor/settings.json
        fi
    fi
    echo "dotnet-monitor not running, starting again..."
    /opt/dotnetcore-tools/dotnet-monitor collect --urls "http://0.0.0.0:50051" --metrics true --metricUrls "http://0.0.0.0:50050" --no-auth > /dev/null 2>&1 &
fi
