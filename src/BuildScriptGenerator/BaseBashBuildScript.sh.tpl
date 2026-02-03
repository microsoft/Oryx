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
	BASE_START_TIME=$SECONDS
	excludedDirectories=""
	{{ for excludedDir in DirectoriesToExcludeFromCopyToIntermediateDir }}
	excludedDirectories+=" --exclude {{ excludedDir }}"
	{{ end }}

	{{ ## We use checksum and not the '--times' because the destination directory could be from
	 a different file system (ex: NFS) where setting modification times results in errors.
	 Even though checksum is slower compared to the '--times' option, it is more reliable
	 which is important for us. ## }}
	rsync -rcE --delete $excludedDirectories . "$INTERMEDIATE_DIR"

	ELAPSED_TIME=$(($SECONDS - $BASE_START_TIME))
	echo "Copying files to intermediate directory done in $ELAPSED_TIME sec(s)."
	SOURCE_DIR="$INTERMEDIATE_DIR"
fi

echo
echo "Source directory     : $SOURCE_DIR"
echo "Destination directory: $DESTINATION_DIR"
echo

{{ if PlatformInstallationScript | IsNotBlank }}
echo "Installing platform..."
BASE_START_TIME=$SECONDS
{{ PlatformInstallationScript }}
ELAPSED_TIME=$(($SECONDS - $BASE_START_TIME))
echo "Platform installation done in $ELAPSED_TIME sec(s)."
{{ end }}

cd "$SOURCE_DIR"

{{ if BenvArgs | IsNotBlank }}
if [ -f {{ BenvPath }} ]; then
	source {{ BenvPath }} {{ BenvArgs }}
fi
{{ end }}

{{ if !OsPackagesToInstall.empty? }}
echo "Installing OS packages..."
BASE_START_TIME=$SECONDS
apt-get update && apt-get install --yes --no-install-recommends {{ for PackageName in OsPackagesToInstall }}{{ PackageName }} {{ end }}
ELAPSED_TIME=$(($SECONDS - $BASE_START_TIME))
echo "OS packages installation done in $ELAPSED_TIME sec(s)."
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
BASE_START_TIME=$SECONDS
{{ PreBuildCommand }}
ELAPSED_TIME=$(($SECONDS - $BASE_START_TIME))
echo "{{ PreBuildCommandEpilogue }}"
echo "Pre-build command done in $ELAPSED_TIME sec(s)."
{{ end }}

echo "Running build script snippets..."
BASE_START_TIME=$SECONDS
{{ for Snippet in BuildScriptSnippets }}
{{ # Makes sure every snippet starts in the context of the source directory. }}
cd "$SOURCE_DIR"
{{~ Snippet }}
{{ end }}
ELAPSED_TIME=$(($SECONDS - $BASE_START_TIME))
echo "Build script snippets done in $ELAPSED_TIME sec(s)."

{{ if PostBuildCommand | IsNotBlank }}
{{ # Make sure to cd to the source directory so that the post-build script runs from there }}
cd $SOURCE_DIR
echo
echo "{{ PostBuildCommandPrologue }}"
BASE_START_TIME=$SECONDS
{{ PostBuildCommand }}
ELAPSED_TIME=$(($SECONDS - $BASE_START_TIME))
echo "{{ PostBuildCommandEpilogue }}"
echo "Post-build command done in $ELAPSED_TIME sec(s)."
{{ end }}

if [ "$SOURCE_DIR" != "$DESTINATION_DIR" ]
then
	echo "Preparing output..."

	{{ ## Determine if direct tar compression can be used based on build configuration ## }}
	CAN_USE_DIRECT_COMPRESSION_TO_DEST=false
	{{ if CompressDestinationDir && CopySourceDirectoryContentToDestinationDirectory && !OutputDirectoryIsNested }}
	CAN_USE_DIRECT_COMPRESSION_TO_DEST=true
	{{ end }}

	{{ ## Check if optimized direct tar compression is enabled ## }}
	if [ "$CAN_USE_DIRECT_COMPRESSION_TO_DEST" = "true" ] && [ "$ENABLE_ORYX_DIRECT_TAR_COMPRESSION" = "true" ]; then
		{{ ## Optimized path: Create tar directly from source to destination without intermediate copy ## }}
		echo "Compressing source directory directly to destination (optimized path)..."
		BASE_START_TIME=$SECONDS
		cd "$SOURCE_DIR"
		
		excludedDirectories=""
		{{ for excludedDir in DirectoriesToExcludeFromCopyToBuildOutputDir }}
		excludedDirectories+=" --exclude={{ excludedDir }}"
		{{ end }}

		COMPRESSION_DONE=false
		if [ "$ORYX_COMPRESS_WITH_ZSTD" = "true" ]; then
			rm -f "$DESTINATION_DIR/output.tar.gz" 2>/dev/null || true
			echo "Using zstd for compression"
			set +e
			output=$( ( tar -I zstd -cf "$DESTINATION_DIR/output.tar.zst" $excludedDirectories . ; exit ${PIPESTATUS[0]} ) 2>&1; exit ${PIPESTATUS[0]} )
			compressionExitCode=${PIPESTATUS[0]}
			set -e
			if [[ $compressionExitCode -eq 0 ]]; then
				ELAPSED_TIME=$(($SECONDS - $BASE_START_TIME))
				echo "Copied the compressed output to '$DESTINATION_DIR'"
				echo "Direct compression with zstd done in $ELAPSED_TIME sec(s)."
				COMPRESSION_DONE=true
			else
				echo "WARNING: Direct compression with zstd failed: $output, exit code: $compressionExitCode"
				echo "Falling back to gzip compression."
			fi
		fi

		if [ "$COMPRESSION_DONE" = "false" ]; then
			if [ -f "$DESTINATION_DIR/output.tar.zst" ]; then
				rm -f "$DESTINATION_DIR/output.tar.zst" 2>/dev/null || true
			fi
			BASE_START_TIME=$SECONDS
			echo "Using gzip for compression"
			tar -zcf "$DESTINATION_DIR/output.tar.gz" $excludedDirectories .
			ELAPSED_TIME=$(($SECONDS - $BASE_START_TIME))
			echo "Copied the compressed output to '$DESTINATION_DIR'"
			echo "Direct compression with gzip done in $ELAPSED_TIME sec(s)."
		fi
	else
		echo "Using standard output preparation..."
		{{ ## When compressing destination directory is chosen, we want to copy the source content to a temporary 
		destination directory first, compress the content there and then copy that content to the final destination 
		directory ## }}
		{{ if CompressDestinationDir }}
		preCompressedDestinationDir="/tmp/_preCompressedDestinationDir"
		rm -rf $preCompressedDestinationDir
		OLD_DESTINATION_DIR="$DESTINATION_DIR"
		DESTINATION_DIR="$preCompressedDestinationDir"
		{{ end }}

		{{ if CopySourceDirectoryContentToDestinationDirectory }}
			cd "$SOURCE_DIR"

			echo
			echo "Copying files to destination directory '$DESTINATION_DIR'..."
			BASE_START_TIME=$SECONDS
			excludedDirectories=""
			{{ for excludedDir in DirectoriesToExcludeFromCopyToBuildOutputDir }}
			excludedDirectories+=" --exclude {{ excludedDir }}"
			{{ end }}

			{{ if OutputDirectoryIsNested }}
			{{ ## We create destination directory upfront for scenarios where pre or post build commands need access
			to it. This espceially hanldes the scenario where output directory is a sub-directory of a source directory ## }}
			tmpDestinationDir="/tmp/__oryxDestinationDir"
			if [ -d "$DESTINATION_DIR" ]; then
				echo "Copying existing destination directory to temporary location..."
				TEMP_START_TIME=$SECONDS
				mkdir -p "$tmpDestinationDir"
				rsync -rcE --links "$DESTINATION_DIR/" "$tmpDestinationDir"
				TEMP_ELAPSED_TIME=$(($SECONDS - $TEMP_START_TIME))
				echo "Copying to temporary location done in $TEMP_ELAPSED_TIME sec(s)."
				rm -rf "$DESTINATION_DIR"
			fi
			{{ end }}

			{{ ## We use checksum and not the '--times' because the destination directory could be from
			 a different file system (ex: NFS) where setting modification times results in errors.
			 Even though checksum is slower compared to the '--times' option, it is more reliable
			 which is important for us. ## }}
			MAIN_RSYNC_START_TIME=$SECONDS
			rsync -rcE --links $excludedDirectories . "$DESTINATION_DIR"
			MAIN_RSYNC_ELAPSED_TIME=$(($SECONDS - $MAIN_RSYNC_START_TIME))
			echo "Copying to destination directory done in $MAIN_RSYNC_ELAPSED_TIME sec(s)."

			{{ if OutputDirectoryIsNested }}
			if [ -d "$tmpDestinationDir" ]; then
				echo "Copying back temporary destination directory contents..."
				TEMP_START_TIME=$SECONDS
				{{ # Do not overwrite files in destination directory }}
				rsync -rcE --links "$tmpDestinationDir/" "$DESTINATION_DIR"
				TEMP_ELAPSED_TIME=$(($SECONDS - $TEMP_START_TIME))
				echo "Copying back from temporary location done in $TEMP_ELAPSED_TIME sec(s)."
				rm -rf "$tmpDestinationDir"
			fi
			{{ end }}

			ELAPSED_TIME=$(($SECONDS - $BASE_START_TIME))
			echo "Total time for destination directory preparation done in $ELAPSED_TIME sec(s)."
		{{ else }}
			{{ if CompressDestinationDir }}
				{{ ## In case of .NET apps, 'dotnet publish' writes to original destination directory. So here we are 
				trying to move the files to the temporary destination directory so that they get compressed and these 
				compressed files are copied to final destination directory ## }}
				origDestDir="$OLD_DESTINATION_DIR"
				tempDestDir="$DESTINATION_DIR"
				cd $origDestDir
				shopt -s dotglob
				mkdir -p $tempDestDir
				echo "Moving files to temporary directory for compression..."
				MV_START_TIME=$SECONDS
				mv * "$tempDestDir/"
				MV_ELAPSED_TIME=$(($SECONDS - $MV_START_TIME))
				echo "Moving files done in $MV_ELAPSED_TIME sec(s)."
			{{ end }}
		{{ end }}

		{{ if CompressDestinationDir }}
		DESTINATION_DIR="$OLD_DESTINATION_DIR"
		echo "Compressing content of directory '$preCompressedDestinationDir'..."
		BASE_START_TIME=$SECONDS
		cd "$preCompressedDestinationDir"

		COMPRESSION_DONE=false
		if [ "$ORYX_COMPRESS_WITH_ZSTD" = "true" ]; then
			rm -f "$DESTINATION_DIR/output.tar.gz" 2>/dev/null || true
			echo "Using zstd for compression"
			set +e
			output=$( ( tar -I zstd -cf "$DESTINATION_DIR/output.tar.zst" . ; exit ${PIPESTATUS[0]} ) 2>&1; exit ${PIPESTATUS[0]} )
			compressionExitCode=${PIPESTATUS[0]}
			set -e
			if [[ $compressionExitCode -eq 0 ]]; then
				ELAPSED_TIME=$(($SECONDS - $BASE_START_TIME))
				echo "Copied the compressed output to '$DESTINATION_DIR'"
				echo "Compression with zstd done in $ELAPSED_TIME sec(s)."
				COMPRESSION_DONE=true
			else
				echo "WARNING: Compression with zstd failed: $output, exit code: $compressionExitCode"
				echo "Falling back to gzip compression."
			fi
		fi

		if [ "$COMPRESSION_DONE" = "false" ]; then
			if [ -f "$DESTINATION_DIR/output.tar.zst" ]; then
				rm -f "$DESTINATION_DIR/output.tar.zst" 2>/dev/null || true
			fi
			BASE_START_TIME=$SECONDS
			echo "Using gzip for compression"
			tar -zcf "$DESTINATION_DIR/output.tar.gz" .
			ELAPSED_TIME=$(($SECONDS - $BASE_START_TIME))
			echo "Copied the compressed output to '$DESTINATION_DIR'"
			echo "Compression with gzip done in $ELAPSED_TIME sec(s)."
		fi
		{{ end }}
	fi
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
echo "Total execution done in $TOTAL_EXECUTION_ELAPSED_TIME sec(s)."