using System.IO.Compression;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using NetSparkleUpdater;
using NetSparkleUpdater.AssemblyAccessors;
using NetSparkleUpdater.Configurations;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;
using NetSparkleUpdater.UI.Avalonia;
using Serilog;
using Utils;

namespace ShinRyuModManager.UserInterface.Updater;

public static class AutoUpdating
{
    private const string GH_PAGES_ROOT = "https://thetruecolonel.github.io/SRMM-AppCast";
    
    private static string _tempDir;
    private static SparkleUpdater _updater;
    
    private static readonly string[] IgnoredBuilds = ["debug", "test"];
    
    public static void Init()
    {
        _tempDir = Path.Combine(Environment.CurrentDirectory, "srmm_temp");
        
        try
        {
            HandleRyuUpdater();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Problem trying to download RyuUpdater! Aborting auto updating...");
            
            return;
        }
        
        var suffix = AssemblyVersion.GetBuildSuffix();
        
        if (IgnoredBuilds.Contains(suffix, StringComparer.OrdinalIgnoreCase))
        {
            return; // Don't need to be annoyed with "Update Now" when debugging
        }
        
        var appcastUrl = $"{GH_PAGES_ROOT}/releases/appcast_{suffix}.xml";
        
        // Update check for SRMM
        _updater = new PortableUpdater(appcastUrl, new Ed25519Checker(SecurityMode.Unsafe))
        {
            UIFactory = new UIFactory
            {
                HideReleaseNotes = true,
                UseStaticUpdateWindowBackgroundColor = true,
                UpdateWindowGridBackgroundBrush = new ImmutableSolidColorBrush(Color.Parse("#373535"))
            },
            TmpDownloadFilePath = _tempDir,
            TmpDownloadFileNameWithExtension = $"{Guid.NewGuid()}.zip",
            RelaunchAfterUpdate = false,
            LogWriter = new SerilogWriter(),
#pragma warning disable CS0618 // Type or member is obsolete
            Configuration = new JSONConfiguration(new AssemblyReflectionAccessor(null))
#pragma warning restore CS0618 // Type or member is obsolete
        };
        
        _ = _updater.StartLoop(true, true, TimeSpan.FromMinutes(15));
    }
    
    private static void HandleRyuUpdater()
    {
        string ryuUpdaterPath;
        string updaterAppcastUrl;
        string updaterLatestUrl;
        
        if (OperatingSystem.IsWindows())
        {
            ryuUpdaterPath = Path.Combine(Environment.CurrentDirectory, "RyuUpdater.exe");
            updaterAppcastUrl = $"{GH_PAGES_ROOT}/releases/appcast_ryuupdater-windows.xml";
            updaterLatestUrl = $"{GH_PAGES_ROOT}/updater/RyuUpdater-Windows-Latest.zip";
        }
        else
        {
            ryuUpdaterPath = Path.Combine(Environment.CurrentDirectory, "RyuUpdater");
            updaterAppcastUrl = $"{GH_PAGES_ROOT}/releases/appcast_ryuupdater-linux.xml";
            updaterLatestUrl = $"{GH_PAGES_ROOT}/updater/RyuUpdater-Linux-Latest.zip";
        }
        
        // Pull grab latest version of updater if missing
        if (!File.Exists(ryuUpdaterPath))
        {
            using var downloadStream = Utils.Client.GetStreamAsync(updaterLatestUrl).GetAwaiter().GetResult();
            
            ZipFile.ExtractToDirectory(downloadStream, Environment.CurrentDirectory, overwriteFiles: true);
        }
        
        // TODO: Linux doesn't store the required information for this to work on compiled binaries. To come back to.
        /*// Update RyuUpdater quietly
        var ryuUpdater = new RyuUpdaterUpdater(updaterAppcastUrl, new Ed25519Checker(SecurityMode.Unsafe), ryuUpdaterPath) {
            UserInteractionMode = UserInteractionMode.DownloadAndInstall,
            UIFactory = null,
            TmpDownloadFilePath = _tempDir,
            TmpDownloadFileNameWithExtension = $"{Guid.NewGuid()}.zip"
        };
        _ = ryuUpdater.StartLoop(true);*/
    }
}
