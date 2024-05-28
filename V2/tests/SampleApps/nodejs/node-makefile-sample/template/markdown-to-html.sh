#! /usr/bin/env bash

cat <<EOF
<section
  data-separator='^---'
  data-separator-vertical='^--'
  data-markdown>
  <textarea data-template>
EOF

cat $1

cat <<EOF
  </textarea>
</section>
EOF
