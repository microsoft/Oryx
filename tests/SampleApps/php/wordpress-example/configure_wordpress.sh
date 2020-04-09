#!/bin/bash

set -ex

chmod +x wp-cli.phar
mv wp-cli.phar /usr/local/bin/wp

wpuser='wordpressdb'
dbname='wordpressdb'
sitename='localsite'
password="Wordpress@123"

echo "================================================================="
echo "Begining WordPress configuration......."
echo "================================================================="

echo "Database Name: "$dbname
echo "Site Name: "$sitename

echo "create the wp-config file with standard setup ...."
wp core config --dbhost="127.0.0.1" --dbname=$dbname --dbuser=$dbname --dbpass=$password --extra-php --allow-root<<PHP
define( 'WP_DEBUG', false );
define( 'DISALLOW_FILE_EDIT', true );
PHP

# restart mysql server and install WordPress
service mysql start

echo "Installing WordPress......."
wp core install --url="http://127.0.0.1" --title="$sitename" --admin_user="$wpuser" --admin_password="$password" --admin_email="user@example.org" --allow-root

# set homepage as front page
wp option update show_on_front 'page' --allow-root

# delete akismet and hello dolly
wp plugin delete akismet --allow-root
wp plugin delete hello --allow-root


# create a navigation bar
echo "================================================================="
echo "Configuration is complete. Your username/password is listed below."
echo ""
echo "Username: $wpuser"
echo "Password: $password"
echo ""
echo "================================================================="