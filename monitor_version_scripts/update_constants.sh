#!/bin/bash

# Read the YAML file line by line

constants_FILE=$(cd .. && pwd)/images/constants.yml

echo $constants_FILE

while IFS= read -r line; do
    # Use yq to parse the YAML line and extract the key and value
    key=$(echo "$line" | yq e 'keys' - | sed 's/^[[:space:]]*-*//' | sed 's/^[[:space:]]*//')

    value=$(echo "$line" | yq e '.[]' -)

    echo "Key: $key, Value: $value,"

    echo "$key"

    if yq eval ".variables | has(\"$key\")" $constants_FILE | grep -q 'true'; then
        yq eval ".variables.$key = \"$value\"" -i $constants_FILE
        echo "Updated $key in constants_FILE"
    else
        yq eval ".variables.$key = \"$value\"" -i $constants_FILE
        echo "Added $key to constants_FILE"
    fi
  
done < <(yq e '.[]' "override_constants.yaml")

while IFS= read -r line; do
    # Use yq to parse the YAML line and extract the key and value
    key=$(echo "$line" | yq e 'keys' - | sed 's/^[[:space:]]*-*//' | sed 's/^[[:space:]]*//')
    value=$(echo "$line" | yq e '.[]' -)

    echo "Key: $key, Value: $value,"

    if yq eval ".variables | has(\"$key\")" $constants_FILE | grep -q 'true'; then
        if yq eval ".variables | has(\"$key\") | not" override_constants.yaml | grep -q 'true'; then
            yq eval ".variables.$key = \"$value\"" -i $constants_FILE
        fi
    else
        yq eval ".variables.$key = \"$value\"" -i $constants_FILE
        echo "Added $key to constants_FILE"
    fi
  
done < <(yq e '.[]' "latest_stack_versions.yaml")

