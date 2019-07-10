#!/bin/bash
set -e

function publishToDirectory()
{
	local directoryToPublishTo="$1"
	echo
	echo "Publishing '{{ ProjectFile }}' to directory '$directoryToPublishTo'..."
	echo
	dotnet publish "{{ ProjectFile }}" -c {{ Configuration }} -o "$directoryToPublishTo"
}

TOTAL_EXECUTION_START_TIME=$SECONDS
SOURCE_DIR=$1
DESTINATION_DIR=$2
INTERMEDIATE_DIR=$3

if [ ! -d "$SOURCE_DIR" ]; then
    echo "Source directory '$SOURCE_DIR' does not exist." 1>&2
    exit 1
fi

# Get full file paths to source and destination directories
cd $SOURCE_DIR
SOURCE_DIR=$(pwd -P)

if [ -z "$DESTINATION_DIR" ]
then
    DESTINATION_DIR="$SOURCE_DIR"
fi

if [ -d "$DESTINATION_DIR" ]
then
    cd $DESTINATION_DIR
    DESTINATION_DIR=$(pwd -P)
fi

if [ ! -z "$INTERMEDIATE_DIR" ]
then
	echo "Using intermediate directory '$INTERMEDIATE_DIR'."
	if [ ! -d "$INTERMEDIATE_DIR" ]
	then
		echo
		echo "Intermediate directory doesn't exist, creating it..."
		mkdir -p "$INTERMEDIATE_DIR"		
	fi

	cd "$INTERMEDIATE_DIR"
	INTERMEDIATE_DIR=$(pwd -P)
	cd "$SOURCE_DIR"
	echo
	echo "Copying files to the intermediate directory..."
	START_TIME=$SECONDS
	excludedDirectories=""
	{{ for excludedDir in DirectoriesToExcludeFromCopyToIntermediateDir }}
	excludedDirectories+=" --exclude {{ excludedDir }}"
	{{ end }}
	rsync --delete -rt $excludedDirectories . "$INTERMEDIATE_DIR"
	ELAPSED_TIME=$(($SECONDS - $START_TIME))
	echo "Done in $ELAPSED_TIME sec(s)."
	SOURCE_DIR="$INTERMEDIATE_DIR"
fi

if [ "$SOURCE_DIR" == "$DESTINATION_DIR" ]
then
	DESTINATION_DIR="{{ PublishDirectory }}"
fi

echo
echo "Source directory     : $SOURCE_DIR"
echo "Destination directory: $DESTINATION_DIR"
echo

tmpDestinationDir="/tmp/puboutput"
tmpDestinationPublishDir="$tmpDestinationDir/publish"
zippedOutputFileName={{ ZippedOutputFileName }}
{{ if ZipAllOutput }}
ORIGINAL_DESTINATION_DIR="$DESTINATION_DIR"
DESTINATION_DIR="$tmpDestinationPublishDir"
{{ end }}

# Export the variables so that pre and post build scripts can access them
export SOURCE_DIR
export DESTINATION_DIR

{{ if BenvArgs | IsNotBlank }}
if [ -f /usr/local/bin/benv ]; then
	source /usr/local/bin/benv {{ BenvArgs }}
fi
{{ end }}

# Make sure to create the destination dir so that pre-build script has access to it
mkdir -p "$DESTINATION_DIR"

{{ if PreBuildCommand | IsNotBlank }}
echo "{{ PreBuildCommandPrologue }}"
# Make sure to cd to the source directory so that the pre-build script runs from there
cd "$SOURCE_DIR"
{{ PreBuildCommand }}
echo "{{ PreBuildCommandEpilogue }}"
{{ end }}

echo
dotnetCoreVersion=$(dotnet --version)
echo "Using .NET Core SDK Version: $dotnetCoreVersion"

cd "$SOURCE_DIR"

echo
echo "Restoring packages for '{{ ProjectFile }}'..."
echo
dotnet restore "{{ ProjectFile }}"

{{ if ZipAllOutput }}
	publishToDirectory "$tmpDestinationPublishDir"

	{{ if PostBuildCommand | IsNotBlank }}
	echo
	echo "{{ PostBuildCommandPrologue }}"
	# Make sure to cd to the source directory so that the post-build script runs from there
	cd "$SOURCE_DIR"
	{{ PostBuildCommand }}
	echo "{{ PostBuildCommandEpilogue }}"
	{{ end }}

	# Zip only the contents and not the parent directory
	echo
	echo "Compressing the contents of the output directory..."
	mkdir -p "$ORIGINAL_DESTINATION_DIR"
	cd "$tmpDestinationPublishDir"
	tar -zcf ../$zippedOutputFileName .
	cd "$tmpDestinationDir"

	cp -f "$zippedOutputFileName" "$ORIGINAL_DESTINATION_DIR/$zippedOutputFileName"
	DESTINATION_DIR="$ORIGINAL_DESTINATION_DIR"
{{ else }}
	publishToDirectory "$DESTINATION_DIR"

	{{ if PostBuildCommand | IsNotBlank }}
	echo
	echo "{{ PostBuildCommandPrologue }}"
	# Make sure to cd to the source directory so that the post-build script runs from there
	cd $SOURCE_DIR
	{{ PostBuildCommand }}
	echo "{{ PostBuildCommandEpilogue }}"
	{{ end }}
{{ end }}

{{ if ManifestFileName | IsNotBlank }}
MANIFEST_FILE={{ ManifestFileName }}

MANIFEST_DIR={{ ManifestDir }}
if [ -z "$MANIFEST_DIR" ];then
	MANIFEST_DIR="$DESTINATION_DIR"
fi
mkdir -p "$MANIFEST_DIR"

echo
echo "Removing existing manifest file"
rm -f "$MANIFEST_DIR/$MANIFEST_FILE"
{{ if BuildProperties != empty }}
echo "Creating a manifest file..."
{{ for prop in BuildProperties }}
echo "{{ prop.Key }}=\"{{ prop.Value }}\"" >> "$MANIFEST_DIR/$MANIFEST_FILE"
{{ end }}
echo "Manifest file created."
{{ end }}
{{ end }}

echo
echo "Done in $TOTAL_EXECUTION_ELAPSED_TIME sec(s)."