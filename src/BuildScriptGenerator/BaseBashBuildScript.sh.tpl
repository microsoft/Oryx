#!/bin/bash
set -e

TOTAL_EXECUTION_START_TIME=$SECONDS
SOURCE_DIR="$1"
DESTINATION_DIR="$2"
INTERMEDIATE_DIR="$3"

if [ -f {{ LoggerPath }} ]; then
	source {{ LoggerPath }}
fi

if [ ! -d "$SOURCE_DIR" ]; then
    echo "Source directory '$SOURCE_DIR' does not exist." 1>&2
    exit 1
fi

{{ # Get full file paths to source and destination directories }}
cd "$SOURCE_DIR"
SOURCE_DIR=$(pwd -P)

if [ -z "$DESTINATION_DIR" ]
then
    DESTINATION_DIR="$SOURCE_DIR"
fi

if [ -d "$DESTINATION_DIR" ]
then
    cd "$DESTINATION_DIR"
    DESTINATION_DIR=$(pwd -P)
fi

{{ if OutputDirectoryIsNested }}
{{ ## For 1st build this is not a problem, but for subsequent builds we want the source directory to be
 in a clean state to avoid considering earlier build's state and potentially yielding incorrect results. ## }}
rm -rf "$DESTINATION_DIR"
{{ end }}

if [ ! -z "$INTERMEDIATE_DIR" ]
then
	echo "Using intermediate directory '$INTERMEDIATE_DIR'."
	if [ ! -d "$INTERMEDIATE_DIR" ]
	then
		echo
		echo "Intermediate directory doesn't exist, creating it...'"
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

	{{ ## We use checksum and not the '--times' because the destination directory could be from
	 a different file system (ex: NFS) where setting modification times results in errors.
	 Even though checksum is slower compared to the '--times' option, it is more reliable
	 which is important for us. ## }}
	rsync -rcE --delete $excludedDirectories . "$INTERMEDIATE_DIR"

	ELAPSED_TIME=$(($SECONDS - $START_TIME))
	echo "Done in $ELAPSED_TIME sec(s)."
	SOURCE_DIR="$INTERMEDIATE_DIR"
fi

echo
echo "Source directory     : $SOURCE_DIR"
echo "Destination directory: $DESTINATION_DIR"
echo

{{ if PlatformInstallationScript | IsNotBlank }}
{{ PlatformInstallationScript }}
{{ end }}

cd "$SOURCE_DIR"

{{ if BenvArgs | IsNotBlank }}
if [ -f {{ BenvPath }} ]; then
	source {{ BenvPath }} {{ BenvArgs }}
fi
{{ end }}

{{ if !OsPackagesToInstall.empty? }}
apt-get update && apt-get install --yes --no-install-recommends {{ for PackageName in OsPackagesToInstall }}{{ PackageName }} {{ end }}
{{ end }}

{{ # Export these variables so that they are available for the pre and post build scripts. }}
export SOURCE_DIR
export DESTINATION_DIR

{{ ## Make sure to create the destination directory before pre and post build commands are run so that users can
access the destination directory ## }}
mkdir -p "$DESTINATION_DIR"

{{ if PreBuildCommand | IsNotBlank }}
{{ # Make sure to cd to the source directory so that the pre-build script runs from there }}
cd "$SOURCE_DIR"
echo "{{ PreBuildCommandPrologue }}"
{{ PreBuildCommand }}
echo "{{ PreBuildCommandEpilogue }}"
{{ end }}

{{ for Snippet in BuildScriptSnippets }}
{{ # Makes sure every snippet starts in the context of the source directory. }}
cd "$SOURCE_DIR"
{{~ Snippet }}
{{ end }}

{{ if PostBuildCommand | IsNotBlank }}
{{ # Make sure to cd to the source directory so that the post-build script runs from there }}
cd $SOURCE_DIR
echo
echo "{{ PostBuildCommandPrologue }}"
{{ PostBuildCommand }}
echo "{{ PostBuildCommandEpilogue }}"
{{ end }}

if [ "$SOURCE_DIR" != "$DESTINATION_DIR" ]
then
	echo "Preparing output..."


	{{ if CopySourceDirectoryContentToDestinationDirectory }}
		cd "$SOURCE_DIR"

	echo
	echo "Copying files to destination directory '$DESTINATION_DIR'..."
	START_TIME=$SECONDS
	excludedDirectories=""
	excludedDirsCount=0
	{{ for excludedDir in DirectoriesToExcludeFromCopyToBuildOutputDir }}
	excludedDirectories+=" --exclude {{ excludedDir }}"
	echo "Excluding directory '{{ excludedDir }}' from copy to destination directory."
	excludedDirsCount=$((excludedDirsCount + 1))
	{{ end }}
	echo "Total directories excluded from copy to build output: $excludedDirsCount"		{{ if OutputDirectoryIsNested }}
		{{ ## We create destination directory upfront for scenarios where pre or post build commands need access
		to it. This espceially hanldes the scenario where output directory is a sub-directory of a source directory ## }}
		tmpDestinationDir="/tmp/__oryxDestinationDir"
		if [ -d "$DESTINATION_DIR" ]; then
			mkdir -p "$tmpDestinationDir"
			rsync -rcE --links "$DESTINATION_DIR/" "$tmpDestinationDir"
			rm -rf "$DESTINATION_DIR"
		fi
		{{ end }}

		{{ ## We use checksum and not the '--times' because the destination directory could be from
		 a different file system (ex: NFS) where setting modification times results in errors.
		 Even though checksum is slower compared to the '--times' option, it is more reliable
		 which is important for us. ## }}
		rsync -rcE --links $excludedDirectories . "$DESTINATION_DIR"

		{{ if OutputDirectoryIsNested }}
		if [ -d "$tmpDestinationDir" ]; then
			{{ # Do not overwrite files in destination directory }}
			rsync -rcE --links "$tmpDestinationDir/" "$DESTINATION_DIR"
			rm -rf "$tmpDestinationDir"
		fi
		{{ end }}

		ELAPSED_TIME=$(($SECONDS - $START_TIME))
		echo "Done in $ELAPSED_TIME sec(s)."
	{{ end }}

	{{ if CompressDestinationDir }}
	cd "$SOURCE_DIR"
	echo "Compressing content of directory '$SOURCE_DIR'..."
	START_TIME=$SECONDS
	tarExclusions=""
	{{ for excludedDir in DirectoriesToExcludeFromCopyToBuildOutputDir }}
	tarExclusions+=" --exclude={{ excludedDir }}"
	{{ end }}

	# Determine compression method based on ORYX_COMPRESS_TYPE environment variable
	# Supported values: gzip (default), lz4, zst
	COMPRESS_TYPE="${ORYX_COMPRESS_TYPE:-gzip}"
	
	case "$COMPRESS_TYPE" in
		lz4)
			echo "Using lz4 compression..."
			tar -I lz4 -cf "$DESTINATION_DIR/output.tar.lz4" $tarExclusions .
			echo "Copied the compressed output to '$DESTINATION_DIR/output.tar.lz4'"
			;;
		zst|zstd)
			echo "Using zstd compression..."
			tar -I zstd -cf "$DESTINATION_DIR/output.tar.zst" $tarExclusions .
			echo "Copied the compressed output to '$DESTINATION_DIR/output.tar.zst'"
			;;
		gzip|gz|*)
			echo "Using gzip compression..."
			tar -zcf "$DESTINATION_DIR/output.tar.gz" $tarExclusions .
			echo "Copied the compressed output to '$DESTINATION_DIR/output.tar.gz'"
			;;
	esac

	ELAPSED_TIME=$(($SECONDS - $START_TIME))
	echo "Done in $ELAPSED_TIME sec(s)."

	cp ./requirements.txt "$DESTINATION_DIR/requirements.txt"
	echo "Copied requirements.txt to '$DESTINATION_DIR'"
	{{ end }}
fi

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

OS_TYPE_SOURCE_DIR="/opt/oryx/.ostype"
if [ -f "$OS_TYPE_SOURCE_DIR" ]
then
	echo "Copying .ostype to manifest output directory."
	cp "$OS_TYPE_SOURCE_DIR" "$MANIFEST_DIR/.ostype"
else
	echo "File $OS_TYPE_SOURCE_DIR does not exist. Cannot copy to manifest directory." 1>&2
	exit 1
fi

TOTAL_EXECUTION_ELAPSED_TIME=$(($SECONDS - $TOTAL_EXECUTION_START_TIME))
echo
echo "Done in $TOTAL_EXECUTION_ELAPSED_TIME sec(s)."