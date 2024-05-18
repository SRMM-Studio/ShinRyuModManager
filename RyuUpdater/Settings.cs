using System.IO;

namespace RyuUpdater
{
    internal static class Settings
    {
        //Repository
        internal static string RepoOwner = "SRMM-Studio";
        internal static string RepoName = "srmm-version-info";
        internal static string RepoPathBranchData = "ShinRyuModManager/config.yaml";
        internal static string RepoPathManifest = "ShinRyuModManager/manifest.yaml";


        //User Agent
        internal static string UserAgent = "RyuUpdater";


        //Path
        internal static string PathTempFolder = Path.Combine(Path.GetTempPath(), "SRMM_Updater");
        internal static string PathTempFileName = "SRMM_Update.zip";
        internal static string PathTempUpdateFile = Path.Combine(PathTempFolder, PathTempFileName);


        //Flag
        internal static string RecentUpdateFlagName = "SRMM_RECENT_UPDATE_FLAG";
    }
}
