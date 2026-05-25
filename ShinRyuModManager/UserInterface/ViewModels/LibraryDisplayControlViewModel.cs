using CommunityToolkit.Mvvm.ComponentModel;

namespace ShinRyuModManager.UserInterface.ViewModels;

public partial class LibraryDisplayControlViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string LibName { get; set; } = "Name";

    [ObservableProperty]
    public partial string Author { get; set; } = "Author";

    [ObservableProperty]
    public partial string Description { get; set; } = "Description";

    [ObservableProperty]
    public partial string Guid { get; set; } = "Guid";

    [ObservableProperty]
    public partial string Version { get; set; } = "Version";

    [ObservableProperty]
    public partial string SourceLink { get; set; }

    [ObservableProperty]
    public partial bool EnableBtnVisibility { get; set; }
    
    [ObservableProperty]
    public partial bool DisableBtnVisibility { get; set; }

    [ObservableProperty]
    public partial bool InstallBtnVisibility { get; set; }

    [ObservableProperty]
    public partial bool UninstallBtnVisibility { get; set; }

    [ObservableProperty]
    public partial bool UpdateBtnVisibility { get; set; }

    [ObservableProperty]
    public partial bool SourceBtnVisibility { get; set; }

    public LibraryDisplayControlViewModel() { }
    
    public LibraryDisplayControlViewModel(LibMeta meta)
    {
        LibName = meta.Name;
        Author = meta.Author;
        Description = meta.Description;
        Guid = meta.GUID.ToString();
        Version = meta.Version;
    }
}
