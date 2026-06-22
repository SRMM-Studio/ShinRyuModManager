using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ShinRyuModManager.UserInterface.UserControls;

namespace ShinRyuModManager.UserInterface.ViewModels;

public partial class LibraryManagerViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial ObservableCollection<LibraryDisplayControl> Library { get; set; } = [];
}
