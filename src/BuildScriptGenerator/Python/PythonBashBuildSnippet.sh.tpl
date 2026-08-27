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
    local ensure_uv_exit_code=$?
    if [[ $ensure_uv_exit_code != 0 ]]; then
        return $ensure_uv_exit_code
    fi
    
    set +e
    echo "Running uv pip install..."
    
    # Build the command as an array so paths are not reinterpreted by the shell.
    local base_cmd=(uv pip install --cache-dir "$UV_PIP_CACHE_DIR" --compile-bytecode)
    
    # Add find-links if PYTHON_PRELOADED_WHEELS_DIR is set
    if [ -n "$PYTHON_PRELOADED_WHEELS_DIR" ]; then
        echo "Using preloaded wheels from: $PYTHON_PRELOADED_WHEELS_DIR"
        base_cmd+=(--find-links "$PYTHON_PRELOADED_WHEELS_DIR")
    fi
    
    base_cmd+=(-r "$requirements_file")
    if [ -n "$constraints_file" ]; then
        base_cmd+=(-c "$constraints_file")
    fi
    
    if [ -n "$target_dir" ]; then
        base_cmd+=(--target "$target_dir")
    fi
    if [ -n "$upgrade_flag" ]; then
        base_cmd+=("$upgrade_flag")
    fi
    
    # Log the command
    local uv_cmd
    printf -v uv_cmd '%q ' "${base_cmd[@]}"
    uv_cmd="${uv_cmd}| ts $TS_FMT"
    if [ "$write_manifest" = "true" ]; then
        printf %s " , $uv_cmd" >> "$COMMAND_MANIFEST_FILE"
    fi
    
    # Execute uv pip install (uv manages its own cache)
    output=$( ( "${base_cmd[@]}" | ts $TS_FMT; exit ${PIPESTATUS[0]} ) 2>&1; exit ${PIPESTATUS[0]} )
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
    local base_cmd=("$python_cmd" -m pip install --cache-dir "$PIP_CACHE_DIR" --prefer-binary -r "$requirements_file")
    if [ -n "$constraints_file" ]; then
        base_cmd+=(-c "$constraints_file")
    fi
    if [ -n "$target_dir" ]; then
        base_cmd+=(--target "$target_dir")
    fi
    if [ -n "$upgrade_flag" ]; then
        base_cmd+=("$upgrade_flag")
    fi

    # Add find-links if PYTHON_PRELOADED_WHEELS_DIR is set
    if [ -n "$PYTHON_PRELOADED_WHEELS_DIR" ]; then
        echo "Using preloaded wheels from: $PYTHON_PRELOADED_WHEELS_DIR"
        base_cmd+=(--find-links "$PYTHON_PRELOADED_WHEELS_DIR")
    fi
    
    # Log the command
    local pip_cmd
    printf -v pip_cmd '%q ' "${base_cmd[@]}"
    pip_cmd="${pip_cmd}| ts $TS_FMT"
    if [ "$write_manifest" = "true" ]; then
        printf %s " , $pip_cmd" >> "$COMMAND_MANIFEST_FILE"
    fi
    
    # Execute pip install
    output=$( ( "${base_cmd[@]}" | ts $TS_FMT; exit ${PIPESTATUS[0]} ) 2>&1; exit ${PIPESTATUS[0]} )
    local exit_code=${PIPESTATUS[0]}
    echo "${output}"
    ELAPSED_TIME=$(($SECONDS - $START_TIME))
    echo "pip install done in $ELAPSED_TIME sec(s)."
    return $exit_code
}

{{ if OryxSafeBuildEnabled }}
SAFE_ORYX_BUILD_CHECKED=false

oryx_safe_build_unavailable() {
    echo "Oryx SafeBuild audit: Assessment was unavailable because $1; deployment will continue using the original Oryx installation path."
}

oryx_safe_build_log_elapsed() {
    local operation=$1
    local start_time=$2
    local elapsed_time=$(($SECONDS - $start_time))
    echo "$operation done in $elapsed_time sec(s)."
}

