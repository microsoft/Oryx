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
            echo -e "\n$value" >> "$stack_versionsToBuild_FILE"
            echo "adding this version to build file"
        elif [[ "$key" = *"python"* ]]; then
            gpgkeyname="python${version}_GPG_keys"
            gpgkeysvalue=$(yq eval ".variables.$gpgkeyname" override_constants.yaml)
            echo -e "\n$value, $gpgkeysvalue" >> "$stack_versionsToBuild_FILE"
        elif [[ "$key" = *"php"* ]]; then
            gpgkeyname="php${version}_GPG_keys"
            gpgkeysvalue=$(yq eval ".variables.$gpgkeyname" override_constants.yaml)
            phpSHAName="php${version}Version_SHA"
            phpSHAValue=$(yq eval ".variables.$phpSHAName" latest_stack_versions.yaml)
            echo -e "\n$value, $phpSHAValue, $gpgkeysvalue" >> "$stack_versionsToBuild_FILE"
        fi

        sort -V "$stack_versionsToBuild_FILE" -o "$stack_versionsToBuild_FILE"
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

        update_stack_versions_to_build $versionsToBuild_FILE $version $value $key
    done
}

file="Stack_Updated_values.txt"

# Read the file line by line
while IFS='=' read -r key value; do
    # Process each key-value pair
    echo "Key: $key, Value: $value"

    update_versions_to_build $key $value
done < "$file"

