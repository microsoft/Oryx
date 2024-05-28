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

# As of mariadb 10.5, which bullseye is using, the service name is now mariadb
# instead of mysql, but the mysql command is still symlinked.
# See: https://mariadb.com/kb/en/changes-improvements-in-mariadb-105/#binaries-named-mariadb-mysql-symlinked
#
# restart mysql server and install WordPress
if service --status-all | grep -Fq 'mysql'; then    
    service mysql start
else
    service mariadb start
fi

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