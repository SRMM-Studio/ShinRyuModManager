using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using ModLoadOrder.Mods;
using YamlDotNet.Serialization;
using static Utils.Constants;

namespace RyuGUI
{
    public static class Program
    {
        private const string Kernel32Dll = "kernel32.dll";

        [DllImport(Kernel32Dll)]
        private static extern bool AllocConsole();

        [DllImport(Kernel32Dll)]
        private static extern bool FreeConsole();

        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                // Read the mod list (and execute other RMM stuff)
                List<ModInfo> mods = RyuHelpers.Program.PreRun();

                // This should be called only after PreRun() to make sure the ini value was loaded
                if (RyuHelpers.Program.ShouldBeExternalOnly())
                {
                    MessageBox.Show(
                        "External mods folder detected. Please run Ryu Mod Manager in CLI mode " +
                        "(use --cli parameter) and use the external mod manager instead.",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (RyuHelpers.Program.ShouldCheckForUpdates())
                {
                    CheckForUpdatesGUI();
                }

                if (RyuHelpers.Program.ShowWarnings())
                {
                    // Check if the ASI loader is not in the directory (possibly due to incorrect zip extraction)
                    if (RyuHelpers.Program.MissingDLL())
                    {
                        MessageBox.Show(
                            DINPUT8DLL + " is missing from this directory. Mods will NOT be applied without this file.",
                            "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    // Check if the ASI is not in the directory
                    if (RyuHelpers.Program.MissingASI())
                    {
                        MessageBox.Show(
                            ASI + " is missing from this directory. Mods will NOT be applied without this file.",
                            "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    // Calculate the checksum for the game's exe to inform the user if their version might be unsupported
                    if (RyuHelpers.Program.InvalidGameExe())
                    {
                        MessageBox.Show(
                            "Game version is unsupported. Please use the latest Steam version of the game. " +
                            "The mod list will still be saved, but mods might not work.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                //LegacyMainWindow window = new LegacyMainWindow();
                MainWindow window = new MainWindow();

                // Add the mod list to the listview
                window.SetupModList(mods);

                App app = new App();
                app.Run(window);
            }
            else
            {
                bool consoleEnabled = true;

                foreach (string a in args)
                {
                    if (a == "-s" || a == "--silent")
                    {
                        consoleEnabled = false;
                        break;
                    }
                }

                if (consoleEnabled)
                    AllocConsole();

                RyuHelpers.Program.Main(args).Wait();

                if (consoleEnabled)
                    FreeConsole();
            }
        }


        private static void CheckForUpdatesGUI()
        {
            string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string updaterPath = Path.Combine(currentPath, "RyuUpdater.exe");
            string updateFlagPath = Path.Combine(currentPath, "update.txt");
            bool updaterResult = false;
            if (File.Exists(updaterPath))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(updaterPath);
                string version = versionInfo.FileVersion;
                updaterResult = UpdateUpdater(updaterPath, version);
            }
            else //Updater not present. Download latest
            {
                updaterResult = UpdateUpdater(updaterPath);
            }

            if (updaterResult)
            {
                Process proc = new Process();
                proc.StartInfo.FileName = updaterPath;
                proc.StartInfo.Arguments = $"-v {Util.GetAppVersion()} -c";
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.Start();
                proc.WaitForExit();
            }

            if (File.Exists(updateFlagPath))
            {
                string updateVersion = File.ReadAllText(updateFlagPath);
                File.Delete(updateFlagPath);
                MessageBoxResult result = MessageBox.Show($"Shin Ryu Mod Manager version {updateVersion} is available for download.\nWould you like to update now?", "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    Process proc = new Process();
                    proc.StartInfo.FileName = updaterPath;
                    proc.StartInfo.Arguments = $"-v {Util.GetAppVersion()}";
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    proc.Start();
                    Environment.Exit(0x55504454); //UPDT
                }
                else if (result == MessageBoxResult.No)
                {
                    return;
                }
            }
        }


        private static bool UpdateUpdater(string updaterPath, string currentVersion = "0.0.0")
        {
            try
            {
                WebClient client = new WebClient();
                string yamlString = client.DownloadString($"https://raw.githubusercontent.com/{RyuHelpers.Program.AUTHOR}/{RyuHelpers.Program.UPDATE_INFO_REPO}/main/{RyuHelpers.Program.UPDATE_INFO_FILE_PATH}");

                var deserializer = new DeserializerBuilder().Build();
                var yamlObject = deserializer.Deserialize<Updater>(yamlString);

                bool isHigher = Util.CompareVersionIsHigher(yamlObject.Version, currentVersion);
                if (isHigher)
                {
                    client.DownloadFile(yamlObject.Download, updaterPath);
                    MessageBox.Show($"RyuUpdater has been updated to version {yamlObject.Version}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                client.Dispose();
                return true;
            }
            catch (WebException)
            {
                MessageBox.Show("Could not fetch update data.\nThis could be a problem with your internet connection or GitHub.\nPlease try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }

    public class Updater
    {
        public string Version { get; set; }
        public string Download {  get; set; }
    }
}