install_with_oryx_safe_build() {
    local manager=$1
    local python_cmd=$2
    local requirements_file=$3
    local target_dir=$4
    local upgrade_flag=$5
    local temp_dir
    local safe_build_start_time=$SECONDS

    SAFE_ORYX_BUILD_CHECKED=false

    if ! command -v oryx-safe-build-checker > /dev/null 2>&1; then
        oryx_safe_build_unavailable "oryx-safe-build-checker is not installed"
        oryx_safe_build_log_elapsed "Oryx SafeBuild" "$safe_build_start_time"
        return 75
    fi
    if ! command -v timeout > /dev/null 2>&1; then
        oryx_safe_build_unavailable "the timeout command is not installed"
        oryx_safe_build_log_elapsed "Oryx SafeBuild" "$safe_build_start_time"
        return 75
    fi

    temp_dir=$(mktemp -d 2> /dev/null)
    if [ -z "$temp_dir" ]; then
        oryx_safe_build_unavailable "a temporary directory could not be created"
        oryx_safe_build_log_elapsed "Oryx SafeBuild" "$safe_build_start_time"
        return 75
    fi

    local resolver_output_file="$temp_dir/resolver-output"
    local frozen_packages_file="$temp_dir/frozen-packages.txt"
    local resolve_cmd
    local resolve_exit_code=0
    local resolution_start_time=$SECONDS
    if [ "$manager" = "uv" ]; then
        ensure_uv "$python_cmd" || resolve_exit_code=$?
        resolve_cmd=(uv pip compile --python "$python_cmd" --cache-dir "$UV_PIP_CACHE_DIR" --no-header --no-annotate --output-file "$resolver_output_file")
    else
        resolve_cmd=("$python_cmd" -m pip install --dry-run --ignore-installed --report "$resolver_output_file" --cache-dir "$PIP_CACHE_DIR" --prefer-binary)
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
    oryx_safe_build_log_elapsed \
        "Oryx SafeBuild dependency resolution" \
        "$resolution_start_time"
    if [[ $resolve_exit_code != 0 ]]; then
        oryx_safe_build_unavailable "$manager dependency resolution failed"
        rm -rf -- "$temp_dir"
        oryx_safe_build_log_elapsed "Oryx SafeBuild" "$safe_build_start_time"
        return 75
    fi

    local assessment_exit_code=0
    local assessment_start_time=$SECONDS
    timeout --signal=TERM --kill-after=10s \
        "{{ OryxSafeBuildCheckerTimeoutInMinutes }}m" \
        oryx-safe-build-checker \
        --manager "$manager" \
        --resolver-output "$resolver_output_file" \
        --frozen-packages "$frozen_packages_file" \
        --mode "{{ OryxSafeBuildMode }}" || assessment_exit_code=$?
    oryx_safe_build_log_elapsed \
        "Oryx SafeBuild assessment" \
        "$assessment_start_time"

    if [[ $assessment_exit_code == 124 || $assessment_exit_code == 137 ]]; then
        oryx_safe_build_unavailable \
            "oryx-safe-build-checker exceeded the {{ OryxSafeBuildCheckerTimeoutInMinutes }}-minute time limit"
        rm -rf -- "$temp_dir"
        oryx_safe_build_log_elapsed "Oryx SafeBuild" "$safe_build_start_time"
        return 75
    elif [[ $assessment_exit_code == 42 ]]; then
        SAFE_ORYX_BUILD_CHECKED=true
        rm -rf -- "$temp_dir"
        oryx_safe_build_log_elapsed "Oryx SafeBuild" "$safe_build_start_time"
        return 42
    elif [[ $assessment_exit_code != 0 ]]; then
        oryx_safe_build_unavailable "oryx-safe-build-checker could not complete the assessment"
        rm -rf -- "$temp_dir"
        oryx_safe_build_log_elapsed "Oryx SafeBuild" "$safe_build_start_time"
        return 75
    elif [ ! -f "$frozen_packages_file" ]; then
        oryx_safe_build_unavailable "oryx-safe-build-checker did not produce frozen packages"
        rm -rf -- "$temp_dir"
        oryx_safe_build_log_elapsed "Oryx SafeBuild" "$safe_build_start_time"
        return 75
    fi

    SAFE_ORYX_BUILD_CHECKED=true
    local install_exit_code=0
    if [ "$manager" = "uv" ]; then
        install_via_uv "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag" "$frozen_packages_file" "false" || install_exit_code=$?
    else
        install_via_pip "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag" "$frozen_packages_file" "false" || install_exit_code=$?
    fi
    rm -rf -- "$temp_dir"
    oryx_safe_build_log_elapsed "Oryx SafeBuild" "$safe_build_start_time"
    return $install_exit_code
}
{{ end }}

