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
    local constraints_file=$5
    local write_manifest=${6:-true}
    
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
    if [ -n "$constraints_file" ]; then
        base_cmd="$base_cmd -c $constraints_file"
    fi
    
    if [ -n "$target_dir" ]; then
        base_cmd="$base_cmd --target=\"$target_dir\""
    fi
    if [ -n "$upgrade_flag" ]; then
        base_cmd="$base_cmd $upgrade_flag"
    fi
    
    # Log the command
    local uv_cmd="$base_cmd | ts $TS_FMT"
    if [ "$write_manifest" = "true" ]; then
        printf %s " , $uv_cmd" >> "$COMMAND_MANIFEST_FILE"
    fi
    
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
    local constraints_file=$5
    local write_manifest=${6:-true}
    
    set +e
    echo "Running pip install..."
    
    # Build the command
    local base_cmd="$python_cmd -m pip install --cache-dir $PIP_CACHE_DIR --prefer-binary -r $requirements_file"
    if [ -n "$constraints_file" ]; then
        base_cmd="$base_cmd -c $constraints_file"
    fi
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
    if [ "$write_manifest" = "true" ]; then
        printf %s " , $pip_cmd" >> "$COMMAND_MANIFEST_FILE"
    fi
    
    # Execute pip install
    output=$( ( $base_cmd | ts $TS_FMT; exit ${PIPESTATUS[0]} ) 2>&1; exit ${PIPESTATUS[0]} )
    local exit_code=${PIPESTATUS[0]}
    echo "${output}"
    ELAPSED_TIME=$(($SECONDS - $START_TIME))
    echo "pip install done in $ELAPSED_TIME sec(s)."
    return $exit_code
}

log_oryx_secure_build_unavailable() {
    echo "Oryx SecureBuild audit: Assessment was unavailable because $1; deployment will continue using the original Oryx installation path."
}

log_oryx_secure_build_elapsed() {
    local operation=$1
    local start_time=$2
    local elapsed_time=$(($SECONDS - $start_time))
    echo "$operation done in $elapsed_time sec(s)."
}

log_secure_build_unavailable_and_cleanup_temp_dir() {
    local reason=$1
    local secure_build_start_time=$2
    local temp_dir=$3

    log_oryx_secure_build_unavailable "$reason"
    if [ -n "$temp_dir" ]; then
        rm -rf -- "$temp_dir"
    fi
    log_oryx_secure_build_elapsed "Oryx SecureBuild" "$secure_build_start_time"
}

publish_dependency_resolution() {
    local manager=$1
    local source_file=$2
    local output_dir=$3
    local python_cmd=$4
    local resolution_file_name

    if [ "$manager" = "pip" ]; then
        resolution_file_name="dependency-resolution.json"
    elif [ "$manager" = "uv" ]; then
        resolution_file_name="dependency-resolution.txt"
    else
        return 1
    fi

    if ! (umask 077 && mkdir -p -- "$output_dir"); then
        return 1
    fi

    (
        umask 077

        local metadata_file_name="dependency-resolution-metadata.json"
        local staging_suffix=".$$.${RANDOM}.tmp"
        local staged_resolution_file="$output_dir/$resolution_file_name$staging_suffix"
        local staged_metadata_file="$output_dir/$metadata_file_name$staging_suffix"
        local resolution_file="$output_dir/$resolution_file_name"
        local metadata_file="$output_dir/$metadata_file_name"

        rm -f -- "$metadata_file"
        cp -- "$source_file" "$staged_resolution_file" || exit $?
        "$python_cmd" -c \
            'import json, sys; json.dump({"schemaVersion": 1, "manager": sys.argv[1], "dependencyResolutionFilePath": sys.argv[2]}, sys.stdout, indent=2); print()' \
            "$manager" \
            "$resolution_file" \
            > "$staged_metadata_file" || {
                rm -f -- "$staged_resolution_file"
                exit 1
            }

        mv -f -- "$staged_resolution_file" "$resolution_file" || {
            rm -f -- "$staged_metadata_file"
            exit 1
        }
        mv -f -- "$staged_metadata_file" "$metadata_file" || {
            rm -f -- "$resolution_file" "$staged_metadata_file"
            exit 1
        }

        if [ "$manager" = "pip" ]; then
            rm -f -- "$output_dir/dependency-resolution.txt"
        else
            rm -f -- "$output_dir/dependency-resolution.json"
        fi
    )
}

