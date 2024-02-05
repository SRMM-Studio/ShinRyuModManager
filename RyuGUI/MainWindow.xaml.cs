using ModLoadOrder.Mods;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ICSharpCode.SharpZipLib.Zip;
using System.Windows.Controls;
using Utils;
using YamlDotNet.Serialization;
using System;
using System.Windows.Media.Imaging;
using YamlDotNet.Core;
using System.Threading;
using System.Reflection;

namespace RyuGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<ModInfo> ModList { get; set; }
        private FileSystemWatcher modsFolderWatcher;


        public MainWindow()
        {
            InitializeComponent();
            lbl_SRMMVersion.Content = $"v{Util.GetAppVersion()}";

            this.Title = $"Shin Ryu Mod Manager [{Utils.GamePath.GetGameFriendlyName(Utils.GamePath.GetGame())}]";

            if (!Directory.Exists("mods"))
                Directory.CreateDirectory("mods");

            modsFolderWatcher = new FileSystemWatcher("mods");
            modsFolderWatcher.Created += (o, args) => { Dispatcher.Invoke(() => Refresh()); };
            modsFolderWatcher.Deleted += (o, args) => { Dispatcher.Invoke(() => Refresh()); };
            modsFolderWatcher.Renamed += (o, args) => { Dispatcher.Invoke(() => Refresh()); };
            modsFolderWatcher.EnableRaisingEvents = true;

            //Display changelog if the recent update flag exists
            string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string updateFlagFilePath = Path.Combine(currentPath, "SRMM_RECENT_UPDATE_FLAG");
            if (File.Exists(updateFlagFilePath))
            {
                ChangelogWindow changelog = new ChangelogWindow();
                changelog.Show();
                File.Delete(updateFlagFilePath);
            }
        }


        public void SetupModList(List<ModInfo> mods)
        {
            this.ModList = new ObservableCollection<ModInfo>(mods);
        }

        private bool TryInstallModZip(string path)
        {
            if (!File.Exists(path))
                return false;

            using (ZipFile zip = new ZipFile(path))
            {
                ZipEntry[] files = zip.Cast<ZipEntry>().ToArray();

                ZipEntry[] rootdirs = files.Where(x => x.IsDirectory && x.ToString().Split('/').Length <= 2).ToArray();

                if (rootdirs.Length <= 0)
                    return false;

                foreach (ZipEntry entry in files)
                {
                    if (entry.IsDirectory)
                        Directory.CreateDirectory(Path.Combine("mods", entry.ToString()));
                    else
                    {
                        using (FileStream outputFile = File.Create(Path.Combine("mods", entry.ToString())))
                        {
                            if (entry.Size > 0)
                            {
                                Stream fileStream = zip.GetInputStream(entry);
                                fileStream.CopyTo(outputFile);
                                fileStream.Close();
                            }
                        }
                    }
                }
            }
            return true;
        }

        private void ModToggle_Click(object sender, RoutedEventArgs e)
        {
            foreach (ModInfo m in this.ModListView.SelectedItems)
            {
                m.ToggleEnabled();
            }
        }


        private void ModUp_Click(object sender, RoutedEventArgs e)
        {
            List<ModInfo> selection = new List<ModInfo>(this.ModListView.SelectedItems.Cast<ModInfo>());

            int limit = 0;
            foreach (int i in selection.Select(t => this.ModList.IndexOf(t)).OrderBy(x => x))
            {
                if (i > limit)
                {
                    ModInfo temp = this.ModList[i - 1];
                    this.ModList[i - 1] = this.ModList[i];
                    this.ModList[i] = temp;
                }
                else
                {
                    ++limit;
                }
            }

            // Restore selection
            foreach (ModInfo m in selection)
            {
                this.ModListView.SelectedItems.Add(m);
            }
        }


        private void ModDown_Click(object sender, RoutedEventArgs e)
        {
            List<ModInfo> selection = new List<ModInfo>(this.ModListView.SelectedItems.Cast<ModInfo>());

            int limit = this.ModList.Count - 1;
            foreach (int i in selection.Select(t => this.ModList.IndexOf(t)).OrderByDescending(x => x))
            {
                if (i < limit)
                {
                    ModInfo temp = this.ModList[i + 1];
                    this.ModList[i + 1] = this.ModList[i];
                    this.ModList[i] = temp;
                }
                else
                {
                    --limit;
                }
            }

            // Restore selection
            foreach (ModInfo m in selection)
            {
                this.ModListView.SelectedItems.Add(m);
            }
        }


        private void ModSave_Click(object sender, RoutedEventArgs e)
        {
            if (RyuHelpers.Program.SaveModList(this.ModList.ToList()))
            {
                // Run generation only if it will not be run on game launch (i.e. if RebuildMLO is disabled or unsupported)
                if (RyuHelpers.Program.RebuildMLO && RyuHelpers.Program.IsRebuildMLOSupported)
                {
                    MessageBox.Show("Mod list was saved. Mods will be applied next time the game is run.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Open MessageBox on a separate thread
                    Task.Run(() => MessageBox.Show("Applying mods. Please wait...", "Info", MessageBoxButton.OK, MessageBoxImage.Information));

                    // Wait for the generation to finish
                    bool success;
                    try
                    {
                        Task<bool> gen = RyuHelpers.Program.RunGeneration(RyuHelpers.Program.ConvertNewToOldModList(this.ModList.ToList()));
                        gen.Wait();
                        success = gen.Result;
                    }
                    catch
                    {
                        success = false;
                    }

                    if (success)
                    {
                        MessageBox.Show("Mod list was saved and mods have been applied.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(
                            "Mods could not be applied. Please make sure that the game directory has write access. " +
                            "\n\nRun Ryu Mod Manager in command line mode (use --cli parameter) for more info.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Mod list is empty and was not saved.", "Info", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            Application.Current.Shutdown();
        }


        private void ModClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


        private void ModInstall_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists("mods"))
                Directory.CreateDirectory("mods");

            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.DefaultExt = "*.zip";
            openFileDialog.Filter = "ZIP files (.zip)|*.zip";

            if (!openFileDialog.ShowDialog().Value)
                return;

            if (!File.Exists(openFileDialog.FileName))
                return;

            if (TryInstallModZip(openFileDialog.FileName))
                Refresh();
        }


        private void ModUninstall_Click(object sender, RoutedEventArgs e)
        {
            //TODO
        }


        private void ModListViewRefresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }


        private void Refresh()
        {
            SetupModList(RyuHelpers.Program.PreRun());
            ModListView.ItemsSource = ModList;
        }


        private void ModListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            ModInfo selected = lv.SelectedItem as ModInfo;
            if (selected == null) return;
            string modPath = Path.Combine(GamePath.GetModsPath(), selected.Name);
            UpdateModMeta(selected.Name, modPath);
        }


        private void UpdateModMeta(string modName, string modPath)
        {
            string pathModMeta = Path.Combine(modPath, "mod-meta.yaml");
            string patternModImage = "mod-image.*";
            List<string> matchingModImageFiles = Directory.EnumerateFiles(modPath, patternModImage).ToList();
            BitmapImage modImage = new BitmapImage(new Uri("pack://application:,,,/Resources/NoImage.png"));

            try
            {
                ModMeta meta = ModMeta.GetPlaceholderModMeta(modName);

                if (File.Exists(pathModMeta))
                {
                    string yamlString = File.ReadAllText(pathModMeta, System.Text.Encoding.UTF8);
                    var deserializer = new DeserializerBuilder().Build();
                    meta = deserializer.Deserialize<ModMeta>(yamlString);
                }

                lbl_ModName.Text = meta.Name;
                lbl_ModAuthor.Content = meta.Author;
                lbl_ModVersion.Content = meta.Version;
                sp_ModDescription.Children.Clear();
                TextBlock tb = new TextBlock();
                tb.TextWrapping = TextWrapping.Wrap;
                tb.Text = meta.Description;
                sp_ModDescription.Children.Add(tb);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error has occurred while trying to load mod-meta. \nThe exception message is:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }


            if (matchingModImageFiles.Count > 0)
            {
                foreach (string filePath in matchingModImageFiles)
                {
                    try
                    {
                        var uri = new Uri($"file://{filePath}");
                        var bitmap = Util.OpenBitmapImage(uri);
                        
                        //bitmap manages to load
                        modImage = bitmap;
                        break;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error has occurred while trying to load {Path.GetFileName(filePath)}. \nThe exception message is:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            img_ModImage.Source = modImage;
        }


        private void mi_AboutSRMM_Click(object sender, RoutedEventArgs e)
        {
            Window window = new AboutWindow();
            window.Show();
        }


        private void mi_Changelog_Click(object sender, RoutedEventArgs e)
        {
            Window window = new ChangelogWindow();
            window.Show();
        }


        private void mi_CheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            new Thread(delegate () {
                Program.CheckForUpdatesGUI(true);
            }).Start();
        }


        private void mi_ModMetaSampleYAML_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.FileName = "mod-meta";
            saveFileDialog.DefaultExt = ".yaml";
            saveFileDialog.Filter = "YAML Files (.yaml)|*.yaml";
            saveFileDialog.InitialDirectory = GamePath.GetModsPath();
            Nullable<bool> result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                ModMeta sampleMeta = new ModMeta()
                {
                    Name = "Your mod name",
                    Author = "Author name",
                    Version = "1.0.0",
                    Description = "Mod description example.\nThis is in a new line.\n\nYou can use single (') or double (\u0022) quotes in your text.\n\nIf you want your text to be more organized in the yaml file, make sure the new line is indented (press TAB)."
                };
                var serializer = new SerializerBuilder().WithDefaultScalarStyle(ScalarStyle.Plain).Build();
                var yaml = serializer.Serialize(sampleMeta);
                File.WriteAllText(saveFileDialog.FileName, yaml, System.Text.Encoding.UTF8);
            }
        }


        private void mi_ModMetaSampleImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.FileName = "mod-image";
            saveFileDialog.DefaultExt = ".png";
            saveFileDialog.Filter = "PNG Files (.png)|*.png";
            saveFileDialog.InitialDirectory = GamePath.GetModsPath();
            Nullable<bool> result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                var uri = new Uri("pack://application:,,,/Resources/NoImage.png");
                var bitmap = new BitmapImage(uri);
                bitmap.Save(saveFileDialog.FileName);
            }
        }

        private void ModListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] dt = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string a in dt.Where(path => path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)))
                {
                    TryInstallModZip(a);
                }
                Refresh();
            }
        }
    }
}
