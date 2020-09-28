echo
echo "Using Node version:"
node --version
echo

echo
echo "Using Npm version:"
npm --version
echo

{{ if HasLernaJsonFile }}
	echo
	echo "Running npm install --global lerna..."
	echo
	{{NpmInstallLernaCommand}}
	echo
	echo "Running lerna init..."
	echo
	{{LernaInitCommand}}
	echo
	echo "Running lerna clean..."
	echo
	{{LernaCleanCommand}}
	echo
	echo "Running lerna list..."
	echo
	{{LernaListCommand}}
	echo
	echo "Running lerna run build..."
	echo
	{{LernaRunBuildCommand}}
{{ end }}

{{ if HasLageConfigJSFile }}
	echo
	echo "Running ..."
	echo
{{ end }}