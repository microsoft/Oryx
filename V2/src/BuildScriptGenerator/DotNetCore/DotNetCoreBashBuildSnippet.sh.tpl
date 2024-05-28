echo
dotnetCoreVersion=$(dotnet --version)
echo "Using .NET Core SDK Version: $dotnetCoreVersion"

{{ if InstallBlazorWebAssemblyAOTWorkloadCommand | IsNotBlank }}
    echo
    echo "Running '{{ InstallBlazorWebAssemblyAOTWorkloadCommand }}'..."
    echo
    {{ InstallBlazorWebAssemblyAOTWorkloadCommand }}
{{ end }}

doc="https://docs.microsoft.com/en-us/azure/app-service/configure-language-dotnetcore?pivots=platform-linux"
suggestion="Please build your app locally before publishing." 
msg="${suggestion} | ${doc}"

{{ # .NET Core 1.1 based projects require restore to be run before publish }}
cmd="dotnet restore \"{{ ProjectFile }}\""
LogErrorWithTryCatch "$cmd" "$msg"

if [ "$SOURCE_DIR" == "$DESTINATION_DIR" ]
then
    echo "Publishing..."
    cmd="dotnet publish \"{{ ProjectFile }}\" -c {{ Configuration }}"
    LogErrorWithTryCatch "$cmd" "$msg"
else
    echo
    echo "Publishing to directory $DESTINATION_DIR..."
    echo    
    cmd="dotnet publish \"{{ ProjectFile }}\" -c {{ Configuration }} -o $DESTINATION_DIR"
    LogErrorWithTryCatch "$cmd" "$msg"

    # we copy *.csproj to destination directory so the detector can identify
    # the destination directory as a DotNet application
    # when running oryx run-script
    # 
    # 2>/dev/null || :
    # code snippet above is used to surpass cp error message & code
    # since this is needed during: oryx run-script 
    # but not during other dotnet builds
    cp ${SOURCE_DIR}/*.csproj ${DESTINATION_DIR} 2>/dev/null || :
fi
