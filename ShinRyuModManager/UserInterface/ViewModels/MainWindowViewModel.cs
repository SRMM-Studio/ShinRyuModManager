using System.Collections.ObjectModel;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using ShinRyuModManager.Extensions;
using ShinRyuModManager.ModLoadOrder.Mods;
using Utils;

namespace ShinRyuModManager.UserInterface.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string TitleText { get; set; } = "Shin Ryu Mod Manager";

    [ObservableProperty]
    public partial string AppVersionText { get; set; } = "SRMM Version";

    [ObservableProperty]
    public partial string GameLaunchPath { get; set; }

    [ObservableProperty]
    public partial string ModName { get; set; } = "Mod Name";
    
    [ObservableProperty]
    public partial string ModDescription { get; set; } = "Mod Description";

    [ObservableProperty]
    public partial string ModAuthor { get; set; } = "Author";

    [ObservableProperty]
    public partial string ModVersion { get; set; } = "Version";

    [ObservableProperty]
    public partial ObservableCollection<ModInfo> ModList { get; set; }

    public MainWindowViewModel()
    {
        Initialize();
    }
    
    public void SelectMod(ModMeta mod)
    {
        ModName = mod.Name;
        ModDescription = mod.Description;
        ModAuthor = mod.Author;
        ModVersion = mod.Version;
    }
    
    [RelayCommand]
    private void UpdateProfile(Profile profile)
    {
        Program.ActiveProfile = profile;
        
        Log.Information("Setting Profile to ");
        
        LoadModList(profile);
    }
    
    private void Initialize()
    {
        AppVersionText = $"v{AssemblyVersion.GetVersion()}";
        GameLaunchPath = GamePath.GetGameLaunchPath();
        
        Directory.CreateDirectory(GamePath.MODS);
        Directory.CreateDirectory(GamePath.LIBRARIES);
    }
    
    internal void LoadModList(Profile? profile = null)
    {
        ModList = new ObservableCollection<ModInfo>(Program.PreRun(profile));
        
        TitleText = $"Shin Ryu Mod Manager [{GamePath.GetGameFriendlyName(GamePath.CurrentGame)}] [{Program.ActiveProfile.GetDescription()}]";
    }
}
