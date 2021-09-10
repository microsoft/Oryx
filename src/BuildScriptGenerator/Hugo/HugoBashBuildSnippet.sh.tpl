echo
echo "Using Hugo version:"
$hugo version
echo

# TODO: figure out how CustomRunBuildCommand is being used here
{{ if CustomRunBuildCommand | IsNotBlank }}
	echo
	echo Running "{{ CustomRunBuildCommand }}"
	{{ CustomRunBuildCommand }}
	echo
{{ end }}


cd "$SOURCE_DIR"

$hugo