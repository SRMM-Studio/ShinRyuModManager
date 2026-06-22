using System.IO.Compression;
using NetSparkleUpdater;
using NetSparkleUpdater.Interfaces;

namespace ShinRyuModManager.UserInterface.Updater;

public class RyuUpdaterUpdater : SparkleUpdater
{
    public RyuUpdaterUpdater(string appcastUrl, ISignatureVerifier signatureVerifier) : base(appcastUrl, signatureVerifier) { }
    
    public RyuUpdaterUpdater(string appcastUrl, ISignatureVerifier signatureVerifier, string referenceAssembly) : base(appcastUrl, signatureVerifier, referenceAssembly, null) { }
    
    public RyuUpdaterUpdater(string appcastUrl, ISignatureVerifier signatureVerifier, string referenceAssembly, IUIFactory factory) : base(appcastUrl, signatureVerifier, referenceAssembly, factory) { }
    
    protected override Task RunDownloadedInstaller(string downloadFilePath)
    {
        ZipFile.ExtractToDirectory(downloadFilePath, Environment.CurrentDirectory, overwriteFiles: true);
        
        return Task.CompletedTask;
    }
}
