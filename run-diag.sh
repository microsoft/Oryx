#!/usr/bin/env bash

if [ -f "/diagServer/DiagServer" ]; then
    RESULT2=`pgrep DiagServer`
    if [ "${RESULT2:-null}" = null ]; then
        echo "DiagServer not running, starting again..."
        (cd /diagServer && ./DiagServer > /dev/null 2>&1) &
    fi
fi
