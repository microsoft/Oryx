phpBin=`which php`
echo "PHP executable: $phpBin"

{{ if ComposerFileExists }}
declare -r composer='/opt/php-composer/composer.phar'
echo "Composer archive: $composer"
echo "Running composer install..."
echo
php $composer install --ignore-platform-reqs --no-interaction
{{ else }}
echo "No 'composer.json' file found; not running composer install."
{{ end }}
