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

                    if [ $key != *"SHA"* ]; then
                        if [ -n $old_value ]; then
                            update_line="Updated $key from $old_value to $value"
                        else
                            update_line="Added $key to $value"
                        fi
                        echo "$update_line" >> "$Updated_ValuesFILE"
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
                    if [ $key != *"SHA"* ]; then
                        update_line="Updated $key from $old_value to $valueInVariableGroup"
                        echo "$update_line" >> "$Updated_ValuesFILE"
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


