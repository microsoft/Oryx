echo "Running the command '{{ Command }}'"
echo
{{ if Command | IsNotBlank }}
$ORYX_PROGRESS_PHASE_COMMAND "build.execute"
{{ end }}
{{ Command }}
