declare -r TS_FMT='[%T%z] '
declare -r REQS_NOT_FOUND_MSG='Could not find requirements.txt; Not running pip install'
echo "Python Version: $python"
PIP_CACHE_DIR=/usr/local/share/pip-cache

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

echo
echo "Using Python version:"
python --version

echo
echo "Python binary location:"
which python

VIRTUALENVIRONMENTNAME={{ VirtualEnvironmentName }}
VIRTUALENVIRONMENTMODULE={{ VirtualEnvironmentModule }}
VIRTUALENVIRONMENTOPTIONS={{ VirtualEnvironmentParameters }}
zippedVirtualEnvFileName={{ CompressedVirtualEnvFileName }}

echo
echo "Python Virtual Environment: $VIRTUALENVIRONMENTNAME"
echo Creating virtual environment ...
python -m $VIRTUALENVIRONMENTMODULE $VIRTUALENVIRONMENTNAME $VIRTUALENVIRONMENTOPTIONS

echo Activating virtual environment ...
source $VIRTUALENVIRONMENTNAME/bin/activate

if [ -e "requirements.txt" ]
then
	echo
	echo "Upgrading pip..."
	START_TIME=$SECONDS
	pip install --upgrade pip
	ELAPSED_TIME=$(($SECONDS - $START_TIME))
	echo "Done in $ELAPSED_TIME sec(s)."

	echo "Running pip install..."
	pip install --cache-dir $PIP_CACHE_DIR --prefer-binary -r requirements.txt | ts $TS_FMT
	pipInstallExitCode=${PIPESTATUS[0]}
	if [[ $pipInstallExitCode != 0 ]]
	then
		exit $pipInstallExitCode
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
	$pip install  --cache-dir $PIP_CACHE_DIR --prefer-binary -r requirements.txt --target="{{ PackagesDirectory }}" --upgrade | ts $TS_FMT
	pipInstallExitCode=${PIPESTATUS[0]}
	ELAPSED_TIME=$(($SECONDS - $START_TIME))
	echo "Done in $ELAPSED_TIME sec(s)."

	if [[ $pipInstallExitCode != 0 ]]
	then
		exit $pipInstallExitCode
	fi
else
	echo $REQS_NOT_FOUND_MSG
fi

# We need to use the python binary selected by benv
python_bin=`which python`

# Detect the location of the site-packages to add the .pth file
# For the local site package, only major and minor versions are provided, so we fetch it again
SITE_PACKAGE_PYTHON_VERSION=$($python -c "import sys; print(str(sys.version_info.major) + '.' + str(sys.version_info.minor))")
SITE_PACKAGES_PATH=$PIP_CACHE_DIR"/lib/python"$SITE_PACKAGE_PYTHON_VERSION"/site-packages"
mkdir -p $SITE_PACKAGES_PATH
# To make sure the packages are available later, e.g. for collect static or post-build hooks, we add a .pth pointing to them
APP_PACKAGES_PATH=$(pwd)"/{{ PackagesDirectory }}"
echo $APP_PACKAGES_PATH > $SITE_PACKAGES_PATH"/oryx.pth"

{{ end }}

echo Done running pip install.

{{ if !DisableCollectStatic }}
if [ -e "$SOURCE_DIR/manage.py" ]
then
	if grep -iq "Django" "$SOURCE_DIR/requirements.txt"
	then
		echo
		echo Content in source directory is a Django app
		echo Running 'collectstatic' ...
		START_TIME=$SECONDS
		$python_bin manage.py collectstatic --noinput || EXIT_CODE=$? && true ; 
		echo "'collectstatic' exited with exit code $EXIT_CODE."
		ELAPSED_TIME=$(($SECONDS - $START_TIME))
		echo "Done in $ELAPSED_TIME sec(s)."
	fi
fi
{{ end }}

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
