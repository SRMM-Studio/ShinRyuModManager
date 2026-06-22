using CommunityToolkit.Mvvm.ComponentModel;
using Utils;

namespace ShinRyuModManager.UserInterface.ViewModels;

public partial class AboutWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Version { get; set; }

    [ObservableProperty]
    public partial string CreditsText { get; set; }

    public AboutWindowViewModel()
    {
        Initialize();
    }
    
    private void Initialize()
    {
        Version = $"v{AssemblyVersion.GetVersion()}";
        
        using var credits = UiHelpers.LoadResourceAsStream("credits.txt");
        using var sr = new StreamReader(credits);
        
        CreditsText = sr.ReadToEnd();
    }
}
