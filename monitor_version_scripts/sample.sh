# #!/bin/bash

while IFS= read -r line; do
    echo "Sdk_version line is $line"
    if [[ "$line" = *"$value"* ]]; then
        sdk_version=$(echo $line | cut -d':' -f2- | tr -d '\n')
        echo "after processing sdk_version is $sdk_version"
        # version_found=false
        # while IFS= read -r line_in_versionsToBuild; do
        #     if [[ "$line_in_versionsToBuild" = *"$sdk_version"* ]]; then
        #         version_found=true
        #     fi
        # done < "$versionsToBuild_FILE"

        # if ! $version_found; then
        #     if [ -n "$(tail -c 1 "$versionsToBuild_FILE")" ]; then
        #         echo "" >> "$versionsToBuild_FILE"
        #     fi
        #     echo -n "$sdk_version" >> "$versionsToBuild_FILE"
        #     updated_files+=("$versionsToBuild_FILE")
        #     # sort_versions_to_build_file $versionsToBuild_FILE
        # fi
    fi
done < "generated_files/dotnet_sdk_latest_versions.txt" 