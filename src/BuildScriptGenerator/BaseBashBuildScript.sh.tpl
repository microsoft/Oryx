#!/bin/bash
set -e

TOTAL_EXECUTION_START_TIME=$SECONDS
SOURCE_DIR="$1"
DESTINATION_DIR="$2"
INTERMEDIATE_DIR="$3"
ORYX_PROGRESS_RESTORE_XTRACE=false
case "$-" in
	*x*)
		{ set +x; } 2>/dev/null
		ORYX_PROGRESS_RESTORE_XTRACE=true
		;;
esac
ORYX_PROGRESS_OWNER="${4:-standalone}"
ORYX_PROGRESS_ENDPOINT_VALUE=
ORYX_PROGRESS_OPERATION_ID_VALUE=
if [ "$ORYX_PROGRESS_OWNER" = "oryx-cli" ]; then
	ORYX_PROGRESS_ENDPOINT_VALUE="${ORYX_PROGRESS_ENDPOINT:-}"
	ORYX_PROGRESS_OPERATION_ID_VALUE="${ORYX_PROGRESS_OPERATION_ID:-}"
	unset ORYX_PROGRESS_ENDPOINT ORYX_PROGRESS_OPERATION_ID
fi

ORYX_PROGRESS_ENABLED=false
ORYX_PROGRESS_EVENT_COUNT=0
ORYX_PROGRESS_LAST_PHASE_RANK=1
ORYX_PROGRESS_MAX_EVENTS=32
ORYX_PROGRESS_MAX_RECORD_BYTES=1024
ORYX_PROGRESS_MAX_FILE_BYTES=32768
ORYX_PROGRESS_PHASE_COMMAND=:
ORYX_PROGRESS_TERMINAL_COMMAND=:

