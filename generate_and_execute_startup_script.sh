#!/bin/bash

oryxArgs="create-script -appPath $appPath -output $startupCommandPath -defaultAppFilePath $defaultAppPath \
    -bindPort $PORT -bindPort2 '$HTTP20_ONLY_PORT' -userStartupCommand '$userStartupCommand' $runFromPathArg"

echo "Running oryx $oryxArgs"
eval oryx $oryxArgs
exec $startupCommandPath
