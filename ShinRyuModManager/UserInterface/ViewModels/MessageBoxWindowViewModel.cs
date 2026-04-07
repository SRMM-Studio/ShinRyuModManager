using CommunityToolkit.Mvvm.ComponentModel;

namespace ShinRyuModManager.UserInterface.ViewModels;

public partial class MessageBoxWindowViewModel : ViewModelBase
{
    [ObservableProperty] private bool _showCancel;
    [ObservableProperty] private bool _showDontRemind;
    
    public MessageBoxWindowViewModel()
    {
        ShowCancel = true;
        ShowDontRemind = true;
    }
    
    public MessageBoxWindowViewModel(bool showCancel, bool dontRemindButton)
    {
        ShowCancel = showCancel;
        ShowDontRemind = dontRemindButton;
    }
}
