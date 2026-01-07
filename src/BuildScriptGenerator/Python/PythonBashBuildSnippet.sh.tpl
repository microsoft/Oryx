set -e
# TODO: refactor redundant code. Work-item: 1476457

declare -r TS_FMT='[%T%z] '
declare -r REQS_NOT_FOUND_MSG='Could not find setup.py or requirements.txt; Not running pip install. More information: https://aka.ms/requirements-not-found'
echo "Python Version: $python"
PIP_CACHE_DIR=/usr/local/share/pip-cache

{{ if PythonBuildCommandsFileName | IsNotBlank }}
COMMAND_MANIFEST_FILE="{{ PythonBuildCommandsFileName }}"
{{ end }}

echo "Creating directory for command manifest file if it does not exist"
mkdir -p "$(dirname "$COMMAND_MANIFEST_FILE")"
echo "Removing existing manifest file"
rm -f "$COMMAND_MANIFEST_FILE"

echo "PlatformWithVersion=Python {{ PythonVersion }}" > "$COMMAND_MANIFEST_FILE"

InstallCommand=""

if [ ! -d "$PIP_CACHE_DIR" ];then
    mkdir -p $PIP_CACHE_DIR
fi

{{ if CustomRequirementsTxtPath | IsNotBlank }}
    REQUIREMENTS_TXT_FILE="{{ CustomRequirementsTxtPath }}"
{{ else }}
    REQUIREMENTS_TXT_FILE="requirements.txt"
{{ end }}

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
        echo "Detected uv.lock (and no $REQUIREMENTS_TXT_FILE); creating virtual environment with uv..."
        echo "Installing uv..."
        InstallUv="python -m pip install uv"
        printf %s " , $InstallUv" >> "$COMMAND_MANIFEST_FILE"
        $python -m pip install uv
        CreateVenvCommand="uv venv --link-mode=copy --system-site-packages $VIRTUALENVIRONMENTNAME"
    else
        if [ -e "$REQUIREMENTS_TXT_FILE" ]; then
            VIRTUALENVIRONMENTOPTIONS="$VIRTUALENVIRONMENTOPTIONS --system-site-packages"
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
    if [ -e "$REQUIREMENTS_TXT_FILE" ]
    then
        set +e
        echo "Running uv pip install..."
        InstallCommand="uv pip install --cache-dir $PIP_CACHE_DIR -r $REQUIREMENTS_TXT_FILE | ts $TS_FMT"
        printf %s " , $InstallCommand" >> "$COMMAND_MANIFEST_FILE"
        output=$( ( uv pip install --cache-dir $PIP_CACHE_DIR -r $REQUIREMENTS_TXT_FILE | ts $TS_FMT; exit ${PIPESTATUS[0]} ) 2>&1; exit ${PIPESTATUS[0]} )
        pipInstallExitCode=${PIPESTATUS[0]}

        set -e
        echo "${output}"
        if [[ $pipInstallExitCode != 0 ]]
        then
            LogError "${output} | Exit code: ${pipInstallExitCode} | Please review your requirements.txt | ${moreInformation}"
            exit $pipInstallExitCode
        fi
    elif [ -e "setup.py" ]
    then
        set +e
        echo "Running pip install setuptools..."
        InstallSetuptoolsPipCommand="pip install setuptools"
        printf %s " , $InstallSetuptoolsPipCommand" >> "$COMMAND_MANIFEST_FILE"
        pip install setuptools
        echo "Running python setup.py install..."
        InstallCommand="pip install . --cache-dir $PIP_CACHE_DIR --prefer-binary | ts $TS_FMT"
        printf %s " , $InstallCommand" >> "$COMMAND_MANIFEST_FILE"
        output=$( ( pip install . --cache-dir $PIP_CACHE_DIR --prefer-binary | ts $TS_FMT; exit ${PIPESTATUS[0]} ) 2>&1; exit ${PIPESTATUS[0]} )
        pythonBuildExitCode=${PIPESTATUS[0]}
        set -e
        echo "${output}"
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
            InstallUvCommand="uv sync --active --link-mode copy"
            printf %s " , $InstallUvCommand" >> "$COMMAND_MANIFEST_FILE"
            output=$( ( $InstallUvCommand; exit ${PIPESTATUS[0]} ) 2>&1 )
            uvExitCode=${PIPESTATUS[0]}
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
            echo "Done in $ELAPSED_TIME sec(s)."
            set -e
            echo "${output}"
        fi
    else
        echo $REQS_NOT_FOUND_MSG
    fi

    # For virtual environment, we use the actual 'python' alias that as setup by the venv,
    python_bin=python
{{ else }}
    moreInformation="More information: https://aka.ms/troubleshoot-python"
    if [ -e "$REQUIREMENTS_TXT_FILE" ]
    then
        set +e
        echo
        echo Running uv pip install...
        START_TIME=$SECONDS
        InstallCommand="uv pip install --cache-dir $PIP_CACHE_DIR -r $REQUIREMENTS_TXT_FILE --target=\"{{ PackagesDirectory }}\" {{ PipUpgradeFlag }} | ts $TS_FMT"
        printf %s " , $InstallCommand" >> "$COMMAND_MANIFEST_FILE"
        output=$( ( uv pip install --cache-dir $PIP_CACHE_DIR -r $REQUIREMENTS_TXT_FILE --target="{{ PackagesDirectory }}" {{ PipUpgradeFlag }} | ts $TS_FMT; exit ${PIPESTATUS[0]} ) 2>&1; exit ${PIPESTATUS[0]} )
        pipInstallExitCode=${PIPESTATUS[0]}

        ELAPSED_TIME=$(($SECONDS - $START_TIME))
        echo "Done in $ELAPSED_TIME sec(s)."
        set -e
        echo "${output}"
        if [[ $pipInstallExitCode != 0 ]]
        then
            LogError "${output} | Exit code: ${pipInstallExitCode} | Please review your requirements.txt | ${moreInformation}"
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
        echo "Done in $ELAPSED_TIME sec(s)."

        set +e
        echo "Running pip install setuptools..."
        InstallSetuptoolsPipCommand="pip install setuptools"
        printf %s " , $InstallSetuptoolsPipCommand" >> "$COMMAND_MANIFEST_FILE"
        pip install setuptools
        echo "Running pip install..."
        InstallCommand="$python -m pip install . --cache-dir $PIP_CACHE_DIR --prefer-binary --target="{{ PackagesDirectory }}" {{ PipUpgradeFlag }} | ts $TS_FMT"
        printf %s " , $InstallCommand" >> "$COMMAND_MANIFEST_FILE"
        output=$( ( $python -m pip install . --cache-dir $PIP_CACHE_DIR --prefer-binary --target="{{ PackagesDirectory }}" {{ PipUpgradeFlag }} | ts $TS_FMT; exit ${PIPESTATUS[0]} ) 2>&1; exit ${PIPESTATUS[0]} )
        pythonBuildExitCode=${PIPESTATUS[0]}
        set -e
        echo "${output}"
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
            
            set +e
            SITE_PACKAGES_PATH="{{ PackagesDirectory }}"
            echo "Installing dependencies..."
            # Stream the export directly into uv pip install using process substitution
            InstallUvCommand="uv export --locked | uv pip install --link-mode copy --target $SITE_PACKAGES_PATH -r -"
            printf %s " , $InstallUvCommand" >> "$COMMAND_MANIFEST_FILE"
            output=$( ( eval $InstallUvCommand; exit ${PIPESTATUS[0]} ) 2>&1 )
            uvExitCode=${PIPESTATUS[0]}
            ELAPSED_TIME=$(($SECONDS - $START_TIME))
            echo "Done in $ELAPSED_TIME sec(s)."
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
            echo "Done in $ELAPSED_TIME sec(s)."
            set -e
            echo "${output}"
        fi
    else
        echo $REQS_NOT_FOUND_MSG
    fi

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
            echo "Done in $ELAPSED_TIME sec(s)."
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
                echo "Done in $ELAPSED_TIME sec(s)."
            fi
        fi
    {{ end }}
{{ end }}
