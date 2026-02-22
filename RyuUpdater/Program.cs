using System.Diagnostics;
using System.IO.Compression;
using Utils;

namespace RyuUpdater;

public static class Program
{
    private static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("This is a helper application for SRMM. Please don't run manually!\nPress any key to exit...");
            Console.ReadKey();
            
            return;
        }
        
        var pid = int.Parse(args[0]);
        var updateFile = args[1];
        var targetDir = args[2];
        var srmmFileName = args[3];
        
        var tempDir = new FileInfo(updateFile).Directory!.FullName;
        
        try
        {
            if (pid != -1)
            {
                await Process.GetProcessById(pid).WaitForExitAsync();
            }
        }
        catch
        {
            // ignore
        }
        
        await Task.Delay(500);
        
        await ZipFile.ExtractToDirectoryAsync(updateFile, targetDir, overwriteFiles: true);
        Directory.Delete(tempDir, recursive: true);
        
        Flags.CreateFlag(Constants.UPDATE_RECENT_FLAG_FILE_NAME);
        
        var srmmPath = Path.Combine(targetDir, srmmFileName);
        
        Process.Start(new ProcessStartInfo
        {
            FileName = srmmPath,
            UseShellExecute = true
        });
    }
}
