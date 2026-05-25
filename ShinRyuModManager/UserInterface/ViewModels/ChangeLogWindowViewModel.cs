using CommunityToolkit.Mvvm.ComponentModel;

namespace ShinRyuModManager.UserInterface.ViewModels;

public partial class ChangeLogWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string ChangeLogText { get; set; }

    public ChangeLogWindowViewModel()
    {
        Initialize();
    }
    
    private void Initialize()
    {
        using var credits = UiHelpers.LoadResourceAsStream("changelog.md");
        using var sr = new StreamReader(credits);
        
        ChangeLogText = sr.ReadToEnd();
    }
}
