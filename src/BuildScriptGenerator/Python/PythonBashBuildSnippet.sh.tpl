set -e
# TODO: refactor redundant code. Work-item: 1476457

declare -r TS_FMT='[%T%z] '
declare -r REQS_NOT_FOUND_MSG='Could not find requirements.txt, pyproject.toml, or setup.py; Not installing dependencies. More information: https://aka.ms/requirements-not-found'
echo "Python Version: $python"

# Cache directories for package installation
PIP_CACHE_DIR=/usr/local/share/pip-cache
UV_PIP_CACHE_DIR=/usr/local/share/uv-pip-cache

{{ if PythonBuildCommandsFileName | IsNotBlank }}
COMMAND_MANIFEST_FILE="{{ PythonBuildCommandsFileName }}"
{{ end }}

echo "Creating directory for command manifest file if it does not exist"
mkdir -p "$(dirname "$COMMAND_MANIFEST_FILE")"
echo "Removing existing manifest file"
rm -f "$COMMAND_MANIFEST_FILE"

echo "PlatformWithVersion=Python {{ PythonVersion }}" > "$COMMAND_MANIFEST_FILE"

InstallCommand=""

# Create PIP cache directory if it doesn't exist
if [ ! -d "$PIP_CACHE_DIR" ];then
    mkdir -p $PIP_CACHE_DIR
fi

{{ if CustomRequirementsTxtPath | IsNotBlank }}
    REQUIREMENTS_TXT_FILE="{{ CustomRequirementsTxtPath }}"
{{ else }}
    REQUIREMENTS_TXT_FILE="requirements.txt"
{{ end }}

# Function to install packages via uv
ensure_uv() {
    local python_cmd=$1

    if ! command -v uv &> /dev/null; then
        echo "Installing uv..."
        local install_uv_cmd="$python_cmd -m pip install uv"
        printf %s " , $install_uv_cmd" >> "$COMMAND_MANIFEST_FILE"
        "$python_cmd" -m pip install uv
        return $?
    fi

    echo "uv is already installed, skipping installation..."
    return 0
}

install_via_uv() {
    # Create UV cache directory if it doesn't exist
    if [ ! -d "$UV_PIP_CACHE_DIR" ];then
        mkdir -p $UV_PIP_CACHE_DIR
    fi

    START_TIME=$SECONDS
    local python_cmd=$1
    local requirements_file=$2
    local target_dir=$3
    local upgrade_flag=$4
    
    ensure_uv "$python_cmd"
    
    set +e
    echo "Running uv pip install..."
    
    # Build the command with --no-build to only use pre-built wheels
    local base_cmd="uv pip install --cache-dir $UV_PIP_CACHE_DIR --compile-bytecode"
    
    # Add find-links if PYTHON_PRELOADED_WHEELS_DIR is set
    if [ -n "$PYTHON_PRELOADED_WHEELS_DIR" ]; then
        echo "Using preloaded wheels from: $PYTHON_PRELOADED_WHEELS_DIR"
        base_cmd="$base_cmd --find-links=$PYTHON_PRELOADED_WHEELS_DIR"
    fi
    
    base_cmd="$base_cmd -r $requirements_file"
    
    if [ -n "$target_dir" ]; then
        base_cmd="$base_cmd --target=\"$target_dir\""
    fi
    if [ -n "$upgrade_flag" ]; then
        base_cmd="$base_cmd $upgrade_flag"
    fi
    
    # Log the command
    local uv_cmd="$base_cmd | ts $TS_FMT"
    printf %s " , $uv_cmd" >> "$COMMAND_MANIFEST_FILE"
    
    # Execute uv pip install (uv manages its own cache)
    output=$( ( $base_cmd | ts $TS_FMT; exit ${PIPESTATUS[0]} ) 2>&1; exit ${PIPESTATUS[0]} )
    local exit_code=${PIPESTATUS[0]}
    echo "${output}"
    ELAPSED_TIME=$(($SECONDS - $START_TIME))
    echo "uv pip install done in $ELAPSED_TIME sec(s)."
    return $exit_code
}

