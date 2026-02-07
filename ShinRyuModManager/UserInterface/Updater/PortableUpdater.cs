using System.Diagnostics;
using NetSparkleUpdater;
using NetSparkleUpdater.Interfaces;

namespace ShinRyuModManager.UserInterface.Updater;

public sealed class PortableUpdater : SparkleUpdater
{
    public PortableUpdater(string appcastUrl, ISignatureVerifier signatureVerifier) : base(appcastUrl, signatureVerifier) { }
    
    protected override Task RunDownloadedInstaller(string downloadFilePath)
    {
        var ryuPath = Path.Combine(Environment.CurrentDirectory, "RyuUpdater");
        
        using var currentProcess = Process.GetCurrentProcess();
        
        var pid = Environment.ProcessId;
        var name = currentProcess.ProcessName;
        
        Process.Start(new ProcessStartInfo
        {
            FileName = ryuPath,
            ArgumentList =
            {
                pid.ToString(),
                downloadFilePath,
                Environment.CurrentDirectory,
                name
            },
            UseShellExecute = false,
        });
        
        Environment.Exit(0x55504454); //UPDT
        
        return Task.CompletedTask;
    }
}
