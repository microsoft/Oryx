<?php

// Autoload files using Composer autoload
require_once __DIR__ . '/../vendor/autoload.php';

use HelloWorld\Greetings;

echo Greetings::sayHelloWorld();