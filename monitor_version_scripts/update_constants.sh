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
            yq eval ".variables.$key = \"$value\"" -i $constants_FILE
            echo "Updated constants.yml with latest value $key=$value"
        elif [ "$valueInVariableGroup" = "dont_change" ]; then
            old_value = $(yq eval ".$key" $Temp_constants_FILE)
            yq eval ".variables.$key = \"$old_value\"" -i $constants_FILE
            echo "constants.yml with old value $key=$old_value"
        else
            yq eval ".variables.$key = \"$valueInVariableGroup\"" -i $constants_FILE
            echo "Updated constants.yml with given value $key=$valueInVariableGroup"
        fi

    else
        yq eval ".variables.$key = \"$value\"" -i $constants_FILE
        echo "Added $key = $value in constants.yml"
    fi

done < <(yq e '.[]' "override_constants.yaml")

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
            yq eval ".variables.$key = \"$value\"" -i $constants_FILE
            echo "Updated constants.yml with latest value $key=$value"
        elif [ "$valueInVariableGroup" = "dont_change" ]; then
            old_value = $(yq eval ".$key" $Temp_constants_FILE)
            yq eval ".variables.$key = \"$old_value\"" -i $constants_FILE
            echo "constants.yml with old value $key=$old_value"
        else
            yq eval ".variables.$key = \"$valueInVariableGroup\"" -i $constants_FILE
            echo "Updated constants.yml with given value $key=$valueInVariableGroup"
        fi

    else
        yq eval ".variables.$key = \"$value\"" -i $constants_FILE
        echo "Added $key = $value in constants.yml"
    fi

  
done < <(yq e '.[]' "latest_stack_versions.yaml")

