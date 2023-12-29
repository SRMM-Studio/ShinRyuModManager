using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using Utils;
using RyuHelpers.Templates;
using ParRepacker;
using ModLoadOrder.Mods;

using static ModLoadOrder.Generator;
using static Utils.GamePath;
using static Utils.Constants;

namespace RyuHelpers
{
    public static class Program
    {
        public const string VERSION = "4.0.3";
        public const string AUTHOR = "SRMM-Studio";
        public const string UPDATE_INFO_REPO = "srmm-version-info";
        public const string UPDATE_INFO_FILE_PATH = "RyuUpdater/config.yaml";

        private static bool externalModsOnly = true;
        private static bool looseFilesEnabled = false;
        private static bool checkForUpdates = true;
        private static bool isSilent = false;
        private static bool migrated = false;

        public static bool RebuildMLO = true;
        public static bool IsRebuildMLOSupported = true;

        public static async Task Main(string[] args)
        {
            Console.WriteLine($"Shin Ryu Mod Manager v{VERSION}");
            Console.WriteLine($"By Jhrino (a continuation of SutandoTsukai181's work)\n");

            // Parse arguments
            List<string> list = new List<string>(args);

            if (list.Contains("-h") || list.Contains("--help"))
            {
                Console.WriteLine("Usage: run without arguments to generate mod load order.");
                Console.WriteLine("       run with \"-s\" or \"--silent\" flag to prevent checking for updates and remove prompts.");
                Console.WriteLine("       run with \"-r\" or \"--run\" flag to run the game after the program finishes.");
                Console.WriteLine("       run with \"-h\" or \"--help\" flag to show this message and exit.");

                return;
            }

            if (list.Contains("-s") || list.Contains("--silent"))
            {
                isSilent = true;
            }

            await RunGeneration(ConvertNewToOldModList(PreRun())).ConfigureAwait(true);
            await PostRun().ConfigureAwait(true);

            if (list.Contains("-r") || list.Contains("--run"))
            {
                // Run game
                if (File.Exists(GetGameExe()))
                {
                    Console.WriteLine($"Launching \"{GetGameExe()}\"...");
                    Process.Start(GetGameExe());
                }
                else
                {
                    Console.WriteLine($"Warning: Could not run game because \"{GetGameExe()}\" does not exist.");
                }
            }
        }

