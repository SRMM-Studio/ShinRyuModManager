using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ShinRyuModManager.UserInterface.ViewModels;
using Utils;

namespace ShinRyuModManager.UserInterface.Views;

public partial class LogWindow : Window
{
    private readonly LogWindowViewModel _viewModel = new();
    
    public LogWindow()
    {
        InitializeComponent();
        
        DataContext = _viewModel;
        
        _viewModel.LinesFlushed += ScrollToEnd;
        Opened += (_, _) => Dispatcher.UIThread.Post(ScrollToEnd, DispatcherPriority.Loaded);
    }
    
    private void ScrollToEnd()
    {
        if (AutoScroll.IsChecked != true || _viewModel.Lines.Count == 0)
            return;
        
        LogList.ScrollIntoView(_viewModel.Lines.Count - 1);
    }
    
    private async void CopyAll_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Clipboard is null)
                return;
            
            await Clipboard.SetTextAsync(_viewModel.GetAllText());
        }
        catch
        {
            // ignored
        }
    }
    
    private async void OpenLogs_OnClick(object sender, RoutedEventArgs e)
    {
        var path = Path.GetFullPath(Constants.LOGS_BASE_PATH);
        
        var launcher = GetTopLevel(this)!.Launcher;
        
        try
        {
            var opened = await launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(path));
            
            if (!opened)
            {
                await MessageBoxWindow.Show(this, "Error", "Unable to open logs directory.");
            }
        }
        catch
        {
            // ignored
        }
    }
    
    protected override void OnClosed(EventArgs e)
    {
        _viewModel.LinesFlushed -= ScrollToEnd;
        _viewModel.Dispose();
        
        base.OnClosed(e);
    }
}

