declare -r TS_FMT='[%T%z] '
declare -r REQS_NOT_FOUND_MSG='Could not find requirements.txt; Not running pip install'
echo "Python Version: $python"

zippedOutputFileName=oryx_output.tar.gz

{{ if VirtualEnvironmentName | IsNotBlank }}

{{ if PackagesDirectory | IsNotBlank }}
if [ -d "{{ PackagesDirectory }}" ]
then
	rm -fr "{{ PackagesDirectory }}"
fi
{{ end }}

VIRTUALENVIRONMENTNAME={{ VirtualEnvironmentName }}
VIRTUALENVIRONMENTMODULE={{ VirtualEnvironmentModule }}
VIRTUALENVIRONMENTOPTIONS={{ VirtualEnvironmentParameters }}
zippedVirtualEnvName="$VIRTUALENVIRONMENTNAME.tar.gz"

echo "Python Virtual Environment: $VIRTUALENVIRONMENTNAME"

echo Creating virtual environment...
$python -m $VIRTUALENVIRONMENTMODULE $VIRTUALENVIRONMENTNAME $VIRTUALENVIRONMENTOPTIONS

echo Activating virtual environment...
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
	pip install --prefer-binary -r requirements.txt | ts $TS_FMT
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
	$pip install --prefer-binary -r requirements.txt --target="{{ PackagesDirectory }}" --upgrade | ts $TS_FMT
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
python_bin=$python

# Detect the location of the site-packages to add the .pth file
# For the local site package, only major and minor versions are provided, so we fetch it again
SITE_PACKAGE_PYTHON_VERSION=$($python -c "import sys; print(str(sys.version_info.major) + '.' + str(sys.version_info.minor))")
SITE_PACKAGES_PATH=$HOME"/.local/lib/python"$SITE_PACKAGE_PYTHON_VERSION"/site-packages"
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
		echo Running 'collectstatic'...
		START_TIME=$SECONDS
		$python_bin manage.py collectstatic --noinput || EXIT_CODE=$? && true ; 
		echo "'collectstatic' exited with exit code $EXIT_CODE."
		ELAPSED_TIME=$(($SECONDS - $START_TIME))
		echo "Done in $ELAPSED_TIME sec(s)."
	fi
fi
{{ end }}

if [ -f "$zippedVirtualEnvName" ]
then
	echo
	echo "File '$zippedVirtualEnvName' already exists under '$SOURCE_DIR'. Deleting it..."
	rm -f "$zippedVirtualEnvName"
fi

if [ "$SOURCE_DIR" != "$DESTINATION_DIR" ]
then
	excludedDirectories=""
	{{ for excludedDir in DirectoriesToExcludeFromCopyToBuildOutputDir }}
	excludedDirectories+=" --exclude={{ excludedDir }}"
	{{ end }}
	
	if [ -d "$DESTINATION_DIR" ] && [ "$(ls -A $DESTINATION_DIR)" ]
	then
		echo
		echo "Destination directory is not empty. Deleting its contents..."
		START_TIME=$SECONDS
		rm -rf "$DESTINATION_DIR"/*
		ELAPSED_TIME=$(($SECONDS - $START_TIME))
		echo "Done in $ELAPSED_TIME sec(s)."
	fi

	mkdir -p "$DESTINATION_DIR"
	{{ if ZipAllOutput }}
		if [ -f "$DESTINATION_DIR/$zippedOutputFileName" ]
		then
			echo
			echo "Deleting existing file '$DESTINATION_DIR/$zippedOutputFileName'..."
			rm -f "$DESTINATION_DIR/$zippedOutputFileName"
		fi

		echo
		echo "Zipping the contents before copy to '$DESTINATION_DIR'..."
		echo
		START_TIME=$SECONDS
		cd "$SOURCE_DIR"
		touch "$DESTINATION_DIR/$zippedOutputFileName"
		tar $excludedDirectories --exclude=$zippedOutputFileName -zcf $zippedOutputFileName .
		cp -f $zippedOutputFileName "$DESTINATION_DIR/$zippedOutputFileName"
		ELAPSED_TIME=$(($SECONDS - $START_TIME))
		echo "Done in $ELAPSED_TIME sec(s)."
	{{ else }}
		{{ if VirtualEnvironmentName | IsNotBlank }}
		{{ if ZipVirtualEnvDir }}
			if [ -d "$VIRTUALENVIRONMENTNAME" ]
			then
				echo
				echo "Zipping existing '$VIRTUALENVIRONMENTNAME' folder..."
				START_TIME=$SECONDS
				# Make the contents of the '$VIRTUALENVIRONMENTNAME' folder appear in the zip file, 
				# not the folder itself
				cd "$VIRTUALENVIRONMENTNAME"
				tar -zcf ../$zippedVirtualEnvName .
				ELAPSED_TIME=$(($SECONDS - $START_TIME))
				echo "Done in $ELAPSED_TIME sec(s)."
			fi
		{{ end }}
		{{ end }}

		echo
		echo "Copying files to destination directory '$DESTINATION_DIR'..."
		START_TIME=$SECONDS
		cd "$SOURCE_DIR"
		rsync -rtE --links $excludedDirectories . "$DESTINATION_DIR"
		ELAPSED_TIME=$(($SECONDS - $START_TIME))
		echo "Done in $ELAPSED_TIME sec(s)."
	{{ end }}
fi
