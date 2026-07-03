using CommunityToolkit.Mvvm.ComponentModel;

namespace ShinRyuModManager.UserInterface.ViewModels;

public partial class UpdateAvailableViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string UpdateAvailableText { get; set; }
    
    public readonly string LinkToUpdate = "https://www.nexusmods.com/site/mods/743?tab=files";
    
    public UpdateAvailableViewModel() { }
    
    public UpdateAvailableViewModel(Version updateVersion)
    {
        UpdateAvailableText = $"An update to SRMM is available. New Version: {updateVersion}";
    }
}
