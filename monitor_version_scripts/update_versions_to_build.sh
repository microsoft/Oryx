sort_versions_to_build_file(){
    versionsToBuild_File="$1"
    tempfile1=$(mktemp)
    tempfile2=$(mktemp)

    while IFS= read -r line || [[ -n "$line" ]]; do
        if [[ "$line" = *"#"* ]]; then
            echo "$line" >> "$tempfile1"
        elif [[ "$line" =~ [^[:space:]] ]]; then
            echo "$line" >> "$tempfile2"
        fi
    done < "$versionsToBuild_File"

    sort -V $tempfile2 -o $tempfile2

    cat $tempfile2 >> $tempfile1
    cp $tempfile1 $versionsToBuild_File
}

update_stack_versions_to_build(){
    versionsToBuild_FILE="$1"
    version="$2"
    value="$3"
    key="$4"

    version_found=false
    while IFS= read -r line; do
        if [[ "$line" == *"$value"* ]]; then
            version_found=true
            echo "version already exists in sdks"
            break
        fi
    done < "$versionsToBuild_FILE"

    if ! $version_found; then
        if [[ "$key" == *"node"* ]]; then
            # Check if the last line is empty
            if [ -n "$(tail -c 1 "$versionsToBuild_FILE")" ]; then
                echo "" >> "$versionsToBuild_FILE"
            fi
            echo -n "$value" >> "$versionsToBuild_FILE"
            updated_files+=("$versionsToBuild_FILE")
            # sort_versions_to_build_file "$versionsToBuild_FILE"
        elif [[ "$key" == *"python"* ]]; then
            gpgkeyname="python${version}_GPG_keys"
            gpgkeysvalue=$(yq eval ".variables.$gpgkeyname" override_constants.yaml)
            if [ -n "$(tail -c 1 "$versionsToBuild_FILE")" ]; then
                echo "" >> "$versionsToBuild_FILE"
            fi
            echo -n "$value, $gpgkeysvalue," >> "$versionsToBuild_FILE"
            updated_files+=("$versionsToBuild_FILE")
            # sort_versions_to_build_file "$versionsToBuild_FILE"
        elif [[ "$key" == *"php"* ]]; then
            gpgkeyname="php${version}_GPG_keys"
            gpgkeysvalue=$(yq eval ".variables.$gpgkeyname" override_constants.yaml)
            phpSHAName="php${version}Version_SHA"
            phpSHAValue=$(yq eval ".variables.$phpSHAName" latest_stack_versions.yaml)
            if [ -n "$(tail -c 1 "$versionsToBuild_FILE")" ]; then
                echo "" >> "$versionsToBuild_FILE"
            fi
            echo -n "$value, $phpSHAValue, $gpgkeysvalue," >> "$versionsToBuild_FILE"
            updated_files+=("$versionsToBuild_FILE")
            # sort_versions_to_build_file "$versionsToBuild_FILE"
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

        if [[ "$key" = *"NET"* ]]; then
            while IFS= read -r line; do
                echo "Sdk_version line is $line"
                if [[ "$line" = *"$value"* ]]; then
                    sdk_version=$(echo "$line" | sed 's/^[^:]*://')
                    echo "after processing sdk_version is $sdk_version"
                    version_found=false
                    while IFS= read -r line_in_versionsToBuild || [[ -n "$line_in_versionsToBuild" ]]; do
                        if [[ "$line_in_versionsToBuild" = *"$sdk_version"* ]]; then
                            version_found=true
                        fi
                    done < "$versionsToBuild_FILE"

                    if ! $version_found; then
                        if [ -n "$(tail -c 1 "$versionsToBuild_FILE")" ]; then
                            echo "" >> "$versionsToBuild_FILE"
                        fi
                        echo "$sdk_version" >> "$versionsToBuild_FILE"
                        updated_files+=("$versionsToBuild_FILE")
                    fi
                fi
            done < "generated_files/dotnet_sdk_latest_versions.txt"  
        else
            update_stack_versions_to_build $versionsToBuild_FILE $version $value $key
        fi
    done
}

file="Stack_Updated_values.txt"
# for sorting the updated files later
updated_files=()

# Read the file line by line
while IFS='=' read -r key value; do
    # Process each key-value pair
    echo "Key: $key, Value: $value"

    update_versions_to_build $key $value
done < "$file"

for element in "${updated_files[@]}"; do
    sort_versions_to_build_file "$element"
done