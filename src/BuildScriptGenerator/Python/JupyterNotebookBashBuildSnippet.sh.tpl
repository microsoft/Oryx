source $CONDA_SCRIPT

echo "Conda info:"
conda info

{{ if EnvironmentYmlFile | IsNotBlank }}
envFile="{{ EnvironmentYmlFile }}"
{{ else }}
envFile="/opt/oryx/conda/oryx.environment.yml"
envFileTemplate="/opt/oryx/conda/{{ EnvironmentTemplateFileName }}"
sed 's/PYTHON_VERSION/{{ EnvironmentTemplatePythonVersion }}/g' "$envFileTemplate" > $envFile
{{ end }}

echo "{{ NoteBookBuildCommandsFileName }}"

{{ if NoteBookBuildCommandsFileName | IsNotBlank }}
COMMAND_MANIFEST_FILE="{{ NoteBookBuildCommandsFileName }}"
{{ end }}

echo "Creating directory for command manifest file if it does not exist"
mkdir -p "$(dirname "$COMMAND_MANIFEST_FILE")"
echo "Removing existing manifest file"
rm -f "$COMMAND_MANIFEST_FILE"

echo "PlatformWithVersion=Python {{ EnvironmentTemplatePythonVersion }}" >> "$COMMAND_MANIFEST_FILE"

environmentPrefix="./venv"
echo
echo "Setting up Conda virtual environemnt..."
echo
START_TIME=$SECONDS
CondaEnvCreateCommand="conda env create --file $envFile --prefix $environmentPrefix --quiet"
echo "BuildCommands=$CondaEnvCreateCommand" >> "$COMMAND_MANIFEST_FILE"
conda env create --file $envFile --prefix $environmentPrefix --quiet
ELAPSED_TIME=$(($SECONDS - $START_TIME))
echo "Done in $ELAPSED_TIME sec(s)."

{{ if HasRequirementsTxtFile }}
	echo
	echo "Activating environemnt..."
	CondaActivateCommand= "conda activate $environmentPrefix"
	printf %s ", $CondaActivateCommand" >> "$COMMAND_MANIFEST_FILE"
	conda activate $environmentPrefix

	echo
	echo "Running pip install..."
	echo
	PipInstallCommand="pip install --no-cache-dir -r requirements.txt"
	printf %s ", $PipInstallCommand" >> "$COMMAND_MANIFEST_FILE"
	pip install --no-cache-dir -r requirements.txt
{{ end }}


ReadImageType=$(cat /opt/oryx/.imagetype)

if [ "$ReadImageType" = "vso-focal" ] || [ "$ReadImageType" = "vso-debian-bullseye" ]
then
	echo $ReadImageType
else
	rm "$COMMAND_MANIFEST_FILE"
fi 
