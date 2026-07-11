using System.Diagnostics;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Serilog;
using Serilog.Events;
using ShinRyuModManager.Helpers;
using ShinRyuModManager.ModLoadOrder.Mods;
using ShinRyuModManager.Services.ReleaseCheck;
using ShinRyuModManager.UserInterface.ViewModels;
using Utils;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using Constants = Utils.Constants;

namespace ShinRyuModManager.UserInterface.Views;

public partial class MainWindow : Window
{
    private FileSystemWatcher _modsFolderWatcher;
    private Window _childWindow;
    
    public MainWindow()
    {
        InitializeComponent();
        Log.Information("MainWindow InitializeComponent");
    }
    
    private async void Window_OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await RunPreInitAsync();
            
            var updateVersion = await ReleaseCheck.CheckForUpdate();
            
            if (updateVersion != null)
            {
                await UpdateAvailableWindow.Show(this, updateVersion);
            }
            
            Program.ReadCachedLocalLibraryData();
            
            _modsFolderWatcher = new FileSystemWatcher(GamePath.ModsPath)
            {
                EnableRaisingEvents = true
            };
            
            _modsFolderWatcher.Created += FileSystemWatcher_Created;
            _modsFolderWatcher.Deleted += FileSystemWatcher_DeletedRenamed;
            _modsFolderWatcher.Renamed += FileSystemWatcher_DeletedRenamed;
            
            RefreshModList();
            
