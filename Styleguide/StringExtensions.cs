namespace Forte.Styleguide
{
    public static class StringExtensions
    {
        public static string RemoveSuffix(this string s, string suffix)
        {
            return s.EndsWith(suffix) ? s.Substring(0, s.Length - suffix.Length) : s;
        }
    }
}