# Function to install packages via pip
install_via_pip() {
    START_TIME=$SECONDS
    local python_cmd=$1
    local requirements_file=$2
    local target_dir=$3
    local upgrade_flag=$4
    
    set +e
    echo "Running pip install..."
    
    # Build the command
    local base_cmd="$python_cmd -m pip install --cache-dir $PIP_CACHE_DIR --prefer-binary -r $requirements_file"
    if [ -n "$target_dir" ]; then
        base_cmd="$base_cmd --target=\"$target_dir\""
    fi
    if [ -n "$upgrade_flag" ]; then
        base_cmd="$base_cmd $upgrade_flag"
    fi

    # Add find-links if PYTHON_PRELOADED_WHEELS_DIR is set
    if [ -n "$PYTHON_PRELOADED_WHEELS_DIR" ]; then
        echo "Using preloaded wheels from: $PYTHON_PRELOADED_WHEELS_DIR"
        base_cmd="$base_cmd --find-links=$PYTHON_PRELOADED_WHEELS_DIR"
    fi
    
    # Log the command
    local pip_cmd="$base_cmd | ts $TS_FMT"
    printf %s " , $pip_cmd" >> "$COMMAND_MANIFEST_FILE"
    
    # Execute pip install
    output=$( ( $base_cmd | ts $TS_FMT; exit ${PIPESTATUS[0]} ) 2>&1; exit ${PIPESTATUS[0]} )
    local exit_code=${PIPESTATUS[0]}
    echo "${output}"
    ELAPSED_TIME=$(($SECONDS - $START_TIME))
    echo "pip install done in $ELAPSED_TIME sec(s)."
    return $exit_code
}

log_dependency_resolution_elapsed() {
    local operation=$1
    local start_time=$2
    local elapsed_time=$(($SECONDS - $start_time))
    echo "$operation done in $elapsed_time sec(s)."
}

# We sanitize to avoid any secrets from getting logged.
sanitize_dependency_resolution() {
    local source_file=$1
    local destination_file=$2
    local python_cmd=$3

    "$python_cmd" - "$source_file" "$destination_file" <<'PY'
import re
import sys

source_path, destination_path = sys.argv[1:3]
with open(source_path, encoding="utf-8") as source:
    resolution = source.read()

resolution = re.sub(
    r"[A-Za-z][A-Za-z0-9+.-]*://\S+",
    "[redacted]",
    resolution,
)

with open(destination_path, "w", encoding="utf-8") as destination:
    destination.write(resolution)
PY
}

clear_dependency_resolution_artifacts() {
    local output_dir=$1

    if [ -z "$output_dir" ]; then
        return
    fi

    rm -f -- \
        "$output_dir/dependency-resolution-metadata.json" \
        "$output_dir/dependency-resolution.txt" \
        "$output_dir/dependency-resolution.json"
}

# Once we determine the transitivie dependencies with their exact version,
# we write two files to the specified output_dir.
# 1. dependency-resolution-metadata.json - Contains infomation about ecosystem, manager and path to resolved dependencies file.
# 2. dependency-resolution.txt - The exact dependencies compiled by uv.
publish_dependency_resolution() {
    local source_file=$1
    local output_dir=$2
    local python_cmd=$3
    local resolution_file_name="dependency-resolution.txt"

    if ! (mkdir -p -- "$output_dir"); then
        return 1
    fi

    (
        local metadata_file_name="dependency-resolution-metadata.json"
        local staging_suffix=".$$.${RANDOM}.tmp"
        local staged_resolution_file="$output_dir/$resolution_file_name$staging_suffix"
        local staged_metadata_file="$output_dir/$metadata_file_name$staging_suffix"
        local resolution_file="$output_dir/$resolution_file_name"
        local metadata_file="$output_dir/$metadata_file_name"

        rm -f -- "$metadata_file"

        # Redact secrets from dependency-resolution files.
        sanitize_dependency_resolution \
            "$source_file" \
            "$staged_resolution_file" \
            "$python_cmd" || {
                rm -f -- "$staged_resolution_file"
                exit 1
            }

        # Use Python to generate a json file.        
        "$python_cmd" -c \
            'import json, sys; json.dump({"schemaVersion": 1, "manager": sys.argv[1], "dependencyResolutionFilePath": sys.argv[2]}, sys.stdout, indent=2); print()' \
            "uv" \
            "$resolution_file" \
            > "$staged_metadata_file" || {
                rm -f -- "$staged_resolution_file"
                exit 1
            }

        # Move these files to the destination directory.
        mv -f -- "$staged_resolution_file" "$resolution_file" || {
            rm -f -- "$staged_metadata_file"
            exit 1
        }
        mv -f -- "$staged_metadata_file" "$metadata_file" || {
            rm -f -- "$resolution_file" "$staged_metadata_file"
            exit 1
        }

        rm -f -- "$output_dir/dependency-resolution.json"
    )
}

