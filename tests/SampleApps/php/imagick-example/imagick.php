<?php
$img = new Imagick('64x64.png');
$d = $img->getImageGeometry(); 
echo $d['width'] .'x'. $d['height'];
