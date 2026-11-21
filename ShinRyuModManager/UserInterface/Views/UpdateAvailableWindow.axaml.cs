using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ShinRyuModManager.UserInterface.ViewModels;

namespace ShinRyuModManager.UserInterface.Views;

public partial class UpdateAvailableWindow : Window
{
    private readonly UpdateAvailableViewModel _viewModel = new();
    
    public UpdateAvailableWindow()
    {
        InitializeComponent();
    }
    
    private UpdateAvailableWindow(Version updateVersion) : this()
    {
        var vm = new UpdateAvailableViewModel(updateVersion);
        
        _viewModel = vm;
        DataContext = vm;
    }
    
    public static Task Show(Window owner, Version updateVersion)
    {
        var win = new UpdateAvailableWindow(updateVersion)
        {
            Icon = owner.Icon,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        
        if (owner.Icon != null)
        {
            win.Icon = owner.Icon;
        }
        
        return win.ShowDialog(owner);
    }
    
    private void OpenBrowser_Click(object sender, RoutedEventArgs e)
    {
        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo { FileName = _viewModel.LinkToUpdate, UseShellExecute = true });
        }
        else if (OperatingSystem.IsLinux())
        {
            Process.Start("xdg-open", _viewModel.LinkToUpdate);
        }
        
        Close();
    }
    
    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
