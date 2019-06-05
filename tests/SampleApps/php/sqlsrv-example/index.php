<?php
$dbhost = getenv('SQLSERVER_DATABASE_HOST');
$dbname = getenv('SQLSERVER_DATABASE_NAME');
$pdo = new PDO("sqlsrv:server=$dbhost;Database=$dbname", getenv('SQLSERVER_DATABASE_USERNAME'), getenv('SQLSERVER_DATABASE_PASSWORD'));

$query = $pdo->query("SELECT Name FROM Products");

$rows = [];
while ($row = $query->fetch(PDO::FETCH_ASSOC))
	$rows[] = $row;

echo json_encode($rows);
