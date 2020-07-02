// {{ AutogenDisclaimer }}

namespace {{ Namespace }}
{
    {{ Scope }} static class {{ Name }}
    {
        {{~ for Const in Constants ~}}
        public const string {{ Const.Key }} = "{{ Const.Value }}";
        {{~ end ~}}
    }
}