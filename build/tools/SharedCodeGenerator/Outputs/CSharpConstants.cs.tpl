{{ Header }}

namespace {{ Namespace }}
{
    {{ Scope }} static class {{ Name }}
    {
        {{~ for Const in StringConstants ~}}
        public const string {{ Const.Key }} = "{{ Const.Value }}";
        {{~ end ~}}
        {{~ for Const in ListConstants ~}}
        public static readonly List<string> {{ Const.Key }} = new List<string> {{ Const.Value }};
        {{~ end ~}}
    }
}