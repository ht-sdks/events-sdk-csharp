using global::System.Text;

namespace Hightouch.Events.Utilities
{
    public static class ExtensionMethods
    {
        public static byte[] GetBytes(this string str) => Encoding.UTF8.GetBytes(str);
    }
}
