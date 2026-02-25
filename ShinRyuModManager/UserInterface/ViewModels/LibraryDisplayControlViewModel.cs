using CommunityToolkit.Mvvm.ComponentModel;

namespace ShinRyuModManager.UserInterface.ViewModels;

public partial class LibraryDisplayControlViewModel : ViewModelBase
{
    [ObservableProperty] private string _libName = "Name";
    [ObservableProperty] private string _author = "Author";
    [ObservableProperty] private string _description = "Description";
    [ObservableProperty] private string _guid = "Guid";
    [ObservableProperty] private string _version = "Version";
    [ObservableProperty] private string _sourceLink;
    
    [ObservableProperty] private bool _enableBtnVisibility;
    [ObservableProperty] private bool _disableBtnVisibility;
    [ObservableProperty] private bool _installBtnVisibility;
    [ObservableProperty] private bool _uninstallBtnVisibility;
    [ObservableProperty] private bool _updateBtnVisibility;
    [ObservableProperty] private bool _sourceBtnVisibility;
    
    public LibraryDisplayControlViewModel() { }
    
    public LibraryDisplayControlViewModel(LibMeta meta)
    {
        _libName = meta.Name;
        _author = meta.Author;
        _description = meta.Description;
        _guid = meta.GUID.ToString();
        _version = meta.Version;
    }
}
