using System.IO;
using System.Collections.Generic;
using System.Linq;
using Utils;
using System;

namespace ShinRyuModManager.ModLoadOrder.Mods
{
    public class Mod
    {
        protected readonly ConsoleOutput console;

        public string Name { get; set; }

        /// <summary>
        /// Files that can be directly loaded from the mod path.
        /// </summary>
        public List<string> Files { get; }

        /// <summary>
        /// Folders that have to be repacked into pars before running the game.
        /// </summary>
        public List<string> ParFolders { get; }

        /// <summary>
        /// Folders that need to be bound as a directory to a CPK binder.
        /// </summary>
        public List<string> CpkFolders { get; }

        /// <summary>
        /// Folders that need to be repacked.
        /// </summary>
        public List<string> RepackCPKs { get; }

        public Mod(string name, int indent = 2)
        {
            this.Name = name;
            this.Files = new List<string>();
            this.ParFolders = new List<string>();
            this.CpkFolders = new List<string>();
            this.RepackCPKs = new List<string>();

            this.console = new ConsoleOutput(indent);
            this.console.WriteLine($"Reading directory: {name} ...");
        }

        public void PrintInfo()
        {
            this.console.WriteLineIfVerbose();

            if (this.Files.Count > 0 || this.ParFolders.Count > 0)
            {
                if (this.Files.Count > 0)
                {
                    this.console.WriteLine($"Added {this.Files.Count} file(s)");
                }

                if (this.ParFolders.Count > 0)
                {
                    this.console.WriteLine($"Added {this.ParFolders.Count} folder(s) to be repacked");
                }

                if (this.CpkFolders.Count > 0)
                {
                    this.console.WriteLine($"Added {this.CpkFolders.Count} CPK folder(s) to be bound");
                }
            }
            else
            {
                this.console.WriteLine($"Nothing found for {this.Name}, skipping");
            }

            this.console.Flush();
        }

