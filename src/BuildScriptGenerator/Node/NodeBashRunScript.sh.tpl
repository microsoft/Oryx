#!/bin/bash

{{ if ToolsVersions | IsNotBlank }}
if [ -f /usr/local/bin/benv ]; then
	source /usr/local/bin/benv {{ ToolsVersions }}
fi
{{ end }}

# Enter the source directory to make sure the script runs where the user expects
cd {{ AppDirectory }}

export PORT={{ BindPort }}

# Add the app directory to the path in case the startup command is a user-provided script
# and is located at the root.
PATH=$PATH:{{ AppDirectory }}

{{ StartupCommand }}