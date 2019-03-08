declare -r composer='/opt/php-composer/composer.phar'

echo "PHP executable: $php"
echo "Composer archive: $composer"

cd "$SOURCE_DIR"
echo "Running composer install..."
echo
$php $composer install
