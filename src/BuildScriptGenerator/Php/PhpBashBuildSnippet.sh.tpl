echo "PHP Version: $php"
cd "$SOURCE_DIR"
echo "Running composer install..."
echo
$php /opt/php-composer/composer.phar install
