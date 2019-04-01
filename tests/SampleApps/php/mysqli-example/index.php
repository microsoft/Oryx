<?php
$mysqli = new mysqli(getenv('DATABASE_HOST'), getenv('DATABASE_USERNAME'), getenv('DATABASE_PASSWORD'), getenv('DATABASE_NAME'));
if ($mysqli->connect_error)
	die('Connection error ('. $mysqli->connect_errno .'): '. $mysqli->connect_error);

$rows = [];
if ($result = $mysqli->query('SELECT Name FROM Products'))
{
	while ($row = $result->fetch_assoc())
		$rows[] = $row;
	$result->free();
}
else echo $mysqli->error;

$mysqli->close();

echo json_encode($rows);
