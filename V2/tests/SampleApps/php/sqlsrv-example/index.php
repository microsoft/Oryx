<?php
$dbhost = getenv('SQLSERVER_DATABASE_HOST');
$dbname = getenv('SQLSERVER_DATABASE_NAME');
$user = getenv('SQLSERVER_DATABASE_USERNAME');
$pwd = getenv('SQLSERVER_DATABASE_PASSWORD');

try {
	$pdo = new PDO("sqlsrv:server=$dbhost;Database=$dbname", $user, $pwd);
	$pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);

	$query = $pdo->query("SELECT Name FROM Products");

	$rows = [];
	while ($row = $query->fetch(PDO::FETCH_ASSOC))
		$rows[] = $row;

	echo json_encode($rows);
}
catch (PDOException $e) {
	print("Error connecting to SQL Server.");
	die(print_r($e->getMessage()));
}
