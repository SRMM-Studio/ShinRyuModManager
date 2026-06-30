using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Serilog;
using ShinRyuModManager.UserInterface.ViewModels;
using ShinRyuModManager.UserInterface.Views;

namespace ShinRyuModManager.UserInterface;

public partial class App : Application
{
    public override void Initialize()
    {
        Log.Information("Avalonia App Init");
        AvaloniaXamlLoader.Load(this);
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        Log.Information("OnFrameworkInitializationCompleted");

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var culture = new CultureInfo("en");
            
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            Log.Information("Start MainWindow");

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
            
            desktop.Exit += DesktopOnExit;
        }
        
#if DEBUG
        this.AttachDeveloperTools();
#endif
        
        base.OnFrameworkInitializationCompleted();
    }
    
    // Handle properly disposing logs for UI
    private static void DesktopOnExit(object sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        Log.CloseAndFlush();
    }
}
