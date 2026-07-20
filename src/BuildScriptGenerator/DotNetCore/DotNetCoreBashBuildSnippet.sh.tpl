echo
dotnetCoreVersion=$(dotnet --version)
echo "Using .NET Core SDK Version: $dotnetCoreVersion"

{{ if InstallBlazorWebAssemblyAOTWorkloadCommand | IsNotBlank }}
    echo
    echo "Running '{{ InstallBlazorWebAssemblyAOTWorkloadCommand }}'..."
    echo
    START_TIME=$SECONDS
    {{ InstallBlazorWebAssemblyAOTWorkloadCommand }}
    ELAPSED_TIME=$(($SECONDS - $START_TIME))
    echo "Installing Blazor WebAssembly AOT workload done in $ELAPSED_TIME sec(s)."
{{ end }}

{{ if CustomBuildCommand | IsNotBlank }}
echo
echo "Running custom build command '{{ CustomBuildCommand }}'..."
echo "Note: the custom build command must output to \$DESTINATION_DIR (e.g., dotnet publish -c Release -o \$DESTINATION_DIR)."
echo
START_TIME=$SECONDS
{{ CustomBuildCommand }}
ELAPSED_TIME=$(($SECONDS - $START_TIME))
echo "Custom build command done in $ELAPSED_TIME sec(s)."
{{ else }}
doc="https://docs.microsoft.com/en-us/azure/app-service/configure-language-dotnetcore?pivots=platform-linux"
suggestion="Please build your app locally before publishing." 
msg="${suggestion} | ${doc}"

{{ # .NET Core 1.1 based projects require restore to be run before publish }}
echo "Restoring..."
START_TIME=$SECONDS
cmd="dotnet restore \"{{ ProjectFile }}\""
LogErrorWithTryCatch "$cmd" "$msg"
ELAPSED_TIME=$(($SECONDS - $START_TIME))
echo "dotnet restore done in $ELAPSED_TIME sec(s)."

if [ "$SOURCE_DIR" == "$DESTINATION_DIR" ]
then
    echo "Publishing..."
    START_TIME=$SECONDS
    cmd="dotnet publish \"{{ ProjectFile }}\" -c {{ Configuration }}"
    LogErrorWithTryCatch "$cmd" "$msg"
    ELAPSED_TIME=$(($SECONDS - $START_TIME))
    echo "dotnet publish done in $ELAPSED_TIME sec(s)."
else
    echo
    echo "Publishing to directory $DESTINATION_DIR..."
    echo    
    START_TIME=$SECONDS
    cmd="dotnet publish \"{{ ProjectFile }}\" -c {{ Configuration }} -o $DESTINATION_DIR"
    LogErrorWithTryCatch "$cmd" "$msg"
    ELAPSED_TIME=$(($SECONDS - $START_TIME))
    echo "dotnet publish done in $ELAPSED_TIME sec(s)."

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
{{ end }}
