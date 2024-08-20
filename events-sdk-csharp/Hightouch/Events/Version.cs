using System.Reflection;

namespace Hightouch.Events
{
    internal static class Version
    {
        static Version()
        {
#if NETSTANDARD2_0
            var assembly = Assembly.GetExecutingAssembly();
# else
            var assembly = typeof(Version).GetTypeInfo().Assembly;
# endif
            string version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            // strip the git commit hash from the version (eg. "1.2.3+gitsha" -> "1.2.3")
            HightouchVersion = version?.Split('+')[0] ?? "0.0.0";
        }

        internal static string HightouchVersion { get; private set; }
    }
}