# Resolves exact direct and transitive dependencies, installs that exact
# resolution with uv, and publishes a sanitized artifact after installation.
install_with_dependency_resolution() {
    local python_cmd=$1
    local requirements_file=$2
    local target_dir=$3
    local upgrade_flag=$4
    local temp_dir=""
    local dependency_resolution_output_dir={{ DependencyResolutionOutputDirBashValue }}

    clear_dependency_resolution_artifacts "$dependency_resolution_output_dir"

    if [ "$PYTHON_FAST_BUILD_ENABLED" != "true" ]; then
        echo "Oryx dependency resolution is unavailable because Python fast build is disabled; deployment will continue using pip installation."
        install_via_pip "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag"
        return $?
    fi

    temp_dir=$(mktemp -d 2> /dev/null)
    if [ -z "$temp_dir" ]; then
        echo "Oryx dependency resolution was unavailable because a temporary directory could not be created; deployment will continue using the original Oryx installation path."
        install_python_packages "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag"
        return $?
    fi

    local dependency_resolution_file="$temp_dir/dependency-resolution"
    local resolve_cmd
    local resolve_exit_code=0
    local resolution_start_time=$SECONDS

    # Resolve exact direct and transitive versions for capture and installation.
    ensure_uv "$python_cmd" || resolve_exit_code=$?
    resolve_cmd=(uv pip compile --python "$python_cmd" --cache-dir "$UV_PIP_CACHE_DIR" --no-header --no-annotate --output-file "$dependency_resolution_file")

    # Set the path to pre-loaded wheels if it exists.
    if [ -n "$PYTHON_PRELOADED_WHEELS_DIR" ]; then
        resolve_cmd+=(--find-links "$PYTHON_PRELOADED_WHEELS_DIR")
    fi

    resolve_cmd+=("$requirements_file")

    if [[ $resolve_exit_code == 0 ]]; then
        "${resolve_cmd[@]}" > "$temp_dir/resolver.log" 2>&1 || resolve_exit_code=$?
    fi
    
    log_dependency_resolution_elapsed \
        "Oryx dependency resolution" \
        "$resolution_start_time"

    # If we fail to resolve the dependencies (non-zero exit code), then we fall back to 
    # pip install or uv pip install.
    if [[ $resolve_exit_code != 0 ]]; then
        echo "Oryx dependency resolution was unavailable because uv dependency resolution failed; deployment will continue using the original Oryx installation path."
        rm -rf -- "$temp_dir"
        install_python_packages "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag"
        return $?
    fi

    install_via_uv \
        "$python_cmd" \
        "$dependency_resolution_file" \
        "$target_dir" \
        "$upgrade_flag"
    local install_exit_code=$?
    if [[ $install_exit_code != 0 ]]; then
        echo "uv pip install failed with exit code ${install_exit_code}, falling back to pip install without dependency resolution artifacts..."
        rm -rf -- "$temp_dir"
        install_via_pip "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag"
        return $?
    fi

    # Publish only after uv successfully installs the compiled resolution.
    if [ -n "$dependency_resolution_output_dir" ]; then
        if publish_dependency_resolution \
            "$dependency_resolution_file" \
            "$dependency_resolution_output_dir" \
            "$python_cmd"; then
            echo "Oryx dependency resolution artifacts written to '$dependency_resolution_output_dir'."
        else
            echo "Oryx dependency resolution artifacts could not be written to '$dependency_resolution_output_dir'; deployment will continue."
        fi
    fi

    rm -rf -- "$temp_dir"
    return 0
}

# Internal function to install packages with uv and fallback to pip
install_python_packages_impl() {
    local python_cmd=$1
    local requirements_file=$2
    local target_dir=$3
    local upgrade_flag=$4
    
    set +e
    # Try uv first
    install_via_uv "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag"
    local exit_code=$?
    
    # Fallback to pip if uv fails
    if [[ $exit_code != 0 ]]; then
        echo "uv pip install failed with exit code ${exit_code}, falling back to pip install..."
        install_via_pip "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag"
        exit_code=$?
    fi
    set -e
    
    return $exit_code
}

install_python_packages() {
    local python_cmd=$1
    local requirements_file=$2
    local target_dir=$3
    local upgrade_flag=$4

    if [ "$PYTHON_FAST_BUILD_ENABLED" = "true" ]; then
        install_python_packages_impl "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag"
    else
        install_via_pip "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag"
    fi
}