# Internal function to install packages with uv and fallback to pip
install_python_packages_impl() {
    local python_cmd=$1
    local requirements_file=$2
    local target_dir=$3
    local upgrade_flag=$4
    
    set +e
    # Try uv first
    install_via_uv "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag" ""
    local exit_code=$?
    
    # Fallback to pip if uv fails
    if [[ $exit_code != 0 ]]; then
        echo "uv pip install failed with exit code ${exit_code}, falling back to pip install..."
        install_via_pip "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag" ""
        exit_code=$?
    fi
    set -e
    
    return $exit_code
}

install_python_requirements() {
    local python_cmd=$1
    local requirements_file=$2
    local target_dir=$3
    local upgrade_flag=$4

{{ if OryxSafeBuildEnabled }}
    local manager="pip"
    if [ "$PYTHON_FAST_BUILD_ENABLED" = "true" ]; then
        manager="uv"
    fi

    install_with_oryx_safe_build "$manager" "$python_cmd" "$requirements_file" "$target_dir" "$upgrade_flag"
    local safe_build_exit_code=$?
    if [ "$SAFE_ORYX_BUILD_CHECKED" = "true" ]; then
        return $safe_build_exit_code
    fi
{{ end }}

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
            if [ "$PYTHON_FAST_BUILD_ENABLED" = "true" ]; then
                echo "Fast build is enabled"
            fi
            set +e
            install_python_requirements "python" "$REQUIREMENTS_TXT_FILE" "" ""
            pipInstallExitCode=$?
            set -e
            if [[ $pipInstallExitCode != 0 ]]
            then
                {{ if OryxSafeBuildEnabled }}
                if [[ $pipInstallExitCode == 42 ]]; then
                    LogError "Oryx SafeBuild blocked the deployment because critical vulnerabilities were found in the resolved Python packages | Exit code: ${pipInstallExitCode} | Review the Oryx SafeBuild assessment above and update your requirements.txt"
                elif [ "$PYTHON_FAST_BUILD_ENABLED" = "true" ]; then
                {{ else }}
                if [ "$PYTHON_FAST_BUILD_ENABLED" = "true" ]; then
                {{ end }}
                    LogError "Package installation failed | Exit code: ${pipInstallExitCode} | Please review your requirements.txt | ${moreInformation}"
                else
                    LogError "${output} | Exit code: ${pipInstallExitCode} | Please review your requirements.txt | ${moreInformation}"
                fi
                exit $pipInstallExitCode
            fi
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
            if [ "$PYTHON_FAST_BUILD_ENABLED" = "true" ]; then
                echo "Fast build is enabled"
            fi
            set +e
            install_python_requirements "$python" "$REQUIREMENTS_TXT_FILE" "{{ PackagesDirectory }}" "{{ PipUpgradeFlag }}"
            pipInstallExitCode=$?
            set -e
            if [[ $pipInstallExitCode != 0 ]]
            then
                {{ if OryxSafeBuildEnabled }}
                if [[ $pipInstallExitCode == 42 ]]; then
                    LogError "Oryx SafeBuild blocked the deployment because critical vulnerabilities were found in the resolved Python packages | Exit code: ${pipInstallExitCode} | Review the Oryx SafeBuild assessment above and update your requirements.txt"
                elif [ "$PYTHON_FAST_BUILD_ENABLED" = "true" ]; then
                {{ else }}
                if [ "$PYTHON_FAST_BUILD_ENABLED" = "true" ]; then
                {{ end }}
                    LogError "Package installation failed | Exit code: ${pipInstallExitCode} | Please review your requirements.txt | ${moreInformation}"
                else
                    LogError "${output} | Exit code: ${pipInstallExitCode} | Please review your requirements.txt | ${moreInformation}"
                fi
                exit $pipInstallExitCode
            fi
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
