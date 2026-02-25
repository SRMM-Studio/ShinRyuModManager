using CommunityToolkit.Mvvm.ComponentModel;

namespace ShinRyuModManager.UserInterface.ViewModels;

public partial class MessageBoxWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string _messageText;
    [ObservableProperty] private bool _isVisible;
    
    public MessageBoxWindowViewModel() { }
    
    public MessageBoxWindowViewModel(string messageText, bool isVisible)
    {
        MessageText = messageText;
        IsVisible = isVisible;
    }
}
