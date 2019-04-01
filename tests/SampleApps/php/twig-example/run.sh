#!/bin/sh

# Enter the source directory to make sure the script runs where the user expects
cd /var/www/html/out
export APACHE_DOCUMENT_ROOT='/var/www/html/out'
apache2-foreground
