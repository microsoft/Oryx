# add_line_to_versions_to_build() {
#     update_file=$1
#     line_to_be_added=$2
    
#     awk -v line="$line_to_be_added" '
#     BEGIN { added = 0 }
#     /^[[:space:]]*$/ { print; next }  # Skip empty lines
#     /^#/ { print; next }              # Skip comment lines
#     {
#         split(line, new_fields, ", ")
#         split($0, fields, ", ")

#         # Use sort -V to compare versions
#         cmd = "echo \"" fields[1] "\n" new_fields[1] "\" | sort -V | head -n 1"
#         cmd | getline result
#         close(cmd)

#         if (!added && result == new_fields[1]) {
#                 print line
#                 added = 1
#         }
#         print
#     }
#     END {
#         if (!added) print line
#     }' "$update_file" > temp && mv temp "$update_file"
# }

sort_versions_to_build_file(){
    versionsToBuild_FILE="$1"
    tempfile1=$(mktemp)
    tempfile2=$(mktemp)

    while IFS= read -r line; do
        if [[ "$line" = *"#"* ]]; then
            echo "$line" >> "$tempfile1"
        elif [[ -n "$line" ]]; then
            echo "$line" >> "$tempfile2"
        fi
    done < "$versionsToBuild_FILE"

    sort -V $tempfile2 -o $tempfile2

    cat $tempfile2 >> $tempfile1

    cp $tempfile1 $versionsToBuild_FILE
}

update_stack_versions_to_build(){
    stack_versionsToBuild_FILE="$1"
    version="$2"
    value="$3"
    key="$4"

    echo "$version"
    echo "$value"

    version_found=false
    while IFS= read -r line; do
        if [[ "$line" == *"$value"* ]]; then
            version_found=true
            echo "version is found"
            break
        fi
    done < "$stack_versionsToBuild_FILE"

    if ! $version_found; then
        if [[ "$key" = *"node"* ]]; then
            # Check if the last line is empty
            if [ -n "$(tail -c 1 "$stack_versionsToBuild_FILE")" ]; then
                echo "" >> "$stack_versionsToBuild_FILE"
            fi
            echo -n "$value" >> "$stack_versionsToBuild_FILE"
            sort_versions_to_build_file "$stack_versionsToBuild_FILE"
        elif [[ "$key" = *"python"* ]]; then
            gpgkeyname="python${version}_GPG_keys"
            gpgkeysvalue=$(yq eval ".variables.$gpgkeyname" override_constants.yaml)
            if [ -n "$(tail -c 1 "$stack_versionsToBuild_FILE")" ]; then
                echo "" >> "$stack_versionsToBuild_FILE"
            fi
            echo -n "$value, $gpgkeysvalue," >> "$stack_versionsToBuild_FILE"
            sort_versions_to_build_file "$stack_versionsToBuild_FILE"
        elif [[ "$key" = *"php"* ]]; then
            gpgkeyname="php${version}_GPG_keys"
            gpgkeysvalue=$(yq eval ".variables.$gpgkeyname" override_constants.yaml)
            phpSHAName="php${version}Version_SHA"
            phpSHAValue=$(yq eval ".variables.$phpSHAName" latest_stack_versions.yaml)
            if [ -n "$(tail -c 1 "$stack_versionsToBuild_FILE")" ]; then
                echo "" >> "$stack_versionsToBuild_FILE"
            fi
            echo -n "$value, $phpSHAValue, $gpgkeysvalue," >> "$stack_versionsToBuild_FILE"
            sort_versions_to_build_file "$stack_versionsToBuild_FILE"
        fi
    fi
}

update_versions_to_build() {
    key="$1"
    value="$2"
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
    echo "The one which needs to be searched is $debianFlavors"
    alldebianFlavors=$(yq eval ".variables.$debianFlavors" override_constants.yaml)
    echo "$alldebianFlavors"

    IFS=','
    for flavor in $alldebianFlavors; do
        echo "$flavor"
        versionsToBuild_FILE="$versionsToBuild_Folder/$flavor/versionsToBuild.txt"

        if [[ "$key" == *"NET"* ]]; then
            while IFS= read -r line; do
                echo "Sdk_version line is $line"
                if [[ "$line" == *"$value"* ]]; then
                    sdk_version=$(echo $line | cut -d':' -f2- | tr -d '\n')
                    echo "after processing sdk_version is $sdk_version"
                    version_found=false
                    while IFS= read -r line_in_versionsToBuild; do
                        if [[ "$line_in_versionsToBuild" == *"$sdk_version"* ]]; then
                            version_found=true
                        fi
                    done < "$versionsToBuild_FILE"

                    if ! $version_found; then
                        if [ -n "$(tail -c 1 "$versionsToBuild_FILE")" ]; then
                            echo "" >> "$versionsToBuild_FILE"
                        fi
                        echo -n "$sdk_version" >> "$versionsToBuild_FILE"
                        sort_versions_to_build_file "$stack_versionsToBuild_FILE"
                    fi
                fi
            done < "generated_files/dotnet_sdk_latest_versions.txt"            
        else
            update_stack_versions_to_build $versionsToBuild_FILE $version $value $key

            sort -V $versionsToBuild_FILE -o $versionsToBuild_FILE
        fi
    done
}

file="Stack_Updated_values.txt"

# Read the file line by line
while IFS='=' read -r key value; do
    # Process each key-value pair
    echo "Key: $key, Value: $value"

    update_versions_to_build $key $value
done < "$file"