{{ if VirtualEnvironmentName | IsNotBlank }}
    {{ if PackagesDirectory | IsNotBlank }}
        if [ -d "{{ PackagesDirectory }}" ]
        then
            rm -fr "{{ PackagesDirectory }}"
        fi
    {{ end }}

    VIRTUALENVIRONMENTNAME={{ VirtualEnvironmentName }}
    VIRTUALENVIRONMENTMODULE={{ VirtualEnvironmentModule }}
    VIRTUALENVIRONMENTOPTIONS="{{ VirtualEnvironmentParameters }}"
    zippedVirtualEnvFileName={{ CompressedVirtualEnvFileName }}

    echo "Python Virtual Environment: $VIRTUALENVIRONMENTNAME"

    if [ -e "pyproject.toml" ] && [ -e "uv.lock" ] && [ ! -e "$REQUIREMENTS_TXT_FILE" ]; then
        echo "Detected uv.lock (and no $REQUIREMENTS_TXT_FILE)"
        echo "Installing uv..."
        START_TIME=$SECONDS
        InstallUv="python -m pip install uv"
        printf %s " , $InstallUv" >> "$COMMAND_MANIFEST_FILE"
        $python -m pip install uv
        ELAPSED_TIME=$(($SECONDS - $START_TIME))
        echo "Installing uv done in $ELAPSED_TIME sec(s)."
        CreateVenvCommand="uv venv --link-mode=copy --system-site-packages $VIRTUALENVIRONMENTNAME"
        VIRTUALENVIRONMENTOPTIONS="$VIRTUALENVIRONMENTOPTIONS --system-site-packages"
    else
        if [ -e "$REQUIREMENTS_TXT_FILE" ]; then
            VIRTUALENVIRONMENTOPTIONS="$VIRTUALENVIRONMENTOPTIONS --system-site-packages"
        fi
        CreateVenvCommand="$python -m $VIRTUALENVIRONMENTMODULE $VIRTUALENVIRONMENTNAME $VIRTUALENVIRONMENTOPTIONS"
    fi

    if [ "$CreateVenvWithPythonVenv" = "true" ]; then
        if [[ "$CreateVenvCommand" == *"uv venv"* ]]; then
            echo "Using Python's built-in venv for virtual environment creation"
        fi
        CreateVenvCommand="$python -m $VIRTUALENVIRONMENTMODULE $VIRTUALENVIRONMENTNAME $VIRTUALENVIRONMENTOPTIONS"
    fi
    
    echo Creating virtual environment...

    echo "BuildCommands=$CreateVenvCommand" >> "$COMMAND_MANIFEST_FILE"
    
    # Execute the resolved CreateVenvCommand
    echo "Executing: $CreateVenvCommand"
    $CreateVenvCommand
    
    echo Activating virtual environment...
    printf %s " , $ActivateVenvCommand" >> "$COMMAND_MANIFEST_FILE"
    ActivateVenvCommand="source $VIRTUALENVIRONMENTNAME/bin/activate"
    source $VIRTUALENVIRONMENTNAME/bin/activate

    moreInformation="More information: https://aka.ms/troubleshoot-python"
    {{ if CustomBuildCommand | IsNotBlank }}
        echo
        echo "Running custom build command '{{ CustomBuildCommand }}'..."
        echo
        {{ CustomBuildCommand }}
    {{ else }}
        if [ -e "$REQUIREMENTS_TXT_FILE" ]
        then
            {{ if DependencyResolutionRequired }}
            if [ "$PYTHON_FAST_BUILD_ENABLED" = "true" ]; then
                echo "Fast build is enabled"
            fi
            set +e
            install_with_dependency_resolution "python" "$REQUIREMENTS_TXT_FILE" "" ""
            pipInstallExitCode=$?
            set -e
            if [[ $pipInstallExitCode != 0 ]]
            then
                if [ "$PYTHON_FAST_BUILD_ENABLED" = "true" ]; then
                    LogError "Package installation failed | Exit code: ${pipInstallExitCode} | Please review your requirements.txt | ${moreInformation}"
                else
                    LogError "${output} | Exit code: ${pipInstallExitCode} | Please review your requirements.txt | ${moreInformation}"
                fi
                exit $pipInstallExitCode
            fi
            {{ else }}
            if [ "$PYTHON_FAST_BUILD_ENABLED" = "true" ]; then
                set +e
                echo "Fast build is enabled"
                install_python_packages_impl "python" "$REQUIREMENTS_TXT_FILE" "" ""
                pipInstallExitCode=$?
                set -e
                if [[ $pipInstallExitCode != 0 ]]
                then
                    LogError "Package installation failed | Exit code: ${pipInstallExitCode} | Please review your requirements.txt | ${moreInformation}"
                    exit $pipInstallExitCode
                fi
            else
                set +e
                echo "Running pip install..."
                START_TIME=$SECONDS
                InstallCommand="python -m pip install --cache-dir $PIP_CACHE_DIR --prefer-binary -r $REQUIREMENTS_TXT_FILE"

                # Add find-links if PYTHON_PRELOADED_WHEELS_DIR is set
                if [ -n "$PYTHON_PRELOADED_WHEELS_DIR" ]; then
                    echo "Using preloaded wheels from: $PYTHON_PRELOADED_WHEELS_DIR"
                    InstallCommand="$InstallCommand --find-links=$PYTHON_PRELOADED_WHEELS_DIR"
                fi

                printf %s " , $InstallCommand | ts $TS_FMT" >> "$COMMAND_MANIFEST_FILE"
                output=$( ( $InstallCommand | ts $TS_FMT; exit ${PIPESTATUS[0]} ) 2>&1; exit ${PIPESTATUS[0]} )
                pipInstallExitCode=${PIPESTATUS[0]}

                ELAPSED_TIME=$(($SECONDS - $START_TIME))
                set -e
                echo "${output}"
                echo "pip install done in $ELAPSED_TIME sec(s)."
                if [[ $pipInstallExitCode != 0 ]]
                then
                    LogError "${output} | Exit code: ${pipInstallExitCode} | Please review your requirements.txt | ${moreInformation}"
                    exit $pipInstallExitCode
                fi
            fi
            {{ end }}
        elif [ -e "setup.py" ]
        then
            set +e
            echo "Running pip install setuptools..."
            START_TIME=$SECONDS
            InstallSetuptoolsPipCommand="pip install setuptools"
            printf %s " , $InstallSetuptoolsPipCommand" >> "$COMMAND_MANIFEST_FILE"
            pip install setuptools
            ELAPSED_TIME=$(($SECONDS - $START_TIME))
            echo "pip install setuptools done in $ELAPSED_TIME sec(s)."
            echo "Running python setup.py install..."
            START_TIME=$SECONDS
            InstallCommand="pip install . --cache-dir $PIP_CACHE_DIR --prefer-binary | ts $TS_FMT"
            printf %s " , $InstallCommand" >> "$COMMAND_MANIFEST_FILE"
            output=$( ( pip install . --cache-dir $PIP_CACHE_DIR --prefer-binary | ts $TS_FMT; exit ${PIPESTATUS[0]} ) 2>&1; exit ${PIPESTATUS[0]} )
            pythonBuildExitCode=${PIPESTATUS[0]}
            ELAPSED_TIME=$(($SECONDS - $START_TIME))
            set -e
            echo "${output}"
            echo "pip install done in $ELAPSED_TIME sec(s)."
            if [[ $pythonBuildExitCode != 0 ]]
            then
                LogError "${output} | Exit code: ${pipInstallExitCode} | Please review your setup.py | ${moreInformation}"
                exit $pythonBuildExitCode
            fi
        elif [ -e "pyproject.toml" ]
        then
            if [ -e "uv.lock" ];
            then
                # Install using uv
                set +e
                echo "Detected uv.lock. Installing dependencies with uv..."
                START_TIME=$SECONDS
                InstallUvCommand="uv sync --active --link-mode copy"
                printf %s " , $InstallUvCommand" >> "$COMMAND_MANIFEST_FILE"
                output=$( ( $InstallUvCommand; exit ${PIPESTATUS[0]} ) 2>&1 )
                uvExitCode=${PIPESTATUS[0]}
                ELAPSED_TIME=$(($SECONDS - $START_TIME))
                echo "uv sync done in $ELAPSED_TIME sec(s)."
                set -e
                echo "${output}"
                if [[ $uvExitCode != 0 ]]; then
                    LogError "${output} | Exit code: ${uvExitCode} | Please review your uv.lock | ${moreInformation}"
                    exit $uvExitCode
                fi
            else
                # Fallback to poetry

                set +e
                echo "Running pip install poetry..."
                START_TIME=$SECONDS
                InstallPipCommand="pip install poetry"
                printf %s " , $InstallPipCommand" >> "$COMMAND_MANIFEST_FILE"
                pip install poetry
                echo "Running poetry install..."

                # Try with --only main flag as --no-dev option is depreciated in latest poetry versions
                InstallPoetryCommand="poetry install --only main"
                printf %s " , $InstallPoetryCommand" >> "$COMMAND_MANIFEST_FILE"
                output=$( ( $InstallPoetryCommand; exit ${PIPESTATUS[0]} ) 2>&1)
                pythonBuildExitCode=${PIPESTATUS[0]}

                # Fallback to --no-dev flag
                if [[ $pythonBuildExitCode != 0 ]]; then
                    echo "poetry install failed with --only main flag, falling back to --no-dev"
                    pip install poetry==1.8.5
                    InstallPoetryCommand="poetry install --no-dev"
                    printf %s " , $InstallPoetryCommand" >> "$COMMAND_MANIFEST_FILE"
                    output=$( ( $InstallPoetryCommand; exit ${PIPESTATUS[0]} ) 2>&1)
                    pythonBuildExitCode=${PIPESTATUS[0]}
                    
                    # Final check after fallback
                    if [[ $pythonBuildExitCode != 0 ]]; then
                        set -e
                        echo "${output}"
                        LogWarning "${output} | Exit code: ${pythonBuildExitCode} | Please review message | ${moreInformation}"
                        exit $pythonBuildExitCode
                    fi
                fi

                ELAPSED_TIME=$(($SECONDS - $START_TIME))
                echo "poetry install done in $ELAPSED_TIME sec(s)."
                set -e
                echo "${output}"
            fi
        else
            echo $REQS_NOT_FOUND_MSG
        fi
    {{ end }}

    # For virtual environment, we use the actual 'python' alias that as setup by the venv,
    python_bin=python
{{ else }}
    moreInformation="More information: https://aka.ms/troubleshoot-python"
    {{ if CustomBuildCommand | IsNotBlank }}
        echo
        echo "Running custom build command '{{ CustomBuildCommand }}'..."
        echo
        {{ CustomBuildCommand }}
    {{ else }}
        if [ -e "$REQUIREMENTS_TXT_FILE" ]
        then
            {{ if DependencyResolutionRequired }}
            if [ "$PYTHON_FAST_BUILD_ENABLED" = "true" ]; then
                echo "Fast build is enabled"
            fi
            set +e
            install_with_dependency_resolution "$python" "$REQUIREMENTS_TXT_FILE" "{{ PackagesDirectory }}" "{{ PipUpgradeFlag }}"
            pipInstallExitCode=$?
            set -e
            if [[ $pipInstallExitCode != 0 ]]
            then
                if [ "$PYTHON_FAST_BUILD_ENABLED" = "true" ]; then
                    LogError "Package installation failed | Exit code: ${pipInstallExitCode} | Please review your requirements.txt | ${moreInformation}"
                else
                    LogError "${output} | Exit code: ${pipInstallExitCode} | Please review your requirements.txt | ${moreInformation}"
                fi
                exit $pipInstallExitCode
            fi
            {{ else }}
            if [ "$PYTHON_FAST_BUILD_ENABLED" = "true" ]; then
                set +e
                echo "Fast build is enabled"
                install_python_packages_impl "python" "$REQUIREMENTS_TXT_FILE" "" ""
                pipInstallExitCode=$?
                set -e
                if [[ $pipInstallExitCode != 0 ]]
                then
                    LogError "Package installation failed | Exit code: ${pipInstallExitCode} | Please review your requirements.txt | ${moreInformation}"
                    exit $pipInstallExitCode
                fi
            else
                set +e
                echo
                echo Running pip install...
                START_TIME=$SECONDS
                InstallCommand="$python -m pip install --cache-dir $PIP_CACHE_DIR --prefer-binary -r $REQUIREMENTS_TXT_FILE --target="{{ PackagesDirectory }}" {{ PipUpgradeFlag }}"

                # Add find-links if PYTHON_PRELOADED_WHEELS_DIR is set
                if [ -n "$PYTHON_PRELOADED_WHEELS_DIR" ]; then
                    echo "Using preloaded wheels from: $PYTHON_PRELOADED_WHEELS_DIR"
                    InstallCommand="$InstallCommand --find-links=$PYTHON_PRELOADED_WHEELS_DIR"
                fi

                printf %s " , $InstallCommand | ts $TS_FMT" >> "$COMMAND_MANIFEST_FILE"
                output=$( ( $InstallCommand | ts $TS_FMT; exit ${PIPESTATUS[0]} ) 2>&1; exit ${PIPESTATUS[0]} )
                pipInstallExitCode=${PIPESTATUS[0]}

                ELAPSED_TIME=$(($SECONDS - $START_TIME))
                set -e
                echo "${output}"
                echo "pip install done in $ELAPSED_TIME sec(s)."
                if [[ $pipInstallExitCode != 0 ]]
                then
                    LogError "${output} | Exit code: ${pipInstallExitCode} | Please review your requirements.txt | ${moreInformation}"
                    exit $pipInstallExitCode
                fi
            fi
            {{ end }}
        elif [ -e "setup.py" ]
        then
            echo
            START_TIME=$SECONDS
            UpgradeCommand="pip install --upgrade pip"
            printf %s " , $UpgradeCommand" >> "$COMMAND_MANIFEST_FILE"
            pip install --upgrade pip
            ELAPSED_TIME=$(($SECONDS - $START_TIME))
            echo "pip upgrade done in $ELAPSED_TIME sec(s)."

            set +e
            echo "Running pip install setuptools..."
            START_TIME=$SECONDS
            InstallSetuptoolsPipCommand="pip install setuptools"
            printf %s " , $InstallSetuptoolsPipCommand" >> "$COMMAND_MANIFEST_FILE"
            pip install setuptools
            ELAPSED_TIME=$(($SECONDS - $START_TIME))
            echo "pip install setuptools done in $ELAPSED_TIME sec(s)."
            echo "Running pip install..."
            START_TIME=$SECONDS
            InstallCommand="$python -m pip install . --cache-dir $PIP_CACHE_DIR --prefer-binary --target=\"{{ PackagesDirectory }}\" {{ PipUpgradeFlag }} | ts $TS_FMT"
            printf %s " , $InstallCommand" >> "$COMMAND_MANIFEST_FILE"
            output=$( ( $python -m pip install . --cache-dir $PIP_CACHE_DIR --prefer-binary --target="{{ PackagesDirectory }}" {{ PipUpgradeFlag }} | ts $TS_FMT; exit ${PIPESTATUS[0]} ) 2>&1; exit ${PIPESTATUS[0]} )
            pythonBuildExitCode=${PIPESTATUS[0]}
            ELAPSED_TIME=$(($SECONDS - $START_TIME))
            set -e
            echo "${output}"
            echo "pip install done in $ELAPSED_TIME sec(s)."
            if [[ $pythonBuildExitCode != 0 ]]
            then
                LogError "${output} | Exit code: ${pipInstallExitCode} | Please review your setup.py | ${moreInformation}"
                exit $pythonBuildExitCode
            fi
        elif [ -e "pyproject.toml" ]
        then
            if [ -e "uv.lock" ];
            then
                # Install using uv
                echo "Detected uv.lock. Installing dependencies with uv..."
                START_TIME=$SECONDS
                echo "Installing uv..."
                InstallUv="python -m pip install uv"
                printf %s " , $InstallUv" >> "$COMMAND_MANIFEST_FILE"
                $python -m pip install uv
                ELAPSED_TIME=$(($SECONDS - $START_TIME))
                echo "Installing uv done in $ELAPSED_TIME sec(s)."
                START_TIME=$SECONDS
                
                set +e
                SITE_PACKAGES_PATH="{{ PackagesDirectory }}"
                echo "Installing dependencies..."
                # Stream the export directly into uv pip install using process substitution
                InstallUvCommand="uv export --locked | uv pip install --link-mode copy --target $SITE_PACKAGES_PATH -r -"
                printf %s " , $InstallUvCommand" >> "$COMMAND_MANIFEST_FILE"
                output=$( ( eval $InstallUvCommand; exit ${PIPESTATUS[0]} ) 2>&1 )
                uvExitCode=${PIPESTATUS[0]}
                ELAPSED_TIME=$(($SECONDS - $START_TIME))
                set -e
                echo "${output}"
                echo "uv pip install done in $ELAPSED_TIME sec(s)."
                if [[ $uvExitCode != 0 ]]; then
                    LogError "${output} | Exit code: ${uvExitCode} | Please review your uv.lock | ${moreInformation}"
                    exit $uvExitCode
                fi
            else
                # Fallback to poetry

                set +e
                echo "Running pip install poetry..."
                START_TIME=$SECONDS
                InstallPipCommand="pip install poetry"
                printf %s " , $InstallPipCommand" >> "$COMMAND_MANIFEST_FILE"
                pip install poetry
                echo "Running poetry install..."

                # Try with --only main flag as --no-dev option is depreciated in latest poetry versions
                InstallPoetryCommand="poetry install --only main"
                printf %s " , $InstallPoetryCommand" >> "$COMMAND_MANIFEST_FILE"
                output=$( ( $InstallPoetryCommand; exit ${PIPESTATUS[0]} ) 2>&1)
                pythonBuildExitCode=${PIPESTATUS[0]}

                # Fallback to --no-dev flag
                if [[ $pythonBuildExitCode != 0 ]]; then
                    echo "poetry install failed with --only main flag, falling back to --no-dev"
                    pip install poetry==1.8.5
                    InstallPoetryCommand="poetry install --no-dev"
                    printf %s " , $InstallPoetryCommand" >> "$COMMAND_MANIFEST_FILE"
                    output=$( ( $InstallPoetryCommand; exit ${PIPESTATUS[0]} ) 2>&1)
                    pythonBuildExitCode=${PIPESTATUS[0]}
                    
                    # Final check after fallback
                    if [[ $pythonBuildExitCode != 0 ]]; then
                        set -e
                        echo "${output}"
                        LogWarning "${output} | Exit code: ${pythonBuildExitCode} | Please review message | ${moreInformation}"
                        exit $pythonBuildExitCode
                    fi
                fi

                ELAPSED_TIME=$(($SECONDS - $START_TIME))
                echo "poetry install done in $ELAPSED_TIME sec(s)."
                set -e
                echo "${output}"
            fi
        else
            echo $REQS_NOT_FOUND_MSG
        fi
    {{ end }}

    # We need to use the python binary selected by benv
    python_bin=$python

    # Detect the location of the site-packages to add the .pth file
    # For the local site package, only major and minor versions are provided, so we fetch it again
    SITE_PACKAGE_PYTHON_VERSION=$($python -c "import sys; print(str(sys.version_info.major) + '.' + str(sys.version_info.minor))")
    SITE_PACKAGES_PATH=$PIP_CACHE_DIR"/lib/python"$SITE_PACKAGE_PYTHON_VERSION"/site-packages"
    mkdir -p $SITE_PACKAGES_PATH
    # To make sure the packages are available later, e.g. for collect static or post-build hooks, we add a .pth pointing to them
    APP_PACKAGES_PATH=$(pwd)"/{{ PackagesDirectory }}"
    echo $APP_PACKAGES_PATH > $SITE_PACKAGES_PATH"/oryx.pth"
{{ end }}

