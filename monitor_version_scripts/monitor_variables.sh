# #!/bin/bash

apt-get update
apt-get install -y python3 python3-pip
pip install bs4
pip install lxml
pip install requests

set -e

error_handler() {
    echo "Error occurred in script at line: $1"
    exit 1
}

trap 'error_handler $LINENO' ERR

mkdir -p generated_files

curl -o generated_files/php_version.xml "https://www.php.net/downloads.php"
curl -o generated_files/node_version.xml "https://nodejs.org/en/about/previous-releases"
curl -o generated_files/dotnet_version.xml "https://dotnet.microsoft.com/en-us/download/dotnet"

create_versionfile() {
    FILE=$1
    if [ ! -e "$FILE" ]; then
        # Create the file
        echo -n "" > $FILE
        echo "File '$FILE' created."
    else
        echo "File '$FILE' already exists."
    fi
}

create_versionfile generated_files/node_latest_versions.txt
create_versionfile generated_files/php_latest_versions.txt
create_versionfile generated_files/python_latest_versions.txt
create_versionfile generated_files/dotnet_latest_versions.txt
create_versionfile generated_files/dotnet_sdk_latest_versions.txt

latest_stack_versions_FILE=latest_stack_versions.yaml
cat <<EOL > $latest_stack_versions_FILE
variables:
EOL

python3 web_scrap_files/php_versions.py 
python3 web_scrap_files/node_versions.py
python3 web_scrap_files/python_versions.py
python3 web_scrap_files/dotnet_versions.py

chmod +x update_latest_stack_versions.sh
./update_latest_stack_versions.sh

chmod +x update_constants.sh
./update_constants.sh

chmod +x update_versions_to_build.sh
./update_versions_to_build.sh

rm -rf "generated_files"
rm "Stack_Updated_values.txt"