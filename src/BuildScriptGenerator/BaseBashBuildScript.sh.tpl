#!/bin/bash
set -e

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

echo
echo "Source directory     : $SOURCE_DIR"
echo "Destination directory: $DESTINATION_DIR"
echo

{{ if BenvArgs | IsNotBlank }}
if [ -f /usr/local/bin/benv ]; then
	source /usr/local/bin/benv {{ BenvArgs }}
fi
{{ end }}

{{ if PreBuildScriptPath | IsNotBlank }}
# Make sure to cd to the source directory so that the pre-build script runs from there
cd "$SOURCE_DIR"
echo "{{ PreBuildScriptPrologue }}"
"{{ PreBuildScriptPath }}"
echo "{{ PreBuildScriptEpilogue }}"
{{ end }}

{{ for Snippet in BuildScriptSnippets }}
# Makes sure every snippet starts in the context of the source directory.
cd "$SOURCE_DIR"
{{~ Snippet }}
{{ end }}

{{ if PostBuildScriptPath | IsNotBlank }}
# Make sure to cd to the source directory so that the post-build script runs from there
cd $SOURCE_DIR
echo "{{ PostBuildScriptPrologue }}"
"{{ PostBuildScriptPath }}"
echo "{{ PostBuildScriptEpilogue }}"
{{ end }}

{{ if ManifestFileName | IsNotBlank }}
MANIFEST_FILE={{ ManifestFileName }}
echo
echo "Removing existing manifest file"
rm -f "$DESTINATION_DIR/$MANIFEST_FILE"
{{ if BuildProperties != empty }}
echo "Creating a manifest file..."
{{ for prop in BuildProperties }}
echo "{{ prop.Key }}=\"{{ prop.Value }}\"" >> "$DESTINATION_DIR/$MANIFEST_FILE"
{{ end }}
echo "Manifest file created."
{{ end }}
{{ end }}

TOTAL_EXECUTION_ELAPSED_TIME=$(($SECONDS - $TOTAL_EXECUTION_START_TIME))
echo
echo "Done in $TOTAL_EXECUTION_ELAPSED_TIME sec(s)."
