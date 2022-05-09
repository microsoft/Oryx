{{ Header }}

package {{ Namespace }}

{{ for Const in StringConstants ~}}
const {{ Const.Key }} string = "{{ Const.Value }}"
{{ end }}
{{ for Const in ListConstants ~}}
{{ Const.Key }} := [...]string"{{ Const.Value }}"
{{ end }}