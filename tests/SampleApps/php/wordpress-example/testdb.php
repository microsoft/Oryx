<?php
$link = mysqli_connect('127.0.0.1', 'wordpressdb', 'Wordpress@123', 'wordpressdb');
if (!$link) {
	echo "Unable to connect to MySQL." . PHP_EOL;
    echo nl2br("Error no: ") . mysqli_connect_errno() . " - " . mysqli_connect_error(). PHP_EOL;
	die(".");
}
echo 'Connected successfully';
mysqli_close($link);
?>
