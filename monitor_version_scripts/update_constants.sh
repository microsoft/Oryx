#!/bin/bash

# Read the YAML file line by line

# Define the content of the constants.yml file
Old_constants_FILE=$(cd .. && pwd)/images/constants.yml
Temp_constants_FILE=$(cd .. && pwd)/images/temp_constants.yml

mv $Old_constants_FILE $Temp_constants_FILE

constants_FILE=$(cd .. && pwd)/images/constants.yml
cat <<EOL > $constants_FILE
variables:
EOL

Updated_ValuesFILE=Updated_Values.txt
cat <<EOL > $Updated_ValuesFILE
EOL

// copy all values from override_constants.yaml to constants.yaml
while IFS= read -r line; do
    key=$(echo "$line" | yq e 'keys' - | sed 's/^[[:space:]]*-*//' | sed 's/^[[:space:]]*//')

    value=$(echo "$line" | yq e '.[]' -)

    echo "Key: $key, Value: $value,"
    yq eval ".variables.$key = \"$value\"" -i $constants_FILE
done < <(yq e '.[]' "override_constants.yaml")

update_node_versions_to_build(){
    node_versionsToBuild_FILE = $1
    version = $2
    value = $3

    version_found = false
    while IFS= read -r line; do
        if [[ "$line" == *"$version"* ]]; then
            version_found = true
            break
        fi
    done < "$node_versionsToBuild_FILE"

    if ! $version_found; then
        echo "$value" >> "$node_versionsToBuild_FILE"
        sort "$node_versionsToBuild_FILE" -o "$node_versionsToBuild_FILE"
    fi
}

# update_python_versions_to_build(){

# }

update_versions_to_build() {
    key = $1
    value = $2
    version=${key//[^0-9]/}
    if [[ "$key" == *"node"* ]]; then
        versionsToBuild_Folder=$(cd .. && pwd)/platforms/nodejs/versions
        debianFlavors="node$version"
    elif [[ "$key" == *"python"* ]]; then
        versionsToBuild_Folder=$(cd .. && pwd)/platforms/python/versions
        debianFlavors="python$version"
    elif [[ "$key" == *"php"* ]]; then
        versionsToBuild_Folder=$(cd .. && pwd)/platforms/php/versions
        debianFlavors="php$version"
    elif [[ "$key" == *"NET"* ]]; then
        versionsToBuild_Folder=$(cd .. && pwd)/platforms/dotnet/versions
        debianFlavors="dotnet$version"
    fi 

    debianFlavors+="DebianFlavors"

    IFS=','

    for flavor in $debianFlavors; do
        versionsToBuild_FILE="$versionsToBuild_Folder/$flavor"

        if [[ "$key" == *"node"* ]]; then
            update_node_versions_to_build $versionsToBuild_FILE $version $value
        # elif [[ "$key" == *"python"* ]]; then

        # elif [[ "$key" == *"php"* ]]; then

        # elif [[ "$key" == *"NET"* ]]; then

        fi
    done
}

update_constants_file() {
    while IFS= read -r line; do
        # Use yq to parse the YAML line and extract the key and value
        key=$(echo "$line" | yq e 'keys' - | sed 's/^[[:space:]]*-*//' | sed 's/^[[:space:]]*//')

        value=$(echo "$line" | yq e '.[]' -)

        echo "Key: $key, Value: $value,"

        keyInVariableGroup=$(echo "$key" | tr '[:lower:]' '[:upper:]')

        # Check if the key exists in the environment variables
        if printenv "$keyInVariableGroup" > /dev/null; then
            # If the key exists, get its value
            valueInVariableGroup=$(printenv "$keyInVariableGroup")
            echo "The value of $key is: $valueInVariableGroup"

            if [ "$valueInVariableGroup" = "latest" ]; then
                old_value=$(yq eval ".variables.$key" $Temp_constants_FILE)

                if [ $old_value = $value ]; then
                    echo "$key is already upto date"
                else
                    yq eval ".variables.$key = \"$value\"" -i $constants_FILE
                    echo "Updated constants.yml with latest value $key=$value"

                    if [[ "$key" != *"SHA"* ]]; then
                        if [ -n $old_value ]; then
                            update_line="Updated $key from $old_value to $value"
                        else
                            update_line="Added $key to $value"
                        fi
                        echo "$update_line" >> "$Updated_ValuesFILE"

                        update_versions_to_build $key $value
                    fi
                fi

            elif [ "$valueInVariableGroup" = "dont_change" ]; then
                old_value=$(yq eval ".variables.$key" $Temp_constants_FILE)
                yq eval ".variables.$key = \"$old_value\"" -i $constants_FILE
                echo "constants.yml with old value $key=$old_value"
            else
                old_value=$(yq eval ".variables.$key" $Temp_constants_FILE)
                if [ $old_value = $valueInVariableGroup ]; then
                    echo "$key has required value in constants.yml"
                    yq eval ".variables.$key = \"$valueInVariableGroup\"" -i $constants_FILE
                else
                    yq eval ".variables.$key = \"$valueInVariableGroup\"" -i $constants_FILE
                    echo "Updated constants.yml with given value $key=$valueInVariableGroup"
                    if [[ "$key" != *"SHA"* ]]; then
                        update_line="Updated $key from $old_value to $valueInVariableGroup"
                        echo "$update_line" >> "$Updated_ValuesFILE"

                        update_versions_to_build $key $value
                    fi
                fi
            fi

        else
            if yq eval ".variables | has(\"$key\")" $Temp_constants_FILE | grep -q 'true'; then
                old_value=$(yq eval ".variables.$key" $Temp_constants_FILE)
                yq eval ".variables.$key = \"$old_value\"" -i $constants_FILE
                echo "constants.yml with old value $key=$old_value"
            else
                yq eval ".variables.$key = \"$value\"" -i $constants_FILE
                echo "Added $key = $value in constants.yml"
            fi
        fi

    done < <(yq e '.[]' "$1")
}

update_constants_file "latest_stack_versions.yaml"

# delete temporary constants file
rm $Temp_constants_FILE


