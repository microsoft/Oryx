phpBin=`which php`
echo "PHP executable: $phpBin"

{{ if ComposerFileExists }}
echo "Composer archive: $composer"
echo "Running composer install..."
echo
php $composer install
{{ else }}
echo "No 'composer.json' file found; not running composer install."
{{ end }}