if [ "$ORYX_PROGRESS_OWNER" = "oryx-cli" ]; then
	case "$ORYX_PROGRESS_ENDPOINT_VALUE" in
		file:/*)
			ORYX_PROGRESS_FILE="${ORYX_PROGRESS_ENDPOINT_VALUE#file:}"
			;;
	esac

	if [ -n "${ORYX_PROGRESS_FILE:-}" ] &&
		[[ "$ORYX_PROGRESS_OPERATION_ID_VALUE" =~ ^[A-Za-z0-9][A-Za-z0-9._:-]{0,255}$ ]] &&
		! builtin printf '%s' "$ORYX_PROGRESS_FILE" | LC_ALL=C /usr/bin/grep -q '[[:cntrl:]]'; then
		ORYX_PROGRESS_ENABLED=true
	fi
fi

if [ "$ORYX_PROGRESS_ENABLED" = "true" ] &&
	! declare -F oryx_progress_transition >/dev/null &&
	! declare -F oryx_progress_phase_started >/dev/null &&
	! declare -F oryx_progress_build_completed >/dev/null; then
oryx_progress_transition() {
	if [ "$ORYX_PROGRESS_ENABLED" != "true" ]; then
		return 0
	fi

	local eventType="$1"
	local value="$2"
	local rank
	case "$eventType:$value" in
		phase_started:script.generate) rank=1 ;;
		phase_started:pre_build) rank=2 ;;
		phase_started:dependencies.restore) rank=3 ;;
		phase_started:build.execute) rank=4 ;;
		phase_started:post_build) rank=5 ;;
		phase_started:output.prepare) rank=6 ;;
		phase_started:output.compress) rank=7 ;;
		phase_started:manifest.write) rank=8 ;;
		build_completed:succeeded) rank=9 ;;
		*) return 0 ;;
	esac
	if [ "$rank" -le "$ORYX_PROGRESS_LAST_PHASE_RANK" ]; then
		return 0
	fi

	local timestamp
	if ! timestamp=$(/usr/bin/date -u +"%Y-%m-%dT%H:%M:%S.%3NZ"); then
		ORYX_PROGRESS_ENABLED=false
		return 0
	fi

	local record
	if [ "$eventType" = "phase_started" ]; then
		builtin printf -v record \
			'{"schemaVersion":1,"eventType":"phase_started","operationId":"%s","timestampUtc":"%s","phase":"%s"}' \
			"$ORYX_PROGRESS_OPERATION_ID_VALUE" \
			"$timestamp" \
			"$value"
	else
		builtin printf -v record \
			'{"schemaVersion":1,"eventType":"build_completed","operationId":"%s","timestampUtc":"%s","outcome":"succeeded"}' \
			"$ORYX_PROGRESS_OPERATION_ID_VALUE" \
			"$timestamp"
	fi

	if [ "$ORYX_PROGRESS_EVENT_COUNT" -ge "$ORYX_PROGRESS_MAX_EVENTS" ]; then
		ORYX_PROGRESS_ENABLED=false
		return 0
	fi

	local recordBytes=$((${#record} + 1))
	if [ "$recordBytes" -gt "$ORYX_PROGRESS_MAX_RECORD_BYTES" ]; then
		ORYX_PROGRESS_ENABLED=false
		return 0
	fi

	{{ ## The endpoint must be a pre-created local regular file. The bounded child revalidates the
	     descriptor immediately before the single-record append. ## }}
	local restoreMonitorMode=false
	case "$-" in
		*m*)
			set +m
			restoreMonitorMode=true
			;;
	esac
	/usr/bin/setsid /usr/bin/env -u BASH_ENV /bin/bash -c '
		endpointFile=$(/usr/bin/realpath -m -s -- "$1") || exit 1
		record="$2"
		maxFileBytes="$3"
		recordBytes="$4"

		[ -f "$endpointFile" ] && [ ! -L "$endpointFile" ] || exit 1
		parent="${endpointFile%/*}"
		[ -n "$parent" ] || parent="/"
		current="$parent"
		while [ "$current" != "/" ]; do
			[ -L "$current" ] && exit 1
			next="${current%/*}"
			[ -n "$next" ] || next="/"
			[ "$next" != "$current" ] || exit 1
			current="$next"
		done

		case "$(/usr/bin/stat -f -c %T -- "$parent")" in
			ext2/ext3|ext4|xfs|btrfs|tmpfs|overlay|overlayfs|ramfs|zfs|aufs) ;;
			*) exit 1 ;;
		esac

		fileBytes=$(/usr/bin/stat -c %s -- "$endpointFile") || exit 1
		[ "$((fileBytes + recordBytes))" -le "$maxFileBytes" ] || exit 1

		exec 3>>"$endpointFile" || exit 1
		[ -f "/proc/$$/fd/3" ] &&
			[ "$endpointFile" -ef "/proc/$$/fd/3" ] ||
			exit 1
		builtin printf "%s\n" "$record" >&3 || exit 1
		exec 3>&-
		/bin/sync -f -- "$endpointFile"
	' oryx-progress-writer \
		"$ORYX_PROGRESS_FILE" \
		"$record" \
		"$ORYX_PROGRESS_MAX_FILE_BYTES" \
		"$recordBytes" \
		>/dev/null 2>&1 &
	local writerPid=$!
	if [ "$restoreMonitorMode" = "true" ]; then
		set -m
	fi
	local writerFinished=false
	local writerAttempt
	for writerAttempt in {1..10}; do
		local writerState=
		if [ ! -r "/proc/$writerPid/status" ]; then
			writerFinished=true
			break
		fi
		if ! while IFS=$'\t' read -r key value remainder; do
			if [ "$key" = "State:" ]; then
				writerState="${value%% *}"
				break
			fi
		done < "/proc/$writerPid/status"; then
			writerFinished=true
			break
		fi
		if [ "$writerState" = "Z" ]; then
			writerFinished=true
			break
		fi
		/bin/sleep 0.1
	done

	if [ "$writerFinished" != "true" ]; then
		builtin kill -KILL -- "-$writerPid" 2>/dev/null || true
		disown "$writerPid" 2>/dev/null || true
		ORYX_PROGRESS_ENABLED=false
		return 0
	fi
	if ! wait "$writerPid"; then
		ORYX_PROGRESS_ENABLED=false
		return 0
	fi

	ORYX_PROGRESS_EVENT_COUNT=$((ORYX_PROGRESS_EVENT_COUNT + 1))
	ORYX_PROGRESS_LAST_PHASE_RANK="$rank"
	if [ "$eventType" = "build_completed" ]; then
		ORYX_PROGRESS_ENABLED=false
	fi
}
oryx_progress_phase_started() {
	local restoreXtrace=false
	case "$-" in
		*x*)
			{ set +x; } 2>/dev/null
			restoreXtrace=true
			;;
	esac
	oryx_progress_transition phase_started "$1"
	if [ "$restoreXtrace" = "true" ]; then
		set -x
	fi
}
oryx_progress_build_completed() {
	local restoreXtrace=false
	case "$-" in
		*x*)
			{ set +x; } 2>/dev/null
			restoreXtrace=true
			;;
	esac
	oryx_progress_transition build_completed succeeded
	if [ "$restoreXtrace" = "true" ]; then
		set -x
	fi
}
ORYX_PROGRESS_PHASE_COMMAND=oryx_progress_phase_started
ORYX_PROGRESS_TERMINAL_COMMAND=oryx_progress_build_completed
else
	ORYX_PROGRESS_ENABLED=false
fi

if [ "$ORYX_PROGRESS_RESTORE_XTRACE" = "true" ]; then
	set -x
fi
unset ORYX_PROGRESS_RESTORE_XTRACE

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
$ORYX_PROGRESS_PHASE_COMMAND "pre_build"
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
$ORYX_PROGRESS_PHASE_COMMAND "post_build"
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
		$ORYX_PROGRESS_PHASE_COMMAND "output.compress"
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
		$ORYX_PROGRESS_PHASE_COMMAND "output.prepare"
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
		$ORYX_PROGRESS_PHASE_COMMAND "output.compress"
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
$ORYX_PROGRESS_PHASE_COMMAND "manifest.write"
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

if [ -z "$DEBIAN_FLAVOR" ] && [ -n "$OS_FLAVOR" ]
then
	echo "Generating .ostype from OS_FLAVOR environment variable."
	echo "UBUNTU|$OS_FLAVOR" | tr '[a-z]' '[A-Z]' > "$MANIFEST_DIR/.ostype"
elif [ -n "$DEBIAN_FLAVOR" ]
then
	echo "Generating .ostype from DEBIAN_FLAVOR environment variable."
	echo "DEBIAN|$DEBIAN_FLAVOR" | tr '[a-z]' '[A-Z]' > "$MANIFEST_DIR/.ostype"
elif [ -f "/opt/oryx/.ostype" ]
then
	echo "Copying .ostype from /opt/oryx/.ostype to manifest output directory."
	cp "/opt/oryx/.ostype" "$MANIFEST_DIR/.ostype"
else
	echo "No OS flavor environment variable set and /opt/oryx/.ostype does not exist. Cannot generate .ostype." 1>&2
	exit 1
fi
TOTAL_EXECUTION_ELAPSED_TIME=$(($SECONDS - $TOTAL_EXECUTION_START_TIME))
echo
echo "Total execution done in $TOTAL_EXECUTION_ELAPSED_TIME sec(s)."
$ORYX_PROGRESS_TERMINAL_COMMAND