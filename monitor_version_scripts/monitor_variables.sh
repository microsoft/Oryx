# #!/bin/bash

# printenv

apt-get update
apt-get install -y python3 python3-pip

mkdir generated_files

# curl -o generated_files/php_version.xml "https://www.php.net/downloads.php"
# curl -o generated_files/node_version.xml "https://nodejs.org/en/about/previous-releases"
curl -o generated_files/dotnet_version.xml "https://dotnet.microsoft.com/en-us/download/dotnet"

# pip install bs4
# pip install lxml
# pip install requests

create_versionfile() {
    FILE=$1
    if [ ! -e "$FILE" ]; then
        # Create the file
        touch "$FILE"
        echo "File '$FILE' created."
    else
        echo "File '$FILE' already exists."
    fi
}

# create_versionfile generated_files/node_latest_versions.txt
# create_versionfile generated_files/php_latest_versions.txt
# create_versionfile generated_files/python_latest_versions.txt
create_versionfile generated_files/dotnet_latest_versions.txt
create_versionfile generated_files/dotnet_sdk_latest_versions.txt


# latest_stack_versions_FILE=latest_stack_versions.yaml
# cat <<EOL > $latest_stack_versions_FILE
# variables:
# EOL

# python3 web_scrap_files/php_versions.py 
# python3 web_scrap_files/node_versions.py
# python3 web_scrap_files/python_versions.py
python3 web_scrap_files/dotnet_versions.py

# chmod +x update_latest_stack_versions.sh
# ./update_latest_stack_versions.sh

# chmod +x update_constants.sh
# ./update_constants.sh

# chmod +x update_versions_to_build.sh
# ./update_versions_to_build.sh

# # #create a monitor.yaml pipeline, create multiple jobs in the pipeline. 
# # # each job for different stack, in each job call the respective .py file for web scrapping, now store the new values in latest_versions file(make diff version file for diff stacks)
# # # compare values latest values for each stack and then update the latest_stack_versions.yaml
# # # as of now, only minor versions

# rm -r generated_files
# rm $latest_stack_versions_FILE