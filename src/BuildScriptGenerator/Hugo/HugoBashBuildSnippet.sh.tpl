echo
echo "Using Hugo version:"
$hugo version
echo

cd "$SOURCE_DIR"

if [ ! -z "$RUN_BUILD_COMMAND" ]
then
	echo 'Running $RUN_BUILD_COMMAND'
	${RUN_BUILD_COMMAND}
else
	$hugo
fi