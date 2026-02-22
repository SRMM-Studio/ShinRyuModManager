using Avalonia.Controls;
using Avalonia.Interactivity;
using Serilog;
using ShinRyuModManager.UserInterface.ViewModels;
using ShinRyuModManager.UserInterface.Views;
using Utils;

namespace ShinRyuModManager.UserInterface.UserControls;

public partial class LibraryDisplayControl : UserControl
{
    private readonly LibMeta _meta;
    private LibMeta _localMeta;
    private bool _isLibraryInstalled;
    private bool _isLibraryEnabled;
    private bool _isLibraryUpdateAvailable;
    
    // ReSharper disable once MemberCanBePrivate.Global
    public LibraryDisplayControl()
    {
        InitializeComponent();
    }
    
    public LibraryDisplayControl(LibMeta meta) : this()
    {
        DataContext = new LibraryDisplayControlViewModel(meta);
        
        _meta = meta;
        
        RefreshComponent();
    }
    
    private void RefreshComponent()
    {
        CompareToLocalInstallation();
        UpdateButtonVisibility();
    }
    
    private void CompareToLocalInstallation()
    {
        if (DataContext is not LibraryDisplayControlViewModel viewModel)
            return;
        
        var dirPath = Path.Combine(GamePath.LIBRARIES, _meta.GUID.ToString());
        var metaPath = Path.Combine(dirPath, Constants.LIBRARIES_LIBMETA_FILE_NAME);
        
        if (Directory.Exists(dirPath))
        {
            _isLibraryInstalled = true;
            
            var yamlString = File.ReadAllText(metaPath);
            
            _localMeta = LibMeta.ReadLibMeta(yamlString);
            _isLibraryEnabled = !File.Exists(Path.Combine(GamePath.LibrariesPath, _meta.GUID.ToString(), ".disabled"));
            _isLibraryUpdateAvailable = Utils.CompareVersionIsHigher(_meta.Version, _localMeta.Version);
            
            if (_isLibraryUpdateAvailable)
                viewModel.Version = $"{_meta.Version} (Installed: {_localMeta.Version})";
        }
        else
        {
            _isLibraryInstalled = false;
        }
    }
    
    private void UpdateButtonVisibility()
    {
        if (DataContext is not LibraryDisplayControlViewModel viewModel)
            return;
        
        viewModel.EnableBtnVisibility = false;
        viewModel.DisableBtnVisibility = false;
        viewModel.InstallBtnVisibility = false;
        viewModel.UninstallBtnVisibility = false;
        viewModel.UpdateBtnVisibility = false;
        viewModel.SourceBtnVisibility = false;
        
        if (_isLibraryInstalled)
        {
            viewModel.UninstallBtnVisibility = true;
            
            if (_isLibraryUpdateAvailable)
            {
                viewModel.UpdateBtnVisibility = true;
            }
            
            if (_isLibraryEnabled)
            {
                if (_meta.CanBeDisabled)
                    viewModel.DisableBtnVisibility = true;
            }
            else
            {
                viewModel.EnableBtnVisibility = true;
            }
        }
        else
        {
            viewModel.InstallBtnVisibility = true;
        }
        
        if (!string.IsNullOrEmpty(_meta.Source) && _meta.Source.StartsWith("http"))
        {
            viewModel.SourceBtnVisibility = true;
            viewModel.SourceLink = _meta.Source;
        }
    }
    
    private async void InstallOrUpdate_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            await Program.InstallLibraryAsync(_meta.GUID.ToString());
            
            RefreshComponent();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to install library!");
            
            var window = TopLevel.GetTopLevel(this) as Window;
            _ = await MessageBoxWindow.Show(window, "Fatal", "An error has occurred.\nPlease check\"srmm_errors.txt\" for more info.");
        }
    }
    
    private void Uninstall_OnClick(object sender, RoutedEventArgs e)
    {
        var path = Path.Combine(GamePath.LIBRARIES, _meta.GUID.ToString());
        
        if (!Directory.Exists(path))
            return;
        
        Directory.Delete(path, true);
        
        RefreshComponent();
    }
    
    private void Enable_OnClick(object sender, RoutedEventArgs e)
    {
        _isLibraryEnabled = true;
        
        // Delete invisible file
        var flagFilePath = Path.Combine(GamePath.LibrariesPath, _meta.GUID.ToString(), ".disabled");
        
        if (File.Exists(flagFilePath))
            File.Delete(flagFilePath);
        
        UpdateButtonVisibility();
    }
    
    private void Disable_OnClick(object sender, RoutedEventArgs e)
    {
        _isLibraryEnabled = false;
        
        // Write invisible file as flag for Parless
        var flagFilePath = Path.Combine(GamePath.LibrariesPath, _meta.GUID.ToString(), ".disabled");
        File.Create(flagFilePath).Close();
        File.SetAttributes(flagFilePath, File.GetAttributes(flagFilePath) | FileAttributes.Hidden);
        
        UpdateButtonVisibility();
    }
}
