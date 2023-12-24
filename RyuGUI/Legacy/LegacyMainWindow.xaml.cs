using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ModLoadOrder.Mods;

namespace RyuGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LegacyMainWindow : Window
    {
        public ObservableCollection<ModInfo> ModList { get; set; }

        public LegacyMainWindow()
        {
            InitializeComponent();
        }

        public void SetupModList(List<ModInfo> mods)
        {
            this.ModList = new ObservableCollection<ModInfo>(mods);
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (ModInfo m in this.ModListView.SelectedItems)
            {
                m.ToggleEnabled();
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
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

        private void DownButton_Click(object sender, RoutedEventArgs e)
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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
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
    }
}