        public void AddFiles(string path, string check, Game game)
        {
            bool needsRepack = false;
            string basename = GamePath.GetBasename(path);
            string parentDir = new DirectoryInfo(path).Parent.Name;

            // We dont want to do all these checks for contents in the Parless mod folder.
            if (Name != "Parless")
            {
                // Check if this path does not need repacking
                switch (check)
                {

                   // case "motion":
                        //needsRepack = true;
                      //  break;
                    case "chara":
                        needsRepack = GamePath.ExistsInDataAsPar(path);
                        break;
                    case "map_":
                        needsRepack = GamePath.ExistsInDataAsPar(path);
                        break;
                    case "effect":
                        needsRepack = GamePath.ExistsInDataAsPar(path);
                        break;
                    case "prep":
                        needsRepack = game < Game.Yakuza0 && GamePath.ExistsInDataAsPar(path);
                        break;
                    case "light_anim":
                        needsRepack = game < Game.Yakuza0 && GamePath.ExistsInDataAsPar(path);
                        break;
                    case "2d":
                        needsRepack = (basename.StartsWith("sprite") || basename.StartsWith("pj")) && GamePath.ExistsInDataAsParNested(path);
                        break;
                    case "cse":
                        needsRepack = (basename.StartsWith("sprite") || basename.StartsWith("pj")) && GamePath.ExistsInDataAsParNested(path);
                        break;
                    case "pausepar":
                        if (GamePath.GetGame() >= Game.Yakuza0)
                            needsRepack = true;
                        else
                            needsRepack = !basename.StartsWith("pause") && GamePath.ExistsInDataAsPar(path);
                        break;
                    case "pausepar_e":
                        needsRepack = !basename.StartsWith("pause") && GamePath.ExistsInDataAsPar(path);
                        break;
                    case "particle":
                        if (game >= Game.Yakuza6 && basename == "arc")
                        {
                            check = "particle/arc";
                        }

                        if (new DirectoryInfo(path).Parent.Name == "arc_list")
                            needsRepack = true;
                        break;
                    case "particle/arc":
                        needsRepack = GamePath.ExistsInDataAsParNested(path);
                        break;
                    case "stage":
                        needsRepack = game == Game.eve && basename == "sct" && GamePath.ExistsInDataAsParNested(path);
                        break;
                    case "":
                        needsRepack = (basename == "ptc" && GamePath.ExistsInDataAsParNested(path))
                            || (basename == "entity_adam" && GamePath.ExistsInDataAsPar(path));

                        if (!needsRepack)
                        {
                            check = this.CheckFolder(basename);
                        }
                        break;
                    default:
                        break;
                }

                // Check for CPK directories
                string cpkDataPath;
                switch (basename)
                {
                    case "bgm":
                        if (game <= Game.yakuzakiwami_r)
                        {
                            cpkDataPath = GamePath.RemoveModPath(path);
                            this.RepackCPKs.Add(cpkDataPath);
                        }
                        break;

                    case "se":
                    case "speech":
                        cpkDataPath = GamePath.RemoveModPath(path);
                        if (game == Game.Yakuza5)
                        {
                            this.RepackCPKs.Add(cpkDataPath + ".cpk");
                            //this.CpkFolders.Add(cpkDataPath + ".cpk");
                            //this.console.WriteLineIfVerbose($"Adding CPK folder: {cpkDataPath}");
                        }
                        else
                        {
                            if (game <= Game.yakuzakiwami_r)
                                this.RepackCPKs.Add(cpkDataPath + ".cpk");
                        }

                        break;
                    case "stream":
                    case "stream_en":
                    case "stmdlc":
                    case "stmdlc_en":
                    case "movie":
                    case "moviesd":
                    case "moviesd_dlc":
                        cpkDataPath = GamePath.RemoveModPath(path);
                        if (game == Game.Judgment || game == Game.LostJudgment)
                        {
                            this.CpkFolders.Add(cpkDataPath + ".par");
                            this.console.WriteLineIfVerbose($"Adding CPK folder: {cpkDataPath}");
                        }

                        break;
                    case "gv_files":
                        cpkDataPath = GamePath.RemoveModPath(path);
                        this.CpkFolders.Add(cpkDataPath + ".cpk");
                        this.console.WriteLineIfVerbose($"Adding CPK folder: {cpkDataPath}");
                        break;
                    default:
                        break;
                }

                if (parentDir != basename)
                {
                    switch (parentDir)
                    {
                        case "motion":
                            //if (game == Game.Yakuza5)
                               // needsRepack = GamePath.ExistsInDataAsPar(path) && basename.ToLowerInvariant().Contains("Battle");
                            break;
                    }
                }

                if(game >= Game.Yakuza6)
                {
                    //Dragon Engine talks use pars directly for these
                    if (path.Contains("talk_"))
                    {
                        if (char.IsDigit(basename[0]) || check == "cmn")
                            needsRepack = true;
                        else
                        {
                            string tCmn = Path.Combine(path, "cmn");
                            string t000 = Path.Combine(path, "000");

                            if (Directory.Exists(tCmn) && Directory.Exists(t000))
                                needsRepack = true;
                        }
                    }
                }

                if(game >= Game.likeadragonpirates)
                {
                    // Additional game specific checks
                    switch (basename)
                    {
                        // Pirates in Hawaii stores gmts inside folders based on the lowercase filename checksum.
                        // For the modders' convenience, move any gmts in the folder root to the corresponding subdirectory.
                        case "motion":
                            {
                                string gmtFolderPath = Path.Combine(path, "gmt");
                                if (!Directory.Exists(gmtFolderPath)) break;
                                string baseParlessPath = Path.Combine(GamePath.GetModsPath(), "Parless", "motion", "gmt");
                                foreach (string p in Directory.GetFiles(gmtFolderPath).Where(f => !f.EndsWith(Constants.VORTEX_MANAGED_FILE)).Select(f => GamePath.GetDataPathFrom(f)))
                                {
                                    // Copy any gmts to the appropriate hash folder in Parless
                                    if (!p.EndsWith(".gmt", StringComparison.InvariantCultureIgnoreCase)) continue;

                                    string fileName = Path.GetFileName(p);
                                    string fileName2 = Path.GetFileNameWithoutExtension(p);
                                    string gmtHashName = fileName2.Length <= 30 ? fileName2 : fileName2.Substring(0, 30);

                                    string gmtPath = Path.Combine(gmtFolderPath, fileName);
                                    string checksum = ((Func<string, string>)(s => (System.Text.Encoding.UTF8.GetBytes(gmtHashName).Sum(b => b) % 256).ToString("x2").PadLeft(4, '0')))(Path.GetFileNameWithoutExtension(p).ToLowerInvariant());
                                    string destinationDirectory = Path.Combine(baseParlessPath, checksum);
                                    if (!Directory.Exists(destinationDirectory))
                                        Directory.CreateDirectory(destinationDirectory);
                                    File.Copy(gmtPath, Path.Combine(destinationDirectory, Path.GetFileName(gmtPath)));
                                }
                                break;
                            }
                    }
                }
            }


            if (needsRepack)
            {
                string dataPath = GamePath.GetDataPathFrom(path);

                // Add this folder to the list of folders to be repacked and stop recursing
                this.ParFolders.Add(dataPath);
                this.console.WriteLineIfVerbose($"Adding repackable folder: {dataPath}");
            }
            else
            {
                // Add files in current directory
                foreach (string p in Directory.GetFiles(path).Where(f => !f.EndsWith(Constants.VORTEX_MANAGED_FILE)).Select(f => GamePath.GetDataPathFrom(f)))
                {
                    this.Files.Add(p);
                    this.console.WriteLineIfVerbose($"Adding file: {p}");
                }

                var isParlessMod = this.GetType() == typeof(ParlessMod);

                // Get files for all subdirectories
                foreach (string folder in Directory.GetDirectories(path))
                {
                    // Break an important rule in the concept of inheritance to make the program function correctly
                    if (isParlessMod)
                    {
                        ((ParlessMod)this).AddFiles(folder, check);
                    }
                    else
                    {
                        this.AddFiles(folder, check, game);
                    }
                }
            }
        }

        protected string CheckFolder(string name)
        {
            foreach (string folder in Constants.IncompatiblePars)
            {
                if (name.StartsWith(folder))
                {
                    return folder;
                }
            }

            return "";
        }
    }
}
