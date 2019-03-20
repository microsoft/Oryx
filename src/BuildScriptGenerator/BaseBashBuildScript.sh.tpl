#!/bin/bash
set -e

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

if [ "$SOURCE_DIR" != "$DESTINATION_DIR" ]
then
	if [ "$(ls -A $DESTINATION_DIR)" ]
	then
		echo
		echo "Destination directory is not empty. Deleting its contents ..."
		rm -rf "$DESTINATION_DIR"/*
	fi
fi

if [ ! -z "$INTERMEDIATE_DIR" ]
then
	echo "Using intermediate directory '$INTERMEDIATE_DIR'."
	if [ ! -d "$INTERMEDIATE_DIR" ]
	then
		echo
		echo "Intermediate directory doesn't exist, creating it ...'"
		mkdir -p "$INTERMEDIATE_DIR"		
	fi

	cd "$INTERMEDIATE_DIR"
	INTERMEDIATE_DIR=$(pwd -P)
	cd "$SOURCE_DIR"
	echo
	echo "Copying files to the intermediate directory..."
	excludedDirectories=""
	{{ for excludedDir in DirectoriesToExcludeFromCopyToIntermediateDir }}
	excludedDirectories+=" --exclude {{ excludedDir }}"
	{{ end }}
	rsync --delete -rt $excludedDirectories . "$INTERMEDIATE_DIR"
	echo "Finished copying files to intermediate directory."
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
# Makes sure every snipped starts in the context of the source directory.
cd "$SOURCE_DIR"
{{~ Snippet }}
{{ end }}

if [ "$SOURCE_DIR" != "$DESTINATION_DIR" ]
then
	cd "$SOURCE_DIR"
	mkdir -p "$DESTINATION_DIR"
	echo
	echo "Copying files to destination directory '$DESTINATION_DIR' ..."
	excludedDirectories=""
	{{ for excludedDir in DirectoriesToExcludeFromCopyToBuildOutputDir }}
	excludedDirectories+=" --exclude {{ excludedDir }}"
	{{ end }}
	rsync -rtE --links $excludedDirectories . "$DESTINATION_DIR"
	echo "Finished copying files to destination directory."
fi

{{ if PostBuildScriptPath | IsNotBlank }}
# Make sure to cd to the source directory so that the post-build script runs from there
cd $SOURCE_DIR
echo "{{ PostBuildScriptPrologue }}"
"{{ PostBuildScriptPath }}"
echo "{{ PostBuildScriptEpilogue }}"
{{ end }}

echo
echo Done.
