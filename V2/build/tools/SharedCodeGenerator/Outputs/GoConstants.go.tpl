{{ Header }}

package {{ Namespace }}

{{ for Const in StringConstants ~}}
const {{ Const.Key }} string = "{{ Const.Value }}"
{{ end }}