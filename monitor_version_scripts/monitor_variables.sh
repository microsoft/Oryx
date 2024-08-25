# #!/bin/bash

printenv

apt-get update
apt-get install -y python3 python3-pip

mkdir generated_files

curl -o generated_files/php_version.xml "https://www.php.net/downloads.php"
curl -o generated_files/node_version.xml "https://nodejs.org/en/about/previous-releases"
curl -o generated_files/dotnet_version.xml "https://dotnet.microsoft.com/en-us/download/dotnet"

pip install bs4
pip install lxml
pip install requests

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

create_versionfile generated_files/node_latest_versions.txt
create_versionfile generated_files/php_latest_versions.txt
create_versionfile generated_files/python_latest_versions.txt
create_versionfile generated_files/dotnet_latest_versions.txt

python3 php_versions.py 
python3 node_versions.py
python3 python_versions.py
python3 dotnet_versions.py

#read the whole text file, if that particular val exists update it in latest_stack_versions.yaml
split_lines() {
    while IFS= read -r line; do
        if [[ "$line" == *"="* ]]; then

            IFS=',' read -ra keyvalue_pairs <<< "$line"

            for keyvalue_pair in "${keyvalue_pairs[@]}"; do
                key="${keyvalue_pair%%=*}"
                value="${keyvalue_pair#*=}"
                echo "key: $key, value: $value"

                if yq eval ".variables | has(\"$key\")" latest_stack_versions.yaml | grep -q 'true'; then
                    if [[ "$key" != *"python"* ]]; then
                        yq eval ".variables.$key = \"$value\"" -i latest_stack_versions.yaml
                    else
                        #this is only for python, in https://www.python.org/downloads/ all available minor versions are present of a major version
                        #so update with latest one
                        current_value=$(yq eval ".variables.$key" latest_stack_versions.yaml)

                        # Update the key in latest_stack_versions.yaml
                        if [[ $(printf '%s\n' "$current_value" "$value" | sort -V | tail -n 1) != "$current_value" ]]; then
                            yq eval ".variables.$key = \"$value\"" -i latest_stack_versions.yaml
                            echo "Updated $key in latest_stack_versions.yaml"
                        fi
                    fi
                else
                    yq eval ".variables.$key = \"$value\"" -i latest_stack_versions.yaml
                    echo "Added $key to latest_stack_versions.yaml"
                fi
            done
        fi
    done < "$1"
}

split_lines "generated_files/node_latest_versions.txt"
split_lines "generated_files/python_latest_versions.txt"
split_lines "generated_files/php_latest_versions.txt"
split_lines "generated_files/dotnet_latest_versions.txt"

yq eval -i 'sort_keys(..)' "latest_stack_versions.yaml"

chmod +x update_constants.sh
./update_constants.sh

# #create a monitor.yaml pipeline, create multiple jobs in the pipeline. 
# # each job for different stack, in each job call the respective .py file for web scrapping, now store the new values in latest_versions file(make diff version file for diff stacks)
# # compare values latest values for each stack and then update the latest_stack_versions.yaml
# # as of now, only minor versions

rm -r generated_files



