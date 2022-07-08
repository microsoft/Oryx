<?php

// Autoload files using Composer autoload
require_once __DIR__ . '/../vendor/autoload.php';

use Fpm\PhpFpmConfig;

echo PhpFpmConfig::outputFpmConfig();