using System.Reflection;

namespace Utils;

public static class AssemblyVersion {
    /// <summary>
    /// Gets the calling assembly version.
    /// </summary>
    /// <returns>A <see cref="string"/>.</returns>
    public static string GetVersion() {
        return Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    }

    public static string GetBuildVersion() {
        var fullVersion = GetVersion();

        return fullVersion[..(fullVersion.IndexOf('-'))];
    }

    public static string GetBuildSuffix() {
        var fullVersion = GetVersion();

        return fullVersion[(fullVersion.IndexOf('-') + 1)..];
    }
}
