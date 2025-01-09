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

    if [ -e "$REQUIREMENTS_TXT_FILE" ]; then
        VIRTUALENVIRONMENTOPTIONS="$VIRTUALENVIRONMENTOPTIONS --system-site-packages"
    fi

    echo Creating virtual environment...

    CreateVenvCommand="$python -m $VIRTUALENVIRONMENTMODULE $VIRTUALENVIRONMENTNAME $VIRTUALENVIRONMENTOPTIONS"
    echo "BuildCommands=$CreateVenvCommand" >> "$COMMAND_MANIFEST_FILE"

    $python -m $VIRTUALENVIRONMENTMODULE $VIRTUALENVIRONMENTNAME $VIRTUALENVIRONMENTOPTIONS

    echo Activating virtual environment...
    printf %s " , $ActivateVenvCommand" >> "$COMMAND_MANIFEST_FILE"
    ActivateVenvCommand="source $VIRTUALENVIRONMENTNAME/bin/activate"
    source $VIRTUALENVIRONMENTNAME/bin/activate

    moreInformation="More information: https://aka.ms/troubleshoot-python"
    if [ -e "$REQUIREMENTS_TXT_FILE" ]
    then
        set +e
        echo "Running pip install..."
        InstallCommand="python -m pip install --cache-dir $PIP_CACHE_DIR --prefer-binary -r $REQUIREMENTS_TXT_FILE | ts $TS_FMT"
        printf %s " , $InstallCommand" >> "$COMMAND_MANIFEST_FILE"
        output=$( ( python -m pip install --cache-dir $PIP_CACHE_DIR --prefer-binary -r $REQUIREMENTS_TXT_FILE | ts $TS_FMT; exit ${PIPESTATUS[0]} ) 2>&1; exit ${PIPESTATUS[0]} )
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
        echo "Running python setup.py install..."
        InstallCommand="$python setup.py install --user| ts $TS_FMT"
        printf %s " , $InstallCommand" >> "$COMMAND_MANIFEST_FILE"
        output=$( ( $python setup.py install --user| ts $TS_FMT; exit ${PIPESTATUS[0]} ) 2>&1; exit ${PIPESTATUS[0]} )
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
        set +e
        echo "Running pip install poetry..."
        InstallPipCommand="pip install poetry==1.8.5"
        printf %s " , $InstallPipCommand" >> "$COMMAND_MANIFEST_FILE"
        pip install poetry==1.8.5
        echo "Running poetry install..."
        InstallPoetryCommand="poetry install --no-dev"
        printf %s " , $InstallPoetryCommand" >> "$COMMAND_MANIFEST_FILE"
        output=$( ( poetry install --no-dev; exit ${PIPESTATUS[0]} ) 2>&1)
        pythonBuildExitCode=${PIPESTATUS[0]}
        set -e
        echo "${output}"
        if [[ $pythonBuildExitCode != 0 ]]
        then
            LogWarning "${output} | Exit code: {pythonBuildExitCode} | Please review message | ${moreInformation}"
            exit $pythonBuildExitCode
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
        echo Running pip install...
        START_TIME=$SECONDS
        InstallCommand="$python -m pip install --cache-dir $PIP_CACHE_DIR --prefer-binary -r $REQUIREMENTS_TXT_FILE --target="{{ PackagesDirectory }}" {{ PipUpgradeFlag }} | ts $TS_FMT"
        printf %s " , $InstallCommand" >> "$COMMAND_MANIFEST_FILE"
        output=$( ( $python -m pip install --cache-dir $PIP_CACHE_DIR --prefer-binary -r $REQUIREMENTS_TXT_FILE --target="{{ PackagesDirectory }}" {{ PipUpgradeFlag }} | ts $TS_FMT; exit ${PIPESTATUS[0]} ) 2>&1; exit ${PIPESTATUS[0]} )
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
        echo "Running python setup.py install..."
        InstallCommand="$python setup.py install --user| ts $TS_FMT"
        printf %s " , $InstallCommand" >> "$COMMAND_MANIFEST_FILE"
        output=$( ( $python setup.py install --user| ts $TS_FMT; exit ${PIPESTATUS[0]} ) 2>&1; exit ${PIPESTATUS[0]} )
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
        set +e
        echo "Running pip install poetry..."
        InstallPipCommand="pip install poetry"
        printf %s " , $InstallPipCommand" >> "$COMMAND_MANIFEST_FILE"
        pip install poetry
        START_TIME=$SECONDS
        echo "Running poetry install..."
        InstallPoetryCommand="poetry install --no-dev"
        printf %s " , $InstallPoetryCommand" >> "$COMMAND_MANIFEST_FILE"
        output=$( ( poetry install --no-dev; exit ${PIPESTATUS[0]} ) 2>&1 )
        pythonBuildExitCode=${PIPESTATUS[0]}
        ELAPSED_TIME=$(($SECONDS - $START_TIME))
        echo "Done in $ELAPSED_TIME sec(s)."
        set -e
        echo "${output}"
        if [[ $pythonBuildExitCode != 0 ]]
        then
            LogWarning "${output} | Exit code: {pythonBuildExitCode} | Please review message | ${moreInformation}"
            exit $pythonBuildExitCode
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
