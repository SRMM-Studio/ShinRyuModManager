using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using Yarhl.FileSystem;
using Yarhl.IO;

namespace ShinRyuModManager
{
    internal static class Util
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


        internal static string GetAppVersion()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return version;
        }


        internal static BitmapImage OpenBitmapImage(byte[] array)
        {
            using (var ms = new MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; //Closes the stream after the bitmap is created
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }


        internal static BitmapImage OpenBitmapImage(Uri uri)
        {
            byte[] file = File.ReadAllBytes(uri.LocalPath);
            return OpenBitmapImage(file);
        }


        internal static bool CheckFlag(string flagName)
        {
            string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string flagFilePath = Path.Combine(currentPath, flagName);
            if (File.Exists(flagFilePath)) return true;
            else return false;
        }


        internal static void CreateFlag(string flagName)
        {
            if (!CheckFlag(flagName))
            {
                string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string flagFilePath = Path.Combine(currentPath, flagName);
                File.Create(flagFilePath);
                File.SetAttributes(flagFilePath, File.GetAttributes(flagFilePath) | FileAttributes.Hidden);
            }
        }


        internal static void DeleteFlag(string flagName)
        {
            if (CheckFlag(flagName))
            {
                string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string flagFilePath = Path.Combine(currentPath, flagName);
                File.Delete(flagFilePath);
            }
        }


        internal static void PlayAudio(string audioName)
        {
            var sri = Application.GetResourceStream(new Uri($"pack://application:,,,/Resources/Audio/{audioName}"));

            if (sri != null)
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(sri.Stream);
                player.Play();
                player.Dispose();
            }
        }


        internal static bool IsFileBlocked(string path)
        {
            if (!File.Exists(path))
                return false;

            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    return false;
                }
            }
            // File is in use
            catch (IOException)
            {
                return true; 
            }
            // Unable to access
            catch (UnauthorizedAccessException)
            {
                return true;
            }
        }



        // EXTENSIONS
        public static void Save(this BitmapImage image, string filePath)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }

        public static Node FromFile(string filePath, string nodeName, FileOpenMode mode)
        {
            // We need to catch if the node creation fails
            // for instance for null names, to dispose the stream.
            var format = new BinaryFormat(DataStreamFactory.FromFile(filePath, mode));
            Node node;
            try
            {
                node = new Node(nodeName, format)
                {
                    Tags = { ["FileInfo"] = new FileInfo(filePath) },
                };
            }
            catch
            {
                format.Dispose();
                throw;
            }

            return node;
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

        public static Node ReadDirectory(string dirPath, string nodeName = "")
        {
            dirPath = Path.GetFullPath(dirPath);

            if (string.IsNullOrEmpty(nodeName))
            {
                nodeName = Path.GetFileName(dirPath);
            }

            Node container = NodeFactory.CreateContainer(nodeName);
            var directoryInfo = new DirectoryInfo(dirPath);
            container.Tags["DirectoryInfo"] = directoryInfo;

            var files = directoryInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                Node fileNode = NodeFactory.FromFile(file.FullName, Yarhl.IO.FileOpenMode.Read);
                container.Add(fileNode);
            }

            var directories = directoryInfo.GetDirectories();
            foreach (DirectoryInfo directory in directories)
            {
                Node directoryNode = ReadDirectory(directory.FullName);
                container.Add(directoryNode);
            }

            return container;
        }

        public static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                string subDirName = Path.GetFileName(dir);
                string destSubDir = Path.Combine(destDir, subDirName);
                CopyDirectory(dir, destSubDir);
            }
        }
    }
}