        public static List<ModInfo> PreRun()
        {
            var iniParser = new FileIniDataParser();
            iniParser.Parser.Configuration.AssigmentSpacer = string.Empty;

            IniData ini;
            if (File.Exists(INI))
            {
                ini = iniParser.ReadFile(INI);

                if (ini.TryGetKey("Overrides.LooseFilesEnabled", out string looseFiles))
                {
                    looseFilesEnabled = int.Parse(looseFiles) == 1;
                }

                if (ini.TryGetKey("RyuModManager.Verbose", out string verbose))
                {
                    ConsoleOutput.Verbose = int.Parse(verbose) == 1;
                }

                if (ini.TryGetKey("RyuModManager.CheckForUpdates", out string check))
                {
                    checkForUpdates = int.Parse(check) == 1;
                }

                if (ini.TryGetKey("RyuModManager.ShowWarnings", out string showWarnings))
                {
                    ConsoleOutput.ShowWarnings = int.Parse(showWarnings) == 1;
                }

                if (ini.TryGetKey("RyuModManager.LoadExternalModsOnly", out string extMods))
                {
                    externalModsOnly = int.Parse(extMods) == 1;
                }

                if (ini.TryGetKey("Overrides.RebuildMLO", out string rebuildMLO))
                {
                    RebuildMLO = int.Parse(rebuildMLO) == 1;
                }

                if (!ini.TryGetKey("Parless.IniVersion", out string iniVersion) || int.Parse(iniVersion) < ParlessIni.CurrentVersion)
                {
                    // Update if ini version is old (or does not exist)
                    Console.Write(INI + " is outdated. Updating ini to the latest version... ");

                    if (int.Parse(iniVersion) <= 3)
                    {
                        // Force enable RebuildMLO option
                        ini.Sections["Overrides"]["RebuildMLO"] = "1";
                        RebuildMLO = true;
                    }

                    iniParser.WriteFile(INI, IniTemplate.UpdateIni(ini));
                    Console.WriteLine("DONE!\n");
                }
            }
            else
            {
                // Create ini if it does not exist
                Console.Write(INI + " was not found. Creating default ini... ");
                iniParser.WriteFile(INI, IniTemplate.NewIni());
                Console.WriteLine("DONE!\n");
            }

            if (GamePath.GetGame() != Game.Unsupported && !Directory.Exists(MODS))
            {
                // Create mods folder if it does not exist
                Console.Write($"\"{MODS}\" folder was not found. Creating empty folder... ");
                Directory.CreateDirectory(MODS);
                Console.WriteLine("DONE!\n");
            }

            // TODO: Maybe move this to a separate "Game patches" file
            // Virtua Fighter eSports crashes when used with dinput8.dll as the ASI loader
            if (GamePath.GetGame() == Game.eve && File.Exists(DINPUT8DLL))
            {
                if (File.Exists(VERSIONDLL))
                {
                    Console.Write($"Game specific patch: Deleting {DINPUT8DLL} because {VERSIONDLL} exists...");

                    // Remove dinput8.dll
                    File.Delete(DINPUT8DLL);
                }
                else
                {
                    Console.Write($"Game specific patch: Renaming {DINPUT8DLL} to {VERSIONDLL}...");

                    // Rename dinput8.dll to version.dll to prevent the game from crashing
                    File.Move(DINPUT8DLL, VERSIONDLL);
                }

                Console.WriteLine(" DONE!\n");
            }
            else if (GamePath.GetGame() >= Game.Judgment)
            {
                IsRebuildMLOSupported = false;

                // Lost Judgment (and Judgment post update 1) does not like Ultimate ASI Loader, so instead we use a custom build of DllSpoofer (https://github.com/Kazurin-775/DllSpoofer)
                if (File.Exists(DINPUT8DLL))
                {
                    Console.Write($"Game specific patch: Deleting {DINPUT8DLL} because it causes crashes with Judgment games...");

                    // Remove dinput8.dll
                    File.Delete(DINPUT8DLL);

                    Console.WriteLine(" DONE!\n");
                }

                if (!File.Exists(WINMMDLL))
                {
                    if (File.Exists(WINMMLJ))
                    {
                        Console.Write($"Game specific patch: Enabling {WINMMDLL} by renaming {WINMMLJ} to fix Judgment games crashes...");

                        // Rename dinput8.dll to version.dll to prevent the game from crashing
                        File.Move(WINMMLJ, WINMMDLL);

                        Console.WriteLine(" DONE!\n");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"WARNING: {WINMMLJ} was not found. Judgment games will NOT load mods without this file. Please redownload Shin Ryu Mod Manager.\n");
                        Console.ResetColor();
                    }
                }
            }

            // Read ini (again) to check if we should try importing the old load order file
            ini = iniParser.ReadFile(INI);

            List<ModInfo> mods = new List<ModInfo>();

