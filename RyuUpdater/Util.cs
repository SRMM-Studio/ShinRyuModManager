using System;
using System.IO.Compression;
using System.IO;

namespace RyuUpdater
{
    public static class Util
    {
        /// <summary>
        /// Compares two versions and returns true if the target version is higher than the current one.
        /// </summary>
        /// <param name="versionTarget">Target version.</param>
        /// <param name="versionCurrent">Current version to compare against.</param>
        /// <returns>A boolean.</returns>
        internal static bool CompareVersionIsHigher(string versionTarget, string versionCurrent)
        {
            Version v1 = new Version(versionTarget);
            Version v2 = new Version(versionCurrent);
            switch (v1.CompareTo(v2))
            {
                case 0: //same
                    return false;

                case 1: //target is higher
                    return true;

                case -1: //target is lower
                    return false;

                default:
                    return false;
            }
        }


        //https://stackoverflow.com/a/14795752
        public static void ExtractToDirectory(this ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }

            DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
            string destinationDirectoryFullPath = di.FullName;

            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, file.FullName));

                if (!completeFileName.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                {
                    throw new IOException("Trying to extract file outside of destination directory. See this link for more info: https://snyk.io/research/zip-slip-vulnerability");
                }

                if (file.Name == "")
                {// Assuming Empty for Directory
                    Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                    continue;
                }
                file.ExtractToFile(completeFileName, true);
            }
        }
    }
}
