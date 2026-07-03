using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ShinRyuModManager.UserInterface.ViewModels;

namespace ShinRyuModManager.UserInterface.Views;

public partial class UpdateAvailableWindow : Window
{
    public UpdateAvailableWindow()
    {
        InitializeComponent();
    }
    
    private UpdateAvailableWindow(Version updateVersion)
    {
        DataContext = new UpdateAvailableViewModel(updateVersion);
        
        InitializeComponent();
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
        if (DataContext is not UpdateAvailableViewModel viewModel)
            return;
        
        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo { FileName = viewModel.LinkToUpdate, UseShellExecute = true });
        }
        else if (OperatingSystem.IsLinux())
        {
            Process.Start("xdg-open", viewModel.LinkToUpdate);
        }
        
        Close();
    }
    
    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
