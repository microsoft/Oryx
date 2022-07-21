<?php
namespace Fpm;

use PHP_Timer;

class PhpFpmConfig {
  public static function outputFpmConfig() {
    exec("php-fpm -tt 2>&1", $lines);
    print_r($lines);
  }
}
