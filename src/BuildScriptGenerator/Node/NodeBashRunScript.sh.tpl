#!/bin/sh

{{ if ToolsVersions | IsNotBlank }}
if [ -f /usr/local/bin/benv ]; then
	source /usr/local/bin/benv {{ ToolsVersions }}
fi
{{ end }}

# Enter the source directory to make sure the script runs where the user expects
cd {{ AppDirectory }}

{{ StartupCommand }}