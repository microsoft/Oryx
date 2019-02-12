// {{ AutogenDisclaimer }}

package {{ Namespace }}

{{ for Const in Constants ~}}
const {{ Const.Key }} string = "{{ Const.Value }}"
{{ end }}