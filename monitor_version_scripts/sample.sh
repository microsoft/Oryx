# #!/bin/bash

sort_versions_to_build_file(){
    versionsToBuild_File="$1"
    tempfile1=$(mktemp)
    tempfile2=$(mktemp)

    while IFS= read -r line; do
        if [[ "$line" = *"#"* ]]; then
            echo "$line" >> "$tempfile1"
        elif [[ -n "$line" ]]; then
            echo "$line" >> "$tempfile2"
        fi
    done < "$versionsToBuild_File"

    sort -V $tempfile2 -o $tempfile2

    cat $tempfile2 >> $tempfile1

    cp $tempfile1 $versionsToBuild_File
}
versionsToBuild_File=$(cd .. && pwd)/platforms/python/versions/bookworm/versionsToBuild.txt
sort_versions_to_build_file "$versionsToBuild_File"