echo
dotnetCoreVersion=$(dotnet --version)
echo "Using .NET Core SDK Version: $dotnetCoreVersion"

if [ "$SOURCE_DIR" == "$DESTINATION_DIR" ]
then
	dotnet publish "{{ ProjectFile }}" -c {{ Configuration }}
else
	echo
	echo "Publishing to directory $DESTINATION_DIR..."
	echo
	dotnet publish "{{ ProjectFile }}" -c {{ Configuration }} -o "$DESTINATION_DIR"
fi
