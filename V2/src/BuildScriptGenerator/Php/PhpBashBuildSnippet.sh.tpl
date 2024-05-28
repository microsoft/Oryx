phpBin=`which php`
echo "PHP executable: $phpBin"

{{ if ComposerFileExists }}
echo "Composer archive: $composer"
echo "Running 'composer install --ignore-platform-reqs --no-interaction'..."
echo
# `--ignore-platform-reqs` ensures Composer won't fail a build when
# an extension is missing from the build image (it could exist in the
# runtime image regardless)
php $composer install --ignore-platform-reqs --no-interaction
{{ else }}
echo "No 'composer.json' file found; not running 'composer install'."
{{ end }}

