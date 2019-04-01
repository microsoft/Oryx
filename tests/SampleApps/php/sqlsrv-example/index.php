<?php
$dbhost = getenv('DATABASE_HOST');
$dbname = getenv('DATABASE_NAME');
$pdo = new PDO("sqlsrv:server=$dbhost;Database=$dbname", getenv('DATABASE_USERNAME'), getenv('DATABASE_PASSWORD'));

$query = $pdo->query("SELECT Name FROM Products");

$rows = [];
while ($row = $query->fetch(PDO::FETCH_ASSOC))
	$rows[] = $row;

echo json_encode($rows);
