declare -r TS_FMT='[%T%z] '
declare -r REQS_NOT_FOUND_MSG='Could not find setup.py or requirements.txt; Not running pip install'
echo "Python Version: $python"
PIP_CACHE_DIR=/usr/local/share/pip-cache

{{ if PythonBuildCommandsFileName | IsNotBlank }}
COMMAND_MANIFEST_FILE={{ PythonBuildCommandsFileName }}
{{ end }}

echo "Creating directory for command manifest file if it doesnot exist"
mkdir -p "$(dirname "$COMMAND_MANIFEST_FILE")"
echo "Removing existing manifest file"
rm -f "$COMMAND_MANIFEST_FILE"

echo "PlatformWithVersion=Python {{ PythonVersion }}" > "$COMMAND_MANIFEST_FILE"

InstallCommand=""

if [ ! -d "$PIP_CACHE_DIR" ];then
	mkdir -p $PIP_CACHE_DIR
fi

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

	if [ -e "requirements.txt" ]; then
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

	if [ -e "requirements.txt" ]
	then
		echo "Running pip install..."
		InstallCommand="python -m pip install --cache-dir $PIP_CACHE_DIR --prefer-binary -r requirements.txt | ts $TS_FMT"
		printf %s " , $InstallCommand" >> "$COMMAND_MANIFEST_FILE"
		python -m pip install --cache-dir $PIP_CACHE_DIR --prefer-binary -r requirements.txt | ts $TS_FMT
		pipInstallExitCode=${PIPESTATUS[0]}
		if [[ $pipInstallExitCode != 0 ]]
		then
			exit $pipInstallExitCode
		fi
	elif [ -e "setup.py" ]
	then
		echo "Running python setup.py install..."
		InstallCommand="$python setup.py install --user| ts $TS_FMT"
		printf %s " , $InstallCommand" >> "$COMMAND_MANIFEST_FILE"
		$python setup.py install --user| ts $TS_FMT
		pythonBuildExitCode=${PIPESTATUS[0]}
		if [[ $pythonBuildExitCode != 0 ]]
		then
			exit $pythonBuildExitCode
		fi
	elif [ -e "pyproject.toml" ]
	then
		echo "Running pip install poetry..."
		InstallPipCommand="pip install poetry"
		printf %s " , $InstallPipCommand" >> "$COMMAND_MANIFEST_FILE"
		pip install poetry
		echo "Running poetry install..."
		InstallPoetryCommand="poetry install"
		printf %s " , $InstallPoetryCommand" >> "$COMMAND_MANIFEST_FILE"
		poetry install
		pythonBuildExitCode=${PIPESTATUS[0]}
		if [[ $pythonBuildExitCode != 0 ]]
		then
			exit $pythonBuildExitCode
		fi
	else
		echo $REQS_NOT_FOUND_MSG
	fi

	# For virtual environment, we use the actual 'python' alias that as setup by the venv,
	python_bin=python
{{ else }}
	if [ -e "requirements.txt" ]
	then
		echo
		echo Running pip install...
		START_TIME=$SECONDS
		InstallCommand="$python -m pip install --cache-dir $PIP_CACHE_DIR --prefer-binary -r requirements.txt --target="{{ PackagesDirectory }}" --upgrade | ts $TS_FMT"
		printf %s " , $InstallCommand" >> "$COMMAND_MANIFEST_FILE"
		$python -m pip install --cache-dir $PIP_CACHE_DIR --prefer-binary -r requirements.txt --target="{{ PackagesDirectory }}" --upgrade | ts $TS_FMT
		pipInstallExitCode=${PIPESTATUS[0]}
		ELAPSED_TIME=$(($SECONDS - $START_TIME))
		echo "Done in $ELAPSED_TIME sec(s)."

		if [[ $pipInstallExitCode != 0 ]]
		then
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

		echo "Running python setup.py install..."
		InstallCommand="$python setup.py install --user| ts $TS_FMT"
		printf %s " , $InstallCommand" >> "$COMMAND_MANIFEST_FILE"
		$python setup.py install --user| ts $TS_FMT
		pythonBuildExitCode=${PIPESTATUS[0]}
		if [[ $pythonBuildExitCode != 0 ]]
		then
			exit $pythonBuildExitCode
		fi
	elif [ -e "pyproject.toml" ]
	then
		echo "Running pip install poetry..."
		InstallPipCommand="pip install poetry"
		printf %s " , $InstallPipCommand" >> "$COMMAND_MANIFEST_FILE"
		pip install poetry
		START_TIME=$SECONDS
		echo "Running poetry install..."
		InstallPoetryCommand="poetry install"
		printf %s " , $InstallPoetryCommand" >> "$COMMAND_MANIFEST_FILE"
		poetry install
		ELAPSED_TIME=$(($SECONDS - $START_TIME))
		echo "Done in $ELAPSED_TIME sec(s)."
		pythonBuildExitCode=${PIPESTATUS[0]}
		if [[ $pythonBuildExitCode != 0 ]]
		then
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
	if [ -e "$SOURCE_DIR/manage.py" ]
	then
		if grep -iq "Django" "$SOURCE_DIR/requirements.txt"
		then
			echo
			echo Content in source directory is a Django app
			echo Running 'collectstatic'...
			START_TIME=$SECONDS
			CollectStaticCommand="$python_bin manage.py collectstatic --noinput || EXIT_CODE=$? && true "
			printf %s " , $CollectStaticCommand" >> "$COMMAND_MANIFEST_FILE"
			$python_bin manage.py collectstatic --noinput || EXIT_CODE=$? && true ; 
			echo "'collectstatic' exited with exit code $EXIT_CODE."
			ELAPSED_TIME=$(($SECONDS - $START_TIME))
			echo "Done in $ELAPSED_TIME sec(s)."
		fi
	fi
{{ end }}


ReadImageType=$(cat /opt/oryx/.imagetype)

if [ "$ReadImageType" = "vso-focal" ]
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