using Avalonia.Controls;
using Avalonia.VisualTree;
using NetSparkleUpdater;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.UI.Avalonia;

namespace ShinRyuModManager.UserInterface.Updater;

public class CustomUIFactory : UIFactory
{
    // Need to override the "Skip this version" button to make a "Never ask again" button...
    public override IUpdateAvailable CreateUpdateAvailableWindow(List<AppCastItem> updates, ISignatureVerifier signatureVerifier, string currentVersion = "", string appName = "the application", bool isUpdateAlreadyDownloaded = false)
    {
        var window = (Window)base.CreateUpdateAvailableWindow(updates, signatureVerifier, currentVersion, appName, isUpdateAlreadyDownloaded);
        
        window.Opened += (_, _) =>
        {
            var skipButton = window.GetVisualDescendants()
                                   .OfType<Button>()
                                   .FirstOrDefault(x => x.Name == "SkipButton");
            
            if (skipButton == null)
                return;
            
            skipButton.Content = "Never ask again";
            
            skipButton.Click += (_, _) =>
            {
                Program.DisableAutoUpdate();
                
                window.Close();
            };
        };
        
        return (IUpdateAvailable)window;
    }
}
