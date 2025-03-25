using CriPakTools;
using ShinRyuModManager.UserControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using Utils;

namespace ShinRyuModManager
{
    /// <summary>
    /// Interaction logic for LibraryManagerWindow.xaml
    /// </summary>
    public partial class LibraryManagerWindow : Window
    {
        public LibraryManagerWindow()
        {
            InitializeComponent();
            PopulateLibraryList();
        }

        private List<LibMeta> DownloadLibraryData()
        {
            WebClient client = new WebClient();
            //TODO UNCOMMENT THIS WHEN THE REPO IS READY. READING FROM LOCAL FILE IN THE MEANTIME
            string yamlString = client.DownloadString($"https://raw.githubusercontent.com/{Settings.LIBRARIES_INFO_REPO_OWNER}/{Settings.LIBRARIES_INFO_REPO}/main/{Settings.LIBRARIES_INFO_REPO_FILE_PATH}");
            //string yamlString = File.ReadAllText(Program.GetLocalLibraryCopyPath());

            // Save a copy of the manifest for offline use
            string localManifestCopyPath = Program.GetLocalLibraryCopyPath();
            if (!File.Exists(localManifestCopyPath) && !Util.IsFileBlocked(localManifestCopyPath))
            {
                File.WriteAllText(localManifestCopyPath, yamlString);
            }

            return LibMeta.ReadLibMetaManifest(yamlString);
        }


        private void PopulateLibraryList()
        {
            sp_Libraries.Children.Clear();
            try
            {
                foreach (var meta in DownloadLibraryData())
                {
                    if (meta.TargetGames != null && meta.TargetGames != string.Empty)
                    {
                        string game = GamePath.GetGameExe().ToLowerInvariant().Replace(".exe", "");
                        string[] targets = meta.TargetGames.ToLowerInvariant().Replace(" ", "").Split(';');
                        if (Array.IndexOf(targets, game) != -1)
                        {
                            sp_Libraries.Children.Add(new LibraryDisplayUC(meta));
                        }
                    }
                    // No targets specified. Assume it works with everything (?)
                    else
                    {
                        sp_Libraries.Children.Add(new LibraryDisplayUC(meta));
                    }
                }
            }
            catch (Exception ex)
            {
                // Fetching library data from github failed. Connection issues or server down?
                // Populate the list with data from the already installed libraries in case the user wants to uninstall or disable any
                lbl_ManifestDownloadError.Visibility = Visibility.Visible;

                List<LibMeta> metaList = new List<LibMeta>();

                foreach (string dir in Directory.GetDirectories(GamePath.GetLibrariesPath()))
                {
                    string path = Path.Combine(dir, Settings.LIBRARIES_LIBMETA_FILE_NAME);
                    if (File.Exists(path))
                    {
                        string yamlString = File.ReadAllText(path);
                        LibMeta meta = LibMeta.ReadLibMeta(yamlString);
                        metaList.Add(meta);
                    }
                }

                foreach(var meta in metaList)
                {
                    sp_Libraries.Children.Add(new LibraryDisplayUC(meta));
                }
            }
        }
    }
}