install_with_dependency_resolution() {
    local manager=$1
    local python_cmd=$2
    local requirements_file=$3
    local target_dir=$4
    local upgrade_flag=$5
    local temp_dir=""
    local secure_build_start_time=$SECONDS
    local secure_build_enabled="{{ OryxSecureBuildEnabled }}"
    local dependency_resolution_output_dir={{ DependencyResolutionOutputDirBashValue }}

    temp_dir=$(mktemp -d 2> /dev/null)
    if [ -z "$temp_dir" ]; then
        echo "Oryx dependency resolution was unavailable because a temporary directory could not be created; deployment will continue using the original Oryx installation path."
        install_python_packages "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag"
        return $?
    fi

    local dependency_resolution_file="$temp_dir/dependency-resolution"
    local secure_build_constraints_file="$temp_dir/secure-build-constraints.txt"
    local resolve_cmd
    local resolve_exit_code=0
    local resolution_start_time=$SECONDS

    # Resolve exact direct and transitive versions for capture and optional assessment.
    if [ "$manager" = "uv" ]; then
        ensure_uv "$python_cmd" || resolve_exit_code=$?
        resolve_cmd=(uv pip compile --python "$python_cmd" --cache-dir "$UV_PIP_CACHE_DIR" --no-header --no-annotate --output-file "$dependency_resolution_file")
    else
        resolve_cmd=("$python_cmd" -m pip install --dry-run --ignore-installed --report "$dependency_resolution_file" --cache-dir "$PIP_CACHE_DIR" --prefer-binary)
        if [ -n "$target_dir" ]; then
            resolve_cmd+=(--target "$target_dir")
        fi
        if [ -n "$upgrade_flag" ]; then
            resolve_cmd+=("$upgrade_flag")
        fi
    fi
    if [ -n "$PYTHON_PRELOADED_WHEELS_DIR" ]; then
        resolve_cmd+=(--find-links "$PYTHON_PRELOADED_WHEELS_DIR")
    fi
    if [ "$manager" = "pip" ]; then
        resolve_cmd+=(-r "$requirements_file")
    else
        resolve_cmd+=("$requirements_file")
    fi
    if [[ $resolve_exit_code == 0 ]]; then
        "${resolve_cmd[@]}" > "$temp_dir/resolver.log" 2>&1 || resolve_exit_code=$?
    fi
    log_oryx_secure_build_elapsed \
        "Oryx dependency resolution" \
        "$resolution_start_time"
    if [[ $resolve_exit_code != 0 ]]; then
        echo "Oryx dependency resolution was unavailable because $manager dependency resolution failed; deployment will continue using the original Oryx installation path."
        rm -rf -- "$temp_dir"
        install_python_packages "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag"
        return $?
    fi

    if [ -n "$dependency_resolution_output_dir" ]; then
        if publish_dependency_resolution \
            "$manager" \
            "$dependency_resolution_file" \
            "$dependency_resolution_output_dir" \
            "$python_cmd"; then
            echo "Oryx dependency resolution artifacts written to '$dependency_resolution_output_dir'."
        else
            echo "Oryx dependency resolution artifacts could not be written to '$dependency_resolution_output_dir'; deployment will continue."
        fi
    fi

    if [ "$secure_build_enabled" != "true" ]; then
        rm -rf -- "$temp_dir"
        install_python_packages "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag"
        return $?
    fi

    if ! command -v oryx-secure-build-checker > /dev/null 2>&1; then
        log_secure_build_unavailable_and_cleanup_temp_dir \
            "oryx-secure-build-checker is not installed" \
            "$secure_build_start_time" \
            "$temp_dir"
        install_python_packages "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag"
        return $?
    fi
    if ! command -v timeout > /dev/null 2>&1; then
        log_secure_build_unavailable_and_cleanup_temp_dir \
            "the timeout command is not installed" \
            "$secure_build_start_time" \
            "$temp_dir"
        install_python_packages "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag"
        return $?
    fi

    local assessment_exit_code=0
    local assessment_start_time=$SECONDS
    local assessment_output_file="$temp_dir/security-assessment.json"
    timeout --signal=TERM --kill-after=10s \
        "{{ OryxSecureBuildCheckerTimeoutInMinutes }}m" \
        oryx-secure-build-checker \
        --manager "$manager" \
        --dependency-resolution-file "$dependency_resolution_file" \
        --assessment-output "$assessment_output_file" \
        --frozen-packages-output "$secure_build_constraints_file" \
        --mode "{{ OryxSecureBuildMode }}" || assessment_exit_code=$?
    log_oryx_secure_build_elapsed \
        "Oryx SecureBuild assessment" \
        "$assessment_start_time"

    case "$assessment_exit_code" in
        0)
            if [ ! -f "$secure_build_constraints_file" ]; then
                log_secure_build_unavailable_and_cleanup_temp_dir \
                    "oryx-secure-build-checker did not produce frozen packages" \
                    "$secure_build_start_time" \
                    "$temp_dir"
                install_python_packages "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag"
                return $?
            fi
            ;;
        42)
            # Exit code 42 is the only checker result that blocks deployment.
            rm -rf -- "$temp_dir"
            log_oryx_secure_build_elapsed "Oryx SecureBuild" "$secure_build_start_time"
            return 42
            ;;
        124|137)
            log_secure_build_unavailable_and_cleanup_temp_dir \
                "oryx-secure-build-checker exceeded the {{ OryxSecureBuildCheckerTimeoutInMinutes }}-minute time limit" \
                "$secure_build_start_time" \
                "$temp_dir"
            install_python_packages "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag"
            return $?
            ;;
        *)
            log_secure_build_unavailable_and_cleanup_temp_dir \
                "oryx-secure-build-checker could not complete the assessment" \
                "$secure_build_start_time" \
                "$temp_dir"
            install_python_packages "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag"
            return $?
            ;;
    esac

    # Constrain installation to the exact versions that passed assessment.
    local install_exit_code=0
    if [ "$manager" = "uv" ]; then
        install_python_packages_impl "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag" "$secure_build_constraints_file" "false" || install_exit_code=$?
    else
        install_via_pip "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag" "$secure_build_constraints_file" "false" || install_exit_code=$?
    fi
    rm -rf -- "$temp_dir"
    log_oryx_secure_build_elapsed "Oryx SecureBuild" "$secure_build_start_time"
    return $install_exit_code
}

