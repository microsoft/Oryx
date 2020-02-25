__CURRENT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
source $__CURRENT_DIR/__commonConstants.sh

function downloadFileAndVerifyChecksum() {
    local platformName="$1"
    local version="$2"
    local downloadedFileName="$3"
    local headersFile="/tmp/headers.txt"

    echo "Downloading $platformName version '$version'..."
    curl \
        -D $headersFile \
        -SL "$DEV_SDK_STORAGE_BASE_URL/$platformName/$platformName-$version.tar.gz" \
        --output $downloadedFileName

    headerName="x-ms-meta-checksum"
    checksumHeader=$(cat $headersFile | grep -i $headerName: | tr -d '\r')
    checksumHeader=${checksumHeader,,}
    checksumValue=${checksumHeader#"$headerName: "}
    echo
    echo "Verifying checksum..."
    echo "$checksumValue $downloadedFileName" | sha512sum -c -
}