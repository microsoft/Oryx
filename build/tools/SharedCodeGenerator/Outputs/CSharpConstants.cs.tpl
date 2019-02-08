// {{ AutogenDisclaimer }}

namespace {{ Namespace }}
{
	public static class {{ Name }}
	{
		{{~ for Const in Constants ~}}
		public const string {{ Const.Key }} = "{{ Const.Value }}";
		{{~ end ~}}
	}
}