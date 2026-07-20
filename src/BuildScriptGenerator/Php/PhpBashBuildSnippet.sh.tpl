phpBin=`which php`
echo "PHP executable: $phpBin"

{{ if CustomBuildCommand | IsNotBlank }}
echo
echo "Running custom build command '{{ CustomBuildCommand }}'..."
echo
START_TIME=$SECONDS
{{ CustomBuildCommand }}
ELAPSED_TIME=$(($SECONDS - $START_TIME))
echo "Custom build command done in $ELAPSED_TIME sec(s)."
{{ else if ComposerFileExists }}
echo "Composer archive: $composer"
echo "Running 'composer install --ignore-platform-reqs --no-interaction'..."
echo
# `--ignore-platform-reqs` ensures Composer won't fail a build when
# an extension is missing from the build image (it could exist in the
# runtime image regardless)
START_TIME=$SECONDS
php $composer install --ignore-platform-reqs --no-interaction
ELAPSED_TIME=$(($SECONDS - $START_TIME))
echo "composer install done in $ELAPSED_TIME sec(s)."
{{ else }}
echo "No 'composer.json' file found; not running 'composer install'."
{{ end }}

