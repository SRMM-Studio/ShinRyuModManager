using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Utils;

namespace ShinRyuModManager.UserControls
{
    /// <summary>
    /// Interaction logic for LibraryDisplayUC.xaml
    /// </summary>
    public partial class LibraryDisplayUC : UserControl
    {
        LibMeta Meta { get; set; }

        LibMeta LocalMeta { get; set; }

        bool IsLibraryInstalled { get; set; }

        bool IsLibraryEnabled { get; set; }

        bool IsLibraryUpdateAvailable { get; set; }


        public LibraryDisplayUC(LibMeta meta)
        {
            InitializeComponent();
            Meta = meta;
            RefreshComponent();
        }


        private void RefreshComponent()
        {
            PopulateElements();
            CompareToLocalInstallation();
            UpdateButtonVisibility();
        }


        private void PopulateElements()
        {
            lbl_Name.Content = Meta.Name;
            lbl_Version.Content = Meta.Version;
            lbl_Description.Content = Meta.Description;
            lbl_Author.Content = Meta.Author;
            lbl_GUID.Content = Meta.GUID.ToString();
        }


        private void CompareToLocalInstallation()
        {
            string directoryPath = Path.Combine(GamePath.LIBRARIES, Meta.GUID.ToString());
            string metaFilePath = Path.Combine(directoryPath, Settings.LIBRARIES_LIBMETA_FILE_NAME);
            if (Directory.Exists(directoryPath))
            {
                if (File.Exists(metaFilePath))
                {
                    IsLibraryInstalled = true;

                    string yamlString = File.ReadAllText(metaFilePath);
                    LocalMeta = LibMeta.ReadLibMeta(yamlString);

                    IsLibraryEnabled = !File.Exists(Path.Combine(GamePath.GetLibrariesPath(), Meta.GUID.ToString(), ".disabled"));
                    IsLibraryUpdateAvailable = Util.CompareVersionIsHigher(Meta.Version, LocalMeta.Version);

                    if (IsLibraryUpdateAvailable)
                        lbl_Version.Content = $"{Meta.Version} (Installed: {LocalMeta.Version})";
                }
            }
            else
            {
                IsLibraryInstalled = false;
            }
        }


        private void UpdateButtonVisibility()
        {
            btn_Enable.Visibility = Visibility.Collapsed;
            btn_Disable.Visibility = Visibility.Collapsed;
            btn_Install.Visibility = Visibility.Collapsed;
            btn_Uninstall.Visibility = Visibility.Collapsed;
            btn_Update.Visibility = Visibility.Collapsed;
            btn_Source.Visibility = Visibility.Collapsed;

            if (IsLibraryInstalled)
            {
                btn_Uninstall.Visibility = Visibility.Visible;

                if (IsLibraryUpdateAvailable)
                {
                    btn_Update.Visibility = Visibility.Visible;
                }

                if (IsLibraryEnabled)
                {
                    if (Meta.CanBeDisabled) btn_Disable.Visibility = Visibility.Visible;
                }
                else
                {
                    btn_Enable.Visibility = Visibility.Visible;
                }
            }
            else
            {
                btn_Install.Visibility = Visibility.Visible;
            }

            if (Meta.Source != null && Meta.Source != string.Empty && Meta.Source.StartsWith("http"))
            {
                btn_Source.Visibility = Visibility.Visible;
            }
        }


        private string DownloadLibraryPackage(string fileName)
        {
            Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Settings.TEMP_DIRECTORY_NAME));

            try
            {
                string path = Path.Combine(Path.GetTempPath(), Settings.TEMP_DIRECTORY_NAME, fileName);
                using (var client = new WebClient())
                {
                    client.DownloadFile(Meta.Download, Path.Combine(Path.GetTempPath(), Settings.TEMP_DIRECTORY_NAME, fileName));
                    return path;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return "";
            }
        }


        private void btn_Install_Click(object sender, RoutedEventArgs e)
        {
            string packagePath = DownloadLibraryPackage($"{Meta.GUID.ToString()}.zip");
            if (packagePath != "")
            {
                using (FileStream zipToOpen = new FileStream(packagePath, FileMode.Open))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        string destinationDir = Path.Combine(GamePath.GetLibrariesPath(), Meta.GUID.ToString());
                        Directory.CreateDirectory(destinationDir);
                        archive.ExtractToDirectory(destinationDir, true);
                    }
                }
            }

            RefreshComponent();
        }


        private void btn_Uninstall_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.Combine(GamePath.LIBRARIES, Meta.GUID.ToString());
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                RefreshComponent();
            }
        }


        private void btn_Update_Click(object sender, RoutedEventArgs e)
        {
            string packagePath = DownloadLibraryPackage($"{Meta.GUID.ToString()}.zip");
            if (packagePath != "")
            {
                using (FileStream zipToOpen = new FileStream(packagePath, FileMode.Open))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        string destinationDir = Path.Combine(GamePath.GetLibrariesPath(), Meta.GUID.ToString());
                        if (Directory.Exists (destinationDir)) Directory.Delete(destinationDir, true);
                        Directory.CreateDirectory(destinationDir);
                        archive.ExtractToDirectory(destinationDir, true);
                    }
                }
            }

            RefreshComponent();
        }


        private void btn_Enable_Click(object sender, RoutedEventArgs e)
        {
            IsLibraryEnabled = true;

            // Write invisible file as flag for Parless
            string flagFilePath = Path.Combine(GamePath.GetLibrariesPath(), Meta.GUID.ToString(), ".disabled");
            if (File.Exists(flagFilePath))
            {
                File.Delete(flagFilePath);
            }

            UpdateButtonVisibility();
        }


        private void btn_Disable_Click(object sender, RoutedEventArgs e)
        {
            IsLibraryEnabled = false;

            // Write invisible file as flag for Parless
            string flagFilePath = Path.Combine(GamePath.GetLibrariesPath(), Meta.GUID.ToString(), ".disabled");
            File.Create(flagFilePath);
            File.SetAttributes(flagFilePath, File.GetAttributes(flagFilePath) | FileAttributes.Hidden);

            UpdateButtonVisibility();
        }


        private void btn_Source_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Meta.Source);
        }
    }
}
