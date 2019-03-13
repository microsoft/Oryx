<?php
$mysqli = new mysqli(getenv('DATABASE_HOST'), getenv('DATABASE_USERNAME'), getenv('DATABASE_PASSWORD'), getenv('DATABASE_NANE'));
if (mysqli_connect_errno())
{
	printf("Connect failed: %s\n", mysqli_connect_error());
	exit();
}

$rows = array();
if ($result = $mysqli->query("SELECT Name FROM Products"))
{
	while ($row = $result->fetch_assoc())
		$rows[] = $row;

	$result->free();
}

$mysqli->close();

echo json_encode($rows);
