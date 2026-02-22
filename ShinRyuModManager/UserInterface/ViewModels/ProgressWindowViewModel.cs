using CommunityToolkit.Mvvm.ComponentModel;

namespace ShinRyuModManager.UserInterface.ViewModels;

public partial class ProgressWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _messageText;
    [ObservableProperty] private bool _isIndeterminate;
    
    public ProgressWindowViewModel() { }
    
    public ProgressWindowViewModel(string title, string messageText, bool isIndeterminate)
    {
        Title = title;
        MessageText = messageText;
        IsIndeterminate = isIndeterminate;
    }
}
