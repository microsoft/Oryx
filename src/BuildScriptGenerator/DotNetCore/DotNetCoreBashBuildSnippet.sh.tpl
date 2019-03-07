if [ "$SOURCE_DIR" == "$DESTINATION_DIR" ]
then
	outputDir="{{ PublishDirectory }}"
	if [ -d "$outputDir" ]
	then
		echo "Destination directory '$outputDir' already exists. Deleting it ..."
		rm -rf "$outputDir"
	fi
else
	outputDir="$DESTINATION_DIR"
fi

echo
dotnetCoreVersion=$(dotnet --version)
echo ".NET Core Version: $dotnetCoreVersion"

echo
echo Restoring packages ...
echo
dotnet restore "{{ ProjectFile }}"

echo
echo "Publishing to directory $outputDir ..."
echo
dotnet publish "{{ ProjectFile }}" -c Release -o "$outputDir"
