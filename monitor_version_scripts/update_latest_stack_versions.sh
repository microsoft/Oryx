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