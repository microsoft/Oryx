#!/bin/bash

set -ex

apt-get update && apt-get install mariadb-server --yes

# As of mariadb 10.5, which bullseye is using, the service name is now mariadb
# instead of mysql, but the mysql command is still symlinked.
# See: https://mariadb.com/kb/en/changes-improvements-in-mariadb-105/#binaries-named-mariadb-mysql-symlinked
if service --status-all | grep -Fq 'mysql'; then    
    service mysql start
else
    service mariadb start
fi

PASSWDDB="Wordpress@123"
MAINDB="wordpressdb"

# If /etc/mysql/my.cnf exists then it won't ask for root password
if [ -f /etc/mysql/my.cnf ]; then
    echo "my.cnf exists"
    mysql -e "CREATE DATABASE ${MAINDB} /*\!40100 DEFAULT CHARACTER SET utf8 */;"
    mysql -e "CREATE USER ${MAINDB}@localhost IDENTIFIED BY '${PASSWDDB}';"
    mysql -e "GRANT ALL PRIVILEGES ON ${MAINDB}.* TO '${MAINDB}'@'localhost';"
    mysql -e "FLUSH PRIVILEGES;"

# If /etc/mysql/my.cnf doesn't exist then loging with default root password <blank>
else
    echo "Using default root user and default root password!"
    rootpasswd=""
    mysql -uroot -p${rootpasswd} -e "CREATE DATABASE ${MAINDB} /*\!40100 DEFAULT CHARACTER SET utf8 */;"
    mysql -uroot -p${rootpasswd} -e "CREATE USER ${MAINDB}@localhost IDENTIFIED BY '${PASSWDDB}';"
    mysql -uroot -p${rootpasswd} -e "GRANT ALL PRIVILEGES ON ${MAINDB}.* TO '${MAINDB}'@'localhost';"
    mysql -uroot -p${rootpasswd} -e "FLUSH PRIVILEGES;"
fi