# Internal function to install packages with uv and fallback to pip
install_python_packages_impl() {
    local python_cmd=$1
    local requirements_file=$2
    local target_dir=$3
    local upgrade_flag=$4
    local constraints_file=$5
    local write_manifest=${6:-true}
    
    set +e
    # Try uv first
    install_via_uv "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag" "$constraints_file" "$write_manifest"
    local exit_code=$?
    
    # Fallback to pip if uv fails
    if [[ $exit_code != 0 ]]; then
        echo "uv pip install failed with exit code ${exit_code}, falling back to pip install..."
        install_via_pip "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag" "$constraints_file" "$write_manifest"
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
        install_python_packages_impl "$python_cmd" "$requirements_file" "" ""
    else
        install_via_pip "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag" ""
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
            secure_build_manager="pip"
            if [ "$PYTHON_FAST_BUILD_ENABLED" = "true" ]; then
                secure_build_manager="uv"
            fi
            install_with_dependency_resolution "$secure_build_manager" "python" "$REQUIREMENTS_TXT_FILE" "" ""
            pipInstallExitCode=$?
            set -e
            if [[ $pipInstallExitCode != 0 ]]
            then
                if [[ $pipInstallExitCode == 42 ]]; then
                    LogError "Oryx SecureBuild blocked the deployment because critical vulnerabilities were found in the resolved Python packages | Exit code: ${pipInstallExitCode} | Review the Oryx SecureBuild assessment above and update your requirements.txt"
                elif [ "$PYTHON_FAST_BUILD_ENABLED" = "true" ]; then
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
            secure_build_manager="pip"
            if [ "$PYTHON_FAST_BUILD_ENABLED" = "true" ]; then
                secure_build_manager="uv"
            fi
            install_with_dependency_resolution "$secure_build_manager" "$python" "$REQUIREMENTS_TXT_FILE" "{{ PackagesDirectory }}" "{{ PipUpgradeFlag }}"
            pipInstallExitCode=$?
            set -e
            if [[ $pipInstallExitCode != 0 ]]
            then
                if [[ $pipInstallExitCode == 42 ]]; then
                    LogError "Oryx SecureBuild blocked the deployment because critical vulnerabilities were found in the resolved Python packages | Exit code: ${pipInstallExitCode} | Review the Oryx SecureBuild assessment above and update your requirements.txt"
                elif [ "$PYTHON_FAST_BUILD_ENABLED" = "true" ]; then
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
