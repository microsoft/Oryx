tmpOutputDir="/tmp/puboutput"
zippedOutputFileName=oryx_output.tar.gz

outputDir="$DESTINATION_DIR"
if [ "$SOURCE_DIR" == "$DESTINATION_DIR" ]
then
	outputDir="{{ PublishDirectory }}"
fi

echo
dotnetCoreVersion=$(dotnet --version)
echo ".NET Core Version: $dotnetCoreVersion"

echo
echo Restoring packages...
echo
dotnet restore "{{ ProjectFile }}"

{{ if ZipAllOutput }}
	publishDir="$tmpOutputDir/publish"
	if [ -d "$publishDir" ]
	then
		rm -rf "$publishDir"
	fi

	echo
	echo "Publishing to directory '$publishDir'..."
	echo
	dotnet publish "{{ ProjectFile }}" -c Release -o "$publishDir"

	mkdir -p "$tmpOutputDir"
	if [ -f "$tmpOutputDir/$zippedOutputFileName" ]; then
		echo
		echo "File '$tmpOutputDir/$zippedOutputFileName' already exists under '$SOURCE_DIR'. Deleting it..."
		rm -f "$tmpOutputDir/$zippedOutputFileName"
	fi

	# Zip only the contents and not the parent directory
	mkdir -p "$outputDir"
	cd "$publishDir"
	tar -zcf ../$zippedOutputFileName .
	cd ..
	cp -f "$zippedOutputFileName" "$outputDir/$zippedOutputFileName"
{{ else }}
	if [ "$(ls -A $outputDir)" ]
	then
		echo
		echo "Destination directory '$outputDir' is not empty. Deleting its contents..."
		rm -rf "$outputDir"/*
	fi

	echo
	echo "Publishing to directory '$outputDir'..."
	echo
	dotnet publish "{{ ProjectFile }}" -c Release -o "$outputDir"
{{ end }}