{{ if RunPythonPackageCommand }}
    echo
    echo "Running python packaging commands ...."
    echo
    echo "Determining python package wheel ...."

    {{ if PythonPackageWheelProperty }}
        echo "Creating universal package wheel ...."
    {{ end }}

    PackageWheelCommand=""

    if [ -z "{{ PythonPackageWheelProperty }}" ]
    then
        echo "Creating non universal package wheel ...."
        PackageWheelCommand="$python setup.py sdist --formats=gztar,zip,tar bdist_wheel"
        $python setup.py sdist --formats=gztar,zip,tar bdist_wheel
    else
        PackageWheelCommand="$python setup.py sdist --formats=gztar,zip,tar bdist_wheel --universal"
        $python setup.py sdist --formats=gztar,zip,tar bdist_wheel --universal
    fi

    PackageEggCommand="$python setup.py bdist_egg"

    echo "Now creating python package egg ...."
    printf %s " , $PackageWheelCommand, $PackageEggCommand" >> "$COMMAND_MANIFEST_FILE"
    $python setup.py bdist_egg
    echo
{{ end }}


{{ if EnableCollectStatic }}
    set +e
    if [ -e "$SOURCE_DIR/manage.py" ]
    then
        if grep -iq "Django" "$SOURCE_DIR/$REQUIREMENTS_TXT_FILE"
        then
            echo
            echo Content in source directory is a Django app
            echo Running 'collectstatic'...
            START_TIME=$SECONDS
            CollectStaticCommand="$python_bin manage.py collectstatic --noinput"
            printf %s " , $CollectStaticCommand" >> "$COMMAND_MANIFEST_FILE"
            output=$(($python_bin manage.py collectstatic --noinput; exit ${PIPESTATUS[0]}) 2>&1)
            EXIT_CODE=${PIPESTATUS[0]}
            echo "${output}"
            if [[ $EXIT_CODE != 0 ]]
            then
                recommendation="Please review message"
                LogWarning "${output} | Exit code: ${EXIT_CODE} | ${recommendation} | ${moreInformation}"
            fi
            ELAPSED_TIME=$(($SECONDS - $START_TIME))
            echo "collectstatic done in $ELAPSED_TIME sec(s)."
        else
            output="Missing Django module in $SOURCE_DIR/$REQUIREMENTS_TXT_FILE"
            recommendation="Add Django to your requirements.txt file."
            LogWarning "${output} | Exit code: 0 | ${recommendation} | ${moreInformation}"
        fi
    fi
    set -e
{{ end }}