            if (ShouldBeExternalOnly())
            {
                // Only load the files inside the external mods path, and ignore the load order in the txt
                mods.Add(new ModInfo(EXTERNAL_MODS));

                if (GamePath.GetGame() == Game.Judgment || GamePath.GetGame() == Game.LostJudgment)
                {
                    // Disable RebuildMLO when using an external mod manager
                    if (ini.TryGetKey("Overrides.RebuildMLO", out string _))
                    {
                        Console.Write($"Game specific patch: Disabling RebuildMLO for Judgment and Lost Judgment when using an external mod manager...");

                        ini.Sections["Overrides"]["RebuildMLO"] = "0";
                        iniParser.WriteFile(INI, ini);

                        Console.WriteLine(" DONE!\n");
                    }
                }
            }
            else
            {
                bool defaultEnabled = true;

                if (File.Exists(TXT_OLD) && ini.GetKey("SavedSettings.ModListImported") == null)
                {
                    // Scanned mods should be disabled, because that's how they were with the old txt format
                    defaultEnabled = false;

                    // Set a flag so we can delete the old file after we actually save the mod list
                    migrated = true;

                    // Migrate old format to new
                    Console.Write("Old format load order file (" + TXT_OLD + ") was found. Importing to the new format...");
                    mods.AddRange(ConvertOldToNewModList(ReadModLoadOrderTxt(TXT_OLD)).Where(n => !mods.Any(m => EqualModNames(m.Name, n.Name))));
                    Console.WriteLine(" DONE!\n");
                }
                else if (File.Exists(TXT))
                {
                    mods.AddRange(ReadModListTxt(TXT).Where(n => !mods.Any(m => EqualModNames(m.Name, n.Name))));
                }
                else
                {
                    Console.WriteLine(TXT + " was not found. Will load all existing mods.\n");
                }

                if (Directory.Exists(MODS))
                {
                    // Add all scanned mods that have not been added to the load order yet
                    Console.Write("Scanning for mods...");
                    mods.AddRange(ScanMods().Where(n => !mods.Any(m => EqualModNames(m.Name, n))).Select(m => new ModInfo(m, defaultEnabled)));
                    Console.WriteLine(" DONE!\n");
                }
            }

            if (GamePath.IsXbox(Path.Combine(GetGamePath(), GetGameExe())))
            {
                if (ini.TryGetKey("Overrides.RebuildMLO", out string _))
                {
                    Console.Write($"Game specific patch: Disabling RebuildMLO for Xbox games...");

                    ini.Sections["Overrides"]["RebuildMLO"] = "0";
                    iniParser.WriteFile(INI, ini);

                    Console.WriteLine(" DONE!\n");
                }
            }

            return mods;
        }

        public static async Task<bool> RunGeneration(List<string> mods)
        {
            if (File.Exists(MLO))
            {
                Console.Write("Removing old MLO...");

                // Remove existing MLO file to avoid it being used if a new MLO won't be generated
                File.Delete(MLO);

                Console.WriteLine(" DONE!\n");
            }

            // Remove previously repacked pars, to avoid unwanted side effects
            Repacker.RemoveOldRepackedPars();

            if (GamePath.GetGame() != Game.Unsupported)
            {
                if (mods?.Count > 0 || looseFilesEnabled)
                {
                    await GenerateModLoadOrder(mods, looseFilesEnabled).ConfigureAwait(false);
                    return true;
                }

                Console.WriteLine("Aborting: No mods were found, and .parless paths are disabled\n");
            }
            else
            {
                Console.WriteLine("Aborting: No supported game was found in this directory\n");
            }

            return false;
        }

        public static async Task PostRun()
        {
            // Check if the ASI loader is not in the directory (possibly due to incorrect zip extraction)
            if (MissingDLL())
            {
                Console.WriteLine($"Warning: \"{DINPUT8DLL}\" is missing from this directory. RyuModManager will NOT function properly without this file\n");
            }

            // Check if the ASI is not in the directory
            if (MissingASI())
            {
                Console.WriteLine($"Warning: \"{ASI}\" is missing from this directory. RyuModManager will NOT function properly without this file\n");
            }

            // Calculate the checksum for the game's exe to inform the user if their version might be unsupported
            if (ConsoleOutput.ShowWarnings && InvalidGameExe())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Warning: Game version is unsupported. Please use the latest Steam version of the game.");
                Console.WriteLine($"RyuModManager will still generate the load order, but the game might CRASH or not function properly\n");
                Console.ResetColor();
            }

