# Reveal.js template

This template only uses very simple tools to set-up a presentation:

* Make
* m4
* sed
* [pandoc](http://pandoc.org/installing.html) (optional for latex conversion)

I know, you want to see a preview, [here it is](
https://alejandrogallo.github.io/reveal-template/
).

## Features

* Write in markdown, html or latex, which means that is also done with
  people writing formulas in mind.
* Automatic conversion from all formats into html.
* Building just by hitting `make`.
* You do not need an internet connection to use the slides, since
  all libraries are linked locally.
* The revealjs template is a simple m4 script (`template/revealjs.m4`)

## Getting started

Pandoc is only needed for latex conversion, if you need it
install [pandoc](http://pandoc.org/installing.html), on ubuntu/debian

```
sudo apt-get install pandoc
```

Clone repository

```
git clone --recursive https://github.com/alejandrogallo/reveal-template
```

and make!

```
make
```

Write your slides in `slides/` in markdown, html or latex and add the
slides in the `main.sed` file with the `html` extension.

The makefile will convert (if necessary) your slides into html and
will create an `index.html` file.