ReadImageType=$(cat /opt/oryx/.imagetype)

if [ "$ReadImageType" = "vso-focal" ] || [ "$ReadImageType" = "vso-debian-bullseye" ]
then
    echo $ReadImageType
    cat "$COMMAND_MANIFEST_FILE"
else
    echo "Not a vso image, so not writing build commands"
    rm "$COMMAND_MANIFEST_FILE"
fi

# Copy requirements.txt to the destination directory if it exists
if [ "$SOURCE_DIR" != "$DESTINATION_DIR" ]
then
    if [ -e "$SOURCE_DIR/$REQUIREMENTS_TXT_FILE" ]
    then
        echo
        echo "Copying '$REQUIREMENTS_TXT_FILE' to destination directory..."
        cp "$SOURCE_DIR/$REQUIREMENTS_TXT_FILE" "$DESTINATION_DIR/requirements.txt" || true
        echo "Done copying requirements.txt to destination directory."
    fi
fi

{{ if VirtualEnvironmentName | IsNotBlank }}
    {{ if CompressVirtualEnvCommand | IsNotBlank }}
        if [ "$SOURCE_DIR" != "$DESTINATION_DIR" ]
        then
            if [ -d "$VIRTUALENVIRONMENTNAME" ]
            then
                echo
                echo "Compressing existing '$VIRTUALENVIRONMENTNAME' folder..."
                START_TIME=$SECONDS
                # Make the contents of the virtual env folder appear in the zip file, not the folder itself
                cd "$VIRTUALENVIRONMENTNAME"
                {{ CompressVirtualEnvCommand }} ../$zippedVirtualEnvFileName .
                ELAPSED_TIME=$(($SECONDS - $START_TIME))
                echo "Compressing virtual environment done in $ELAPSED_TIME sec(s)."
            fi
        fi
    {{ end }}
{{ end }}
