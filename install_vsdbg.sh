#!/usr/bin/env bash

if [ "$APPSVC_REMOTE_DEBUGGING" != true ]; then
    return
fi

# check for empty
if [ -z "$APPSVC_REMOTE_DEBUGGING_VERSION" ]; then
    echo "APPSVC_REMOTE_DEBUGGING_VERSION is empty." >/home/LogFiles/vsdbg_validation_error.log
    return
fi

# Trim leading and trailing whitespace from APPSVC_REMOTE_DEBUGGING_VERSION
APPSVC_REMOTE_DEBUGGING_VERSION="${APPSVC_REMOTE_DEBUGGING_VERSION#"${APPSVC_REMOTE_DEBUGGING_VERSION%%[![:space:]]*}"}"
APPSVC_REMOTE_DEBUGGING_VERSION="${APPSVC_REMOTE_DEBUGGING_VERSION%"${APPSVC_REMOTE_DEBUGGING_VERSION##*[![:space:]]}"}"

# Remove any extra value
sanitized_version="${APPSVC_REMOTE_DEBUGGING_VERSION//[^vs0-9u]/}"

if [[ ! $sanitized_version =~ ^vs[0-9]{4}(u[0-9]+)?$ ]]; then
    echo "Sanitized APPSVC_REMOTE_DEBUGGING_VERSION '$sanitized_version' does not match the expected pattern." >/home/LogFiles/vsdbg_validation_error.log
    return
fi

# Install vsdbg. Adding log redirection if vsdbg installation fails.
# Keeping the output and error log in the same file because error logs are also coming as output only. Keeping error redirection in case we get an error in any scenarios.
if curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v $sanitized_version -l /opt/vsdbg >/home/LogFiles/vsdbginstallation.log 2>&1; then
    echo "vsdbg installation succeeded for Sanitized APPSVC_REMOTE_DEBUGGING_VERSION '$sanitized_version'."
else
    echo "vsdbg installation failed for Sanitized APPSVC_REMOTE_DEBUGGING_VERSION '$sanitized_version'." >>/home/LogFiles/vsdbginstallation.log
fi
