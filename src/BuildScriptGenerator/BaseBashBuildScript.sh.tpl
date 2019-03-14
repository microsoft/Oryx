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
	if [ -d "$DESTINATION_DIR" ]
	then
		echo
		echo Destination directory already exists. Deleting it ...
		rm -rf "$DESTINATION_DIR"
	fi
fi

if [ ! -z "$INTERMEDIATE_DIR" ]
then
	echo "Using intermediate directory '$INTERMEDIATE_DIR'."
	if [ ! -d "$INTERMEDIATE_DIR" ]
	then
		echo "Intermediate directory doesn't exist, creating it...'"
		mkdir -p "$INTERMEDIATE_DIR"		
	fi

	cd "$INTERMEDIATE_DIR"
	INTERMEDIATE_DIR=$(pwd -P)
	cd "$SOURCE_DIR"
	# TODO make the exclusion list be dynamic, from a list provided by the languages
	echo
	echo "Copying files to the intermediate directory..."
	rsync --delete -rt --exclude node_modules.zip --exclude node_modules --exclude .git . "$INTERMEDIATE_DIR"
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
cd $SOURCE_DIR
echo "{{ PreBuildScriptPrologue }}"
"{{ PreBuildScriptPath }}"
echo "{{ PreBuildScriptEpilogue }}"
{{ end }}

{{ for Snippet in BuildScriptSnippets }}
{{~ Snippet }}
{{ end }}

if [ "$SOURCE_DIR" != "$DESTINATION_DIR" ]
then
	cd "$SOURCE_DIR"
	mkdir -p "$DESTINATION_DIR"
	echo
	echo "Copying files to destination directory, '$DESTINATION_DIR'"
	# TODO make the exclusion list dynamic, provided by each language
	if [ "$ENABLE_NODE_MODULES_ZIP" == "true" ]; then
		rsync -rtE --links --exclude node_modules --exclude .git . "$DESTINATION_DIR"
	else
		rsync -rtE --links --exclude .git . "$DESTINATION_DIR"
	fi
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
