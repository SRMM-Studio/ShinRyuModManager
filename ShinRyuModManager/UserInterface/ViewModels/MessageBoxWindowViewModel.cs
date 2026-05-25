using CommunityToolkit.Mvvm.ComponentModel;

namespace ShinRyuModManager.UserInterface.ViewModels;

public partial class MessageBoxWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial bool ShowCancel { get; set; }

    [ObservableProperty]
    public partial bool ShowDontRemind { get; set; }

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
