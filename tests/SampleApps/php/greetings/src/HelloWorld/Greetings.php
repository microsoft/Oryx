<?php
namespace HelloWorld;

use PHP_Timer;

class Greetings {
  public static function sayHelloWorld() {
    $timer = new PHP_Timer();
    $timer->start();
    return "Hello World\n" . $timer->resourceUsage() . "\n";
  }
}
