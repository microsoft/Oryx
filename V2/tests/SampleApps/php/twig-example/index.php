<?php
require_once __DIR__ . '/vendor/autoload.php';

$twig = new Twig_Environment(new Twig_Loader_Filesystem(__DIR__ . '/templates'));

echo $twig->render('home.html', [
    'page_title' => 'Home',
    'page_heading' => 'Hello World!'
]);
