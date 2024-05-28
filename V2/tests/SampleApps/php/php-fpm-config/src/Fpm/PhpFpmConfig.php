<?php
namespace Fpm;

use PHP_Timer;

class PhpFpmConfig {
  public static function outputFpmConfig() {
    print_r("Hello World!\n");
    exec("php-fpm -tt 2>&1", $lines);
    print_r($lines);
  }
}
