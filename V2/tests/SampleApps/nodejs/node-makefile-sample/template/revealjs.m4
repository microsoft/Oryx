<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="generator" content="m4">
  <title>__title__</title>
  <meta name="apple-mobile-web-app-capable" content="yes">
  <meta name="apple-mobile-web-app-status-bar-style" content="black-translucent">
  <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no, minimal-ui">
  <link rel="stylesheet" href="__revealjs_url__/css/reveal.css">
  <link rel="stylesheet" href="__revealjs_url__/lib/css/zenburn.css">
  <style type="text/css">
      code{white-space: pre-wrap;}
      span.smallcaps{font-variant: small-caps;}
      span.underline{text-decoration: underline;}
      div.column{display: inline-block; vertical-align: top; width: 50%;}
  </style>
ifdef(
  `__theme__',
  `<link rel="stylesheet" href="__revealjs_url__/css/theme/__theme__.css" id="theme">',
  `<link rel="stylesheet" href="__revealjs_url__/css/theme/black.css" id="theme">'
)
  <!-- Printing and PDF exports -->
  <script>
    var link = document.createElement( 'link' );
    link.rel = 'stylesheet';
    link.type = 'text/css';
    link.href = window.location.search.match( /print-pdf/gi ) ? '__revealjs_url__/css/print/pdf.css' : '__revealjs_url__/css/print/paper.css';
    document.getElementsByTagName( 'head' )[0].appendChild( link );
  </script>
  <!--[if lt IE 9]>
  <script src="__revealjs_url__/lib/js/html5shiv.js"></script>
  <![endif]-->
</head>
<body>


  <div class="reveal">
    <div class="slides">

include(main.html)

    </div>
  </div>

  <script src="__revealjs_url__/lib/js/head.min.js"></script>
  <script src="__revealjs_url__/js/reveal.js"></script>

  <script>

      // Full list of configuration options available at:
      // https://github.com/hakimel/reveal.js#configuration
      Reveal.initialize({

        // Display controls in the bottom right corner
        controls: true,

        // Display a presentation progress bar
        progress: true,

        // Display the page number of the current slide
        slideNumber: true,

        // Push each slide change to the browser history
        history: true,

        // Enable keyboard shortcuts for navigation
        keyboard: true,

        // Enable the slide overview mode
        overview: true,

        // Vertical centering of slides
        center: false,

        // Enables touch navigation on devices with touch input
        touch: true,

        // Loop the presentation
        loop: false,

        // Change the presentation direction to be RTL
        rtl: false,

        // Turns fragments on and off globally
        fragments: true,

        // Flags if the presentation is running in an embedded mode,
        // i.e. contained within a limited portion of the screen
        embedded: false,

        // Flags if we should show a help overlay when the questionmark
        // key is pressed
        help: true,

        // Flags if speaker notes should be visible to all viewers
        showNotes: true,

        // Number of milliseconds between automatically proceeding to the
        // next slide, disabled when set to 0, this value can be overwritten
        // by using a data-autoslide attribute on your slides
        autoSlide: 0,

        // Stop auto-sliding after user input
        autoSlideStoppable: true,

        // Enable slide navigation via mouse wheel
        mouseWheel: false,

        // Hides the address bar on mobile devices
        hideAddressBar: true,

        // Opens links in an iframe preview overlay
        previewLinks: false,
        ifdef(
          `__transition__',
          `
        // Transition style
        transition: "__transition__",
        '
        )
        // The "normal" size of the presentation, aspect ratio will be preserved
        // when the presentation is scaled to fit different resolutions. Can be
        // specified using percentage units.
        width: '100%',
        height: '100%',

        // Factor of the display size that should remain empty around the content
        margin: 0,

        math: {
          mathjax: '__mathjax__/MathJax.js',
          config: 'TeX-AMS_HTML-full',
          tex2jax: {
            inlineMath: [['$', '$'], ['\\(','\\)']],
            displayMath: [['$$', '$$'], ['\\[','\\]']],
            balanceBraces: true,
            processEscapes: false,
            processRefs: true,
            processEnvironments: true,
            preview: 'TeX',
            skipTags: ['script','noscript','style','textarea','pre','code'],
            ignoreClass: 'tex2jax_ignore',
            processClass: 'tex2jax_process'
          },
        },

        // Optional reveal.js plugins
        dependencies: [
          { src: '__revealjs_url__/lib/js/classList.js', condition: function() { return !document.body.classList; } },
          { src: '__revealjs_url__/plugin/highlight/highlight.js', async: true, callback: function() { hljs.initHighlightingOnLoad(); } },
          { src: '__revealjs_url__/plugin/zoom-js/zoom.js', async: true },
          //{ src: '__revealjs_url__/socket.io/socker.io.js', async: true },
          //{ src: '__revealjs_url__/plugin/notes-server/client.js', async: true },
          { src: '__revealjs_url__/plugin/math/math.js', async: true },
          { src: '__revealjs_url__/plugin/notes/notes.js', async: true },

          // Interpret Markdown in <section> elements
          { src: '__revealjs_url__/plugin/markdown/marked.js', condition: function() { return !!document.querySelector( '[data-markdown]' ); } },
          { src: '__revealjs_url__/plugin/markdown/markdown.js', condition: function() { return !!document.querySelector( '[data-markdown]' ); } },

        ]
      });
    </script>

  </body>
</html>
