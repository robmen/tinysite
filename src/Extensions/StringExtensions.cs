
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
    }
}
