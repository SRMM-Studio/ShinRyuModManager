using CommunityToolkit.Mvvm.ComponentModel;

namespace ShinRyuModManager.UserInterface.ViewModels;

public partial class ProgressWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial string MessageText { get; set; }

    [ObservableProperty]
    public partial bool IsIndeterminate { get; set; }

    public ProgressWindowViewModel() { }
    
    public ProgressWindowViewModel(string title, string messageText, bool isIndeterminate)
    {
        Title = title;
        MessageText = messageText;
        IsIndeterminate = isIndeterminate;
    }
}
