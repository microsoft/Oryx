include config.mk

.PHONY: all run clean

SLIDES = $(shell grep -E -h -o 'slides/.*' main.sed)

$(info )
$(info Slides)
$(info =======)
$(info $(SLIDES))
$(info )

all: index.html

run: index.html
	python -m SimpleHTTPServer 8080

main.html: main.sed $(SLIDES) Makefile
	echo | sed -f main.sed > $@

%.html: %.md
	./template/markdown-to-html.sh "$<" > $@

dist: index.html
	mkdir -p $@
	cp $< $@/
	{\
		grep 'mathjax *:\|src *[=:]\|href *=' $< ; \
		test -n "$(DIST_FILES)" && echo $(DIST_FILES) | tr ' ' '\n' || echo; \
	} | \
	tr '"'"'" '\n' | \
	cat - | \
	while read line; do \
		test -e "$$line" && { \
			echo "Copying $$line" ; \
			mkdir -p $@/$$(dirname  "$$line"); \
			cp -r "$$line" $@/$$(dirname  "$$line"); \
		} || echo -n ;\
	done

index.html: main.html
	m4 \
		-D __author__="$(AUTHOR)" \
		-D __title__="$(TITLE)" \
		-D __date__="$(DATE)" \
		-D __revealjs_url__="$(REVEALJS_URL)" \
		-D __theme__=$(THEME) \
		-D __mathjax__="$(MATHJAX_URL)" \
		-D __transition__=$(TRANSITION) \
		template/revealjs.m4 \
		> $@


.FORCE:
gh-pages:
	$(MAKE) CDNLIBS=1
	mv index.html gh-pages.html
	git checkout gh-pages
	mv gh-pages.html index.html
	git add index.html
	git commit -m Update

clean:
	-rm -f index.html
	-rm -f main.html
	-rm -rf dist
