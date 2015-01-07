
namespace TinySite.Extensions
{
    public static class StringExtensions
    {
        public static string EnsureBackslashTerminated(this string path)
        {
            return path.EnsureEndsWith(@"\");
        }

        public static string EnsureEndsWith(this string str, string append)
        {
            return str.EndsWith(append) ? str : str + append;
        }

        public static string EnsureStartsWith(this string str, string prepend)
        {
            return str != null && str.StartsWith(prepend) ? str : prepend + str;
        }
    }
}
