echo "THIS IS A NEW COMMAND WILL THIS SHOW UP???"

echo
dotnetCoreVersion=$(dotnet --version)
echo "Using .NET Core SDK Version: $dotnetCoreVersion"

{{ # .NET Core 1.1 based projects require restore to be run before publish }}
dotnet restore "{{ ProjectFile }}"

if [ ! -z "$RUN_BUILD_COMMAND" ]
then
	echo 'Running $RUN_BUILD_COMMAND'
	${RUN_BUILD_COMMAND}
else
	if [ "$SOURCE_DIR" == "$DESTINATION_DIR" ]
	then
		dotnet publish "{{ ProjectFile }}" -c {{ Configuration }}
	else
		echo
		echo "Publishing to directory $DESTINATION_DIR..."
		echo
		dotnet publish "{{ ProjectFile }}" -c {{ Configuration }} -o "$DESTINATION_DIR"
	fi
fi