<?php
$dbhost = getenv('DATABASE_HOST');
$dbuser = getenv('DATABASE_USERNAME');
$dbpass = getenv('DATABASE_PASSWORD');
$dbname = getenv('DATABASE_NAME');
$db = pg_connect("host=$dbhost dbname=$dbname user=$dbuser password=$dbpass connect_timeout=5");
if (!$db)
	die('Connection error');

$query = pg_query($db, 'SELECT Name FROM Products');
if (!$query)
	die('Query error');

$rows = [];
while ($row = pg_fetch_assoc($query))
	$rows[] = ['Name' => $row['name']];

pg_free_result($query);
pg_close($db);

echo json_encode($rows);