            Log.Information("MainWindow OnLoaded");
        }
        catch
        {
            // ignored
        }
    }
    
    // Handles properly disposing of the folder watcher
    protected override void OnClosed(EventArgs e)
    {
        _modsFolderWatcher.EnableRaisingEvents = false;
        _modsFolderWatcher.Dispose();
        
        base.OnClosed(e);
    }
    
    private async void FileSystemWatcher_Created(object _, FileSystemEventArgs e)
    {
        try
        {
            if (DataContext is not MainWindowViewModel viewModel)
                return;
            
            await Dispatcher.UIThread.InvokeAsync(RefreshModList);
            await Program.InstallAllModDependenciesAsync(viewModel.ModList.ToList());
        }
        catch
        {
            // ignored
            // Prevents application crashing
        }
    }
    
    private async void FileSystemWatcher_DeletedRenamed(object _, FileSystemEventArgs e)
    {
        try
        {
            await Dispatcher.UIThread.InvokeAsync(RefreshModList);
        }
        catch
        {
            // ignored
        }
    }
    
    private async Task RunPreInitAsync()
    {
        // Third party mods disclaimer. Show on first launch.
        if (!Flags.CheckFlag(Constants.THIRD_PARTY_DISCLAIMER_FLAG_NAME))
        {
            await MessageBoxWindow.Show(this, "Notice", "Mods are third-party content, not verified or moderated by SRMM Studio. Only install mods from sources you trust (e.g. NexusMods). Some may modify game files or otherwise affect your installation.");
            
            Flags.CreateFlag(Constants.THIRD_PARTY_DISCLAIMER_FLAG_NAME);
        }
        
        // Referencing `Program` all the time isn't ideal. Maybe move to global settings/helper file?
        if (Program.ShouldBeExternalOnly())
        {
            await MessageBoxWindow.Show(this, "Warning", "External mods folder detected. Please run Shin Ryu Mod Manager in CLI mode (use --cli parameter) and use the external mod manager instead.");
        }
        
        if (Program.LogLevel <= LogEventLevel.Warning)
        {
            if (Program.MissingDll())
            {
                await MessageBoxWindow.Show(this, "Warning", $"Warning: Neither \"{Constants.VERSIONDLL}\" or \"{Constants.DINPUT8DLL}\" is present in this directory. Shin Ryu Mod Manager will NOT function properly without these files.");
            }
            
            if (Program.MissingAsi())
            {
                await MessageBoxWindow.Show(this, "Warning", $"{Constants.ASI} is missing from this directory. Mods will NOT be applied without this file.");
            }
        }
    }
    
    // Event Handlers
    private async void ModList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            var grid = sender as DataGrid;
            
            if (grid?.SelectedItem is not ModInfo selected)
                return;
            
            await UpdateModMetaAsync(DataContext as MainWindowViewModel, selected);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to select mod!");
            
            await MessageBoxWindow.Show(this, "Error", "An error has occurred. \nPlease check\"srmm_errors.txt\" for more info.");
        }
    }
    
    private void ModToggle_OnClick(object sender, RoutedEventArgs e)
    {
        foreach (ModInfo mod in ModListView.SelectedItems)
        {
            mod.ToggleEnabled();
        }
    }
    
    private void ModUp_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;
        
        var selection = ModListView.SelectedItems.Cast<ModInfo>().OrderBy(m => viewModel!.ModList.IndexOf(m)).ToList();
        
        var limit = 0;
        
        foreach (var i in selection.Select(m => viewModel.ModList.IndexOf(m)).OrderBy(x => x))
        {
            if (i > limit)
            {
                (viewModel.ModList[i - 1], viewModel.ModList[i]) = (viewModel.ModList[i], viewModel.ModList[i - 1]);
            }
            else
            {
                ++limit;
            }
        }
        
        // Restore selection
        foreach (var mod in selection)
        {
            ModListView.SelectedItems.Add(mod);
        }
    }
    
    private void ModDown_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;
        
        var selection = ModListView.SelectedItems.Cast<ModInfo>().OrderBy(m => viewModel.ModList.IndexOf(m)).ToList();
        
        var limit = viewModel.ModList.Count - 1;
        
        foreach (var i in selection.Select(m => viewModel.ModList.IndexOf(m)).OrderByDescending(x => x))
        {
            if (i < limit)
            {
                (viewModel.ModList[i + 1], viewModel.ModList[i]) = (viewModel.ModList[i], viewModel.ModList[i + 1]);
            }
            else
            {
                --limit;
            }
        }
        
        // Restore selection
        foreach (var mod in selection)
        {
            ModListView.SelectedItems.Add(mod);
        }
    }
    
    private async void ModSave_OnClick(object sender, RoutedEventArgs e)
    {
        var progressWindow = new ProgressWindow("Applying mods. Please wait...", true);
        
        try
        {
            if (DataContext is not MainWindowViewModel viewModel)
                return;
            
            if (OperatingSystem.IsLinux() && !Flags.CheckFlag(Constants.LINUX_WIKI_REMINDER_FLAG_FILE_NAME))
            {
                var result = await MessageBoxWindow.Show(this, "Information",
                    "Check your [launch settings](https://github.com/SRMM-Studio/ShinRyuModManager/wiki/Launch-Options) to help ensure your mods load correctly.",
                    showCancel: false, dontRemind: true);
                
                if (result == MessageBoxResult.DontRemind)
                {
                    Flags.CreateFlag(Constants.LINUX_WIKI_REMINDER_FLAG_FILE_NAME);
                }
            }
            
            if (viewModel.ModList.Count > 0)
            {
                var mods = viewModel.ModList.ToList();
                
                Program.SaveModList(mods);
                
                await CheckModDependenciesAsync(mods);
                
                // Run generation only if it will not be run on game launch (i.e., if RebuildMlo is disabled or unsupported)
                if (Program.RebuildMlo && Program.IsRebuildMloSupported)
                {
                    await MessageBoxWindow.Show(this, "Information", "Mod list was saved. Mods will be applied next time the game is run.");
                    
                    return;
                }
                
                _ = progressWindow.ShowDialog(this);
                
                try {
                    await Task.Run(() => Program.RunGeneration(mods.Where(x => x.Enabled).ToList()));
                } catch (Exception ex) {
                    Log.Error(ex, "Could not generate MLO!");
                    
                    await Dispatcher.UIThread.InvokeAsync(() => MessageBoxWindow.Show(this, "Error", "Mods could not be applied. Please make sure that the game directory has write access." +
                        "\n\nPlease check\"srmm_errors.txt\" for more info."));
                } finally {
                    progressWindow.Close();
                }
            }
            else
            {
                progressWindow.Close();
                
                await MessageBoxWindow.Show(this, "Error", "Mod list is empty and was not saved.");
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "ModSave failed!");
            
            progressWindow.Close();
            
            await ShowDefaultError();
        }
    }
    
    private async Task CheckModDependenciesAsync(List<ModInfo> mods)
    {
        var modsWithDependencyProblems = new List<string>();
        var disabledLibraries = new List<string>();
        var missingLibraries = new List<string>();
        
        foreach (var modName in mods.Where(x => x.Enabled).Select(x => x.Name))
        {
            foreach (var dependencyGuid in Program.GetModDependencies(modName))
            {
                if (!Program.DoesLibraryExist(dependencyGuid))
                {
                    if (!modsWithDependencyProblems.Contains(modName))
                        modsWithDependencyProblems.Add(modName);
                    
                    missingLibraries.Add(dependencyGuid);
                }
                else if (!Program.IsLibraryEnabled(dependencyGuid))
                {
                    if (!modsWithDependencyProblems.Contains(modName))
                        modsWithDependencyProblems.Add(modName);
                    
                    disabledLibraries.Add(dependencyGuid);
                }
            }
        }
        
        if (disabledLibraries.Count > 0 || missingLibraries.Count > 0)
        {
            var sb = new StringBuilder();
            
            if (missingLibraries.Count > 0)
            {
                sb.AppendLine("Missing libraries:");
                
                sb.AppendJoin('\n', missingLibraries.Select(Program.GetLibraryName));
                
                sb.AppendLine();
            }
            
            if (disabledLibraries.Count > 0)
            {
                sb.AppendLine("Disabled libraries:");
                
                sb.AppendJoin('\n', disabledLibraries.Select(Program.GetLibraryName));
                
                sb.AppendLine();
            }
            
            sb.AppendLine("The following mods depend on these libraries:");
            
            sb.AppendJoin('\n', modsWithDependencyProblems.Select(Program.GetLibraryName));
            
            sb.AppendLine();
            
            sb.Append("Your mods may not properly work without them. Consider installing or enabling them from the Libraries tab.");
            
            await MessageBoxWindow.Show(this, "Mod Library Dependency Warning", sb.ToString());
        }
    }
    
    private void ModClose_OnClick(object sender, RoutedEventArgs e) => Close();
    
    private async void ModInstall_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is not MainWindowViewModel viewModel)
                return;
            
            Directory.CreateDirectory(GamePath.MODS);
            
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = true,
                FileTypeFilter = [CustomFilePickerFileTypes.CompressedZip, CustomFilePickerFileTypes.CompressedRar, CustomFilePickerFileTypes.CompressedGzip, FilePickerFileTypes.All]
            });
            
            if (files.Count == 0)
                return;
            
            var progressWindow = new ProgressWindow("Installing mod(s). Please wait...", true);
            
            _ = progressWindow.ShowDialog(this);
            
            await Task.Yield();
            
            await Task.Run(async () =>
            {
                foreach (var file in files)
                {
                    var localPath = file.TryGetLocalPath();
                    
                    if (!File.Exists(localPath))
                        return;
                    
                    await Utils.TryInstallModZipAsync(localPath);
                }
                
                await Program.InstallAllModDependenciesAsync(viewModel.ModList.ToList());
            });
            
            RefreshModList();
            
            progressWindow.Close();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "ModInstalled failed!");
            
            await ShowDefaultError();
        }
    }
    
    private void MenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        CreateOrActivateWindow<LibraryManagerWindow>();
    }
    
    private void ModListViewRefresh_OnClick(object sender, RoutedEventArgs e) => RefreshModList();
    
    private async void ModMetaSampleYaml_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var saveDialog = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                SuggestedFileName = "mod-meta",
                DefaultExtension = ".yaml",
                ShowOverwritePrompt = true,
                SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(GamePath.ModsPath),
                FileTypeChoices = [CustomFilePickerFileTypes.YamlFile, FilePickerFileTypes.All]
            });
            
            if (saveDialog?.TryGetLocalPath() == null)
                return;
            
            var sampleMeta = new ModMeta
            {
                Name = "Your mod name",
                Author = "Author name",
                Version = "1.0.0",
                Description = "Mod description example.\nThis is in a new line.\n\nYou can use single (') or double (\") quotes in your text.\n\nIf you want your text to be more organized in the yaml file, make sure the new line is indented (press TAB)."
            };
            
            var serializer = new SerializerBuilder().WithDefaultScalarStyle(ScalarStyle.Plain).Build();
            var yaml = serializer.Serialize(sampleMeta);
            
            await File.WriteAllTextAsync(saveDialog.TryGetLocalPath()!, yaml, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "ModMetaSampleYaml failed!");
            
            await ShowDefaultError();
        }
    }
    
    private async void ModMetaSampleImage_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var saveDialog = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                SuggestedFileName = "mod-image",
                DefaultExtension = ".png",
                ShowOverwritePrompt = true,
                SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(GamePath.ModsPath),
                FileTypeChoices = [FilePickerFileTypes.ImagePng]
            });
            
            if (saveDialog?.TryGetLocalPath() == null)
                return;
            
            var sampleImage = await UiHelpers.LoadResourceAsBitmapAsync("NoImage.png");
            sampleImage.Save(saveDialog.TryGetLocalPath()!);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "ModMetaSampleImage failed!");
            
            await ShowDefaultError();
        }
    }
    
    private void About_OnClick(object sender, RoutedEventArgs e)
    {
        CreateOrActivateWindow<AboutWindow>();
    }
    
    private void ChangeLog_OnClick(object sender, RoutedEventArgs e)
    {
        CreateOrActivateWindow<ChangeLogWindow>();
    }
    
    private async void MetaEditEnable_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (ModListView.SelectedItems.Count == 0)
            {
                await MessageBoxWindow.Show(this, "Error", "No mod selected.");
                
                return;
            }
            
            ChangeUiState(UiState.Editable);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "MetaEditEnable failed!");
            
            await ShowDefaultError();
        }
    }
    
    private async void MetaEditCancel_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is not MainWindowViewModel viewModel)
                return;
            
            ChangeUiState(UiState.Normal);
            
            var selection = ModListView.SelectedItems.Cast<ModInfo>().First();
            await UpdateModMetaAsync(viewModel, selection);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "MetaEditCancel failed!");
            
            await ShowDefaultError();
        }
    }
    
    private async void MetaEditSave_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is not MainWindowViewModel viewModel)
                return;
            
            var selection = ModListView.SelectedItems.Cast<ModInfo>().First();
            var meta = await GetModMetaAsync(selection.Name);
            
            meta.Name = ModNameEditable.Text?.Trim();
            meta.Author = ModAuthorEditable.Text?.Trim();
            meta.Version = ModVersionEditable.Text?.Trim();
            meta.Description = ModDescriptionEditable.Text?.Trim();
            
            // TODO: Library dependencies
            
            // Save mod-meta.yaml
            try
            {
                var yaml = YamlHelpers.SerializeObject(meta);
                
                var path = Path.Combine(GamePath.ModsPath, selection.Name, "mod-meta.yaml");
                await File.WriteAllTextAsync(path, yaml, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "MetaEditSave failed!");
                
                await ShowDefaultError();
                
                return;
            }
            
            ChangeUiState(UiState.Normal);
            
            await UpdateModMetaAsync(viewModel, selection);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "MetaEditSave failed!");
            
            await ShowDefaultError();
        }
    }
    
    private async void LaunchGame_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is not MainWindowViewModel viewModel)
                return;
            
            if (!string.IsNullOrEmpty(viewModel.GameLaunchPath))
            {
                if (OperatingSystem.IsWindows())
                {
                    Process.Start(new ProcessStartInfo(viewModel.GameLaunchPath)
                    {
                        UseShellExecute = true
                    });
                }
                else if (OperatingSystem.IsLinux())
                {
                    Process.Start("xdg-open", viewModel.GameLaunchPath);
                }
            }
            else
            {
                await MessageBoxWindow.Show(this, "Error", "The game can't be launched. Please launch manually.");
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "LaunchGame failed!");
            
            await ShowDefaultError();
        }
    }
    
    // Private methods
    private async Task UpdateModMetaAsync(MainWindowViewModel viewModel, ModInfo mod)
    {
        const string modImagePattern = "mod-image.*";
        
        var modPath = GamePath.GetModDirectory(mod.Name);
        var matchingModImages = Directory.EnumerateFiles(modPath, modImagePattern);
        
        Bitmap modImage = null;
        
        try
        {
            var meta = await GetModMetaAsync(mod.Name);
            
            viewModel.SelectMod(meta);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load mod meta!");
            
            await MessageBoxWindow.Show(this, "Error", $"An error has occurred while trying to load mod-meta.\nPlease check\"srmm_errors.txt\" for more info.");
        }
        
        foreach (var filePath in matchingModImages)
        {
            try
            {
                await using var fs = File.OpenRead(filePath);
                
                modImage = new Bitmap(fs);
                
                break;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load mod image!");
                
                await MessageBoxWindow.Show(this, "Error", $"An error has occurred while trying to load {Path.GetFileName(filePath)}.\nPlease check\"srmm_errors.txt\" for more info.");
            }
        }
        
        modImage ??= await UiHelpers.LoadResourceAsBitmapAsync("NoImage.png");
        
        ModImage.Source = modImage;
    }
    
    private static async Task<ModMeta> GetModMetaAsync(string modName)
    {
        var modPath = GamePath.GetModDirectory(modName);
        var modMetaPath = Path.Combine(modPath, "mod-meta.yaml");
        
        var meta = ModMeta.GetPlaceholder(modName);
        
        if (File.Exists(modMetaPath))
        {
            meta = await YamlHelpers.DeserializeYamlFromPathAsync<ModMeta>(modMetaPath);
        }
        
        return meta;
    }
    
    private void RefreshModList()
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;
        
        viewModel.LoadModList();
    }
    
    private void CreateOrActivateWindow<T>() where T : Window, new()
    {
        if (_childWindow is { IsVisible: true })
        {
            // Window visible
            _childWindow.Activate();
        }
        else
        {
            _childWindow = new T();
            _childWindow.Closed += (_, _) => _childWindow = null;
            _childWindow.Show(this);
        }
    }
    
    private async Task ShowDefaultError() {
        await MessageBoxWindow.Show(this, "Fatal", "An error has occurred.\nPlease check\"srmm_errors.txt\" for more info.");
    }
    
    private void ChangeUiState(UiState state)
    {
        switch (state)
        {
            case UiState.Normal:
                ModName.IsVisible = true;
                ModAuthor.IsVisible = true;
                ModVersion.IsVisible = true;
                ModDescription.IsVisible = true;
                ModNameEditable.IsVisible = false;
                ModAuthorEditable.IsVisible = false;
                ModVersionEditable.IsVisible = false;
                ModDescriptionEditable.IsVisible = false;
                
                MetaEditEnable.IsVisible = true;
                MetaEditCancel.IsVisible = false;
                MetaEditSave.IsVisible = false;
                
                ModListView.IsEnabled = true;
                
                break;
            case UiState.Editable:
                ModName.IsVisible = false;
                ModAuthor.IsVisible = false;
                ModVersion.IsVisible = false;
                ModDescription.IsVisible = false;
                ModNameEditable.IsVisible = true;
                ModAuthorEditable.IsVisible = true;
                ModVersionEditable.IsVisible = true;
                ModDescriptionEditable.IsVisible = true;
                
                MetaEditEnable.IsVisible = false;
                MetaEditCancel.IsVisible = true;
                MetaEditSave.IsVisible = true;
                
                ModListView.IsEnabled = false;
                
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }
}
