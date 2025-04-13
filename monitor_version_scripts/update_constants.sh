#!/bin/bash

Old_constants_FILE="../images/constants.yml"
Temp_constants_FILE="../images/temp_constants.yml"

mv $Old_constants_FILE $Temp_constants_FILE

constants_FILE="../images/constants.yml"
cat <<EOL > $constants_FILE
variables:
EOL

Updated_ValuesFILE=Updated_Values.txt
echo -n "" > $Updated_ValuesFILE

Stack_Updated_values=Stack_Updated_values.txt
echo -n "" > $Stack_Updated_values

update_constants_file(){
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

            if [[ "$valueInVariableGroup" = "latest" ]]; then
                old_value=$(yq eval ".variables.$key" $Temp_constants_FILE)

                if [ $old_value = $value ]; then
                    yq eval ".variables.$key = \"$value\"" -i $constants_FILE
                    echo "$key is already upto date"
                else
                    # during updating php or python versions we need gpg keys as well during sdk building
                    # so if it is not present in Temp_constants_FILE or in variable group, ask for it

                    if [[ "$1" = "latest_stack_versions.yaml" && ("$key" = *"python"* || "$key" = *"php"*) && "$key" != *"SHA"* ]]; then
                        gpgkeys=$(echo "$key" | sed 's/Version.*//')
                        gpgkeys+="_GPG_keys"

                        if ! yq eval ".variables | has(\"$gpgkeys\")" $Temp_constants_FILE; then
                            echo "GPG Keys needed for $key" >&2
                            exit 1
                        fi
                    fi

                    yq eval ".variables.$key = \"$value\"" -i $constants_FILE
                    echo "Updated constants.yml with latest value $key=$value"

                    # This is for all updates (for PR description)
                    if [[ "$key" != *"SHA"* ]]; then
                        echo "$key=$value" >> "$Stack_Updated_values"
                        if [ -n $old_value ]; then
                            update_line="Updated $key from $old_value to $value"
                        else
                            update_line="Added $key to $value"
                        fi
                        echo "$update_line" >> "$Updated_ValuesFILE"
                    fi
                fi

            elif [[ "$valueInVariableGroup" = "dont_change" ]]; then
                old_value=$(yq eval ".variables.$key" $Temp_constants_FILE)
                yq eval ".variables.$key = \"$old_value\"" -i $constants_FILE
                echo "constants.yml with old value $key=$old_value"
            else
                old_value=$(yq eval ".variables.$key" $Temp_constants_FILE)
                if [ $old_value = $valueInVariableGroup ]; then
                    echo "$key has required value in constants.yml"
                    yq eval ".variables.$key = \"$valueInVariableGroup\"" -i $constants_FILE
                else
                    if [[ "$1" == "override_constants.yaml" ]]; then
                        yq eval ".variables.$key = \"$valueInVariableGroup\"" -i "$1"
                    fi

                    if [[ "$1" == "latest_stack_versions.yaml" && "$key" != *"SHA"* ]]; then
                        echo "$key=$valueInVariableGroup" >> "$Stack_Updated_values"
                    fi

                    yq eval ".variables.$key = \"$valueInVariableGroup\"" -i $constants_FILE
                    echo "Updated constants.yml with given value $key=$valueInVariableGroup"

                    # This is for all updates (for PR description)
                    if [[ "$key" != *"SHA"* ]]; then
                        if [ -n $old_value ]; then
                            update_line="Updated $key from $old_value to $value"
                        else
                            update_line="Added $key to $value"
                        fi
                        echo "$update_line" >> "$Updated_ValuesFILE"
                    fi
                fi
            fi

        else
            yq eval ".variables.$key = \"$value\"" -i $constants_FILE
            echo "Added $key = $value in constants.yml"
        fi

    done < <(yq e '.[]' "$1")
}

update_constants_file "override_constants.yaml"
update_constants_file "latest_stack_versions.yaml"

rm $Temp_constants_FILE