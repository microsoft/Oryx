AUTHOR ?= $(shell whoami)
TITLE ?= Title of my talk
DATE ?= $(shell date)

REVEALJS_URL ?= lib/reveal.js
MATHJAX_URL ?= lib/MathJax

ifdef CDNLIBS
REVEALJS_URL = https://cdnjs.cloudflare.com/ajax/libs/reveal.js/3.6.0
MATHJAX_URL = https://cdnjs.cloudflare.com/ajax/libs/mathjax/2.7.4
endif

#beige black blood league moon night README.md serif simple sky solarized white
THEME ?= beige
TRANSITION = fade

DIST_FILES ?=\
lib/MathJax/config/ \
lib/MathJax/extensions/ \
lib/MathJax/fonts/ \
lib/MathJax/jax/ \
