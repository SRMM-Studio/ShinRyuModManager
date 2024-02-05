using System.Reflection;

namespace Utils
{
    public static class AssemblyVersion
    {
        /// <summary>
        /// Gets the calling assembly version.
        /// </summary>
        /// <returns>A <see cref="string"/>.</returns>
        public static string GetVersion()
        {
            string version = Assembly.GetCallingAssembly().GetName().Version.ToString();
            return version;
        }
    }
}