            if (!isSilent)
            {
                Console.WriteLine("Program finished. Press any key to exit...");
                Console.ReadKey();
            }
        }

        public static bool ShowWarnings()
        {
            return ConsoleOutput.ShowWarnings;
        }

        public static bool MissingDLL()
        {
            return !(File.Exists(DINPUT8DLL) || File.Exists(VERSIONDLL) || File.Exists(WINMMDLL));
        }

        public static bool MissingASI()
        {
            return !File.Exists(ASI);
        }

        public static bool InvalidGameExe()
        {
            string path = Path.Combine(GetGamePath(), GetGameExe());
            return GetGame() == Game.Unsupported || !GameHash.ValidateFile(path, GetGame());
        }

        /// <summary>
        /// Read the load order from ModLoadOrder.txt (old format).
        /// </summary>
        /// <param name="txt">expected to be "ModLoadOrder.txt".</param>
        /// <returns>list of strings containing mod names according to the load order in the file.</returns>
        public static List<string> ReadModLoadOrderTxt(string txt)
        {
            List<string> mods = new List<string>();

            if (!File.Exists(txt))
            {
                return mods;
            }

            StreamReader file = new StreamReader(new FileInfo(txt).FullName);

            string line;
            while ((line = file.ReadLine()) != null)
            {
                if (!line.StartsWith(";"))
                {
                    line = line.Split(new char[] { ';' }, 1)[0];

                    // Only add existing mods that are not duplicates
                    if (line.Length > 0 && Directory.Exists(Path.Combine(MODS, line)) && !mods.Contains(line))
                    {
                        mods.Add(line);
                    }
                }
            }

            file.Close();

            return mods;
        }

        /// <summary>
        /// Read the mod list from ModList.txt (current format).
        /// </summary>
        /// <param name="txt">expected to be "ModList.txt".</param>
        /// <returns>list of ModInfo for each mod in the file.</returns>
        public static List<ModInfo> ReadModListTxt(string txt)
        {
            List<ModInfo> mods = new List<ModInfo>();

            if (!File.Exists(txt))
            {
                return mods;
            }

            StreamReader file = new StreamReader(new FileInfo(txt).FullName);

            string line = file.ReadLine();

            if (line != null)
            {
                foreach (string mod in line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (mod.StartsWith("<") || mod.StartsWith(">"))
                    {
                        ModInfo info = new ModInfo(mod.Substring(1), mod[0] == '<');

                        if (ModInfo.IsValid(info) && !mods.Contains(info))
                        {
                            mods.Add(info);
                        }
                    }
                }
            }

            file.Close();

            return mods;
        }

        public static bool SaveModList(List<ModInfo> mods)
        {
            bool result = WriteModListTxt(mods);

            if (migrated)
            {
                try
                {
                    File.Delete(TXT_OLD);

                    var iniParser = new FileIniDataParser();
                    iniParser.Parser.Configuration.AssigmentSpacer = string.Empty;

                    IniData ini = iniParser.ReadFile(INI);
                    ini.Sections.AddSection("SavedSettings");
                    ini["SavedSettings"].AddKey("ModListImported", "true");
                    iniParser.WriteFile(INI, ini);
                }
                catch
                {
                    Console.WriteLine("Could not delete " + TXT_OLD + ". This file should be deleted manually.");
                }
            }

            return result;
        }

        private static bool WriteModListTxt(List<ModInfo> mods)
        {
            // No need to write the file if it's going to be empty
            if (mods?.Count > 0)
            {
                string content = "";

                foreach (ModInfo m in mods)
                {
                    content += "|" + (m.Enabled ? "<" : ">") + m.Name;
                }

                File.WriteAllText(TXT, content.Substring(1));

                return true;
            }

            return false;
        }

        public static List<ModInfo> ConvertOldToNewModList(List<string> mods)
        {
            return mods.Select(m => new ModInfo(m)).ToList();
        }

        public static List<string> ConvertNewToOldModList(List<ModInfo> mods)
        {
            return mods.Where(m => m.Enabled).Select(m => m.Name).ToList();
        }

        public static bool ShouldBeExternalOnly()
        {
            return externalModsOnly && Directory.Exists(GetExternalModsPath());
        }

        public static bool ShouldCheckForUpdates()
        {
            return checkForUpdates;
        }

        private static List<string> ScanMods()
        {
            return Directory.GetDirectories(GetModsPath())
                .Select(d => Path.GetFileName(d.TrimEnd(new char[] { Path.DirectorySeparatorChar })))
                .Where(m => (m != "Parless") && (m != EXTERNAL_MODS))
                .ToList();
        }

        private static bool EqualModNames(string m, string n)
        {
            return string.Compare(m, n, StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }
}
