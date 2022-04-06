echo
dotnetCoreVersion=$(dotnet --version)
echo "Using .NET Core SDK Version: $dotnetCoreVersion"

{{ if InstallBlazorWebAssemblyAOTWorkloadCommand | IsNotBlank }}
    echo
    echo "Running '{{ InstallBlazorWebAssemblyAOTWorkloadCommand }}'..."
    echo
    command='{{ InstallBlazorWebAssemblyAOTWorkloadCommand }}'
	eval $command
{{ end }}

{{ # .NET Core 1.1 based projects require restore to be run before publish }}
dotnet restore "{{ ProjectFile }}"

if [ "$SOURCE_DIR" == "$DESTINATION_DIR" ]
then
	dotnet publish "{{ ProjectFile }}" -c {{ Configuration }}
else
	echo
	echo "Publishing to directory $DESTINATION_DIR..."
	echo
	dotnet publish "{{ ProjectFile }}" -c {{ Configuration }} -o "$DESTINATION_DIR"

	# we copy *.csproj to destination directory so the detector can identify
	# the destination directory as a DotNet application
	# when running oryx run-script
	# 
	# 2>/dev/null || :
	# code snippet above is used to surpass cp error message & code
	# since this is needed during: oryx run-script 
	# but not during other dotnet builds
	cp ${SOURCE_DIR}/*.csproj ${DESTINATION_DIR} 2>/dev/null || :
fi
