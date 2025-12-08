using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class GamePath
    {
        public const string DATA = "data";
        public const string MODS = "mods";
        public const string LIBRARIES = "srmm-libs";

        private static Game? currentGame = null;

        public static string GetBasename(string path)
        {
            return Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar));
        }

        public static string GetGamePath()
        {
            return Directory.GetCurrentDirectory();
        }

        public static string GetDataPath()
        {
            return Path.Combine(GetGamePath(), DATA);
        }

        public static string GetModsPath()
        {
            return Path.Combine(GetGamePath(), MODS);
        }

        public static string GetExternalModsPath()
        {
            return Path.Combine(GetModsPath(), Constants.EXTERNAL_MODS);
        }

        public static string GetLibrariesPath()
        {
            return Path.Combine(GetGamePath(), LIBRARIES);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Given path but starting after /mods/ModName/ </returns>
        public static string RemoveModPath(string path)
        {
            return path.Substring(path.IndexOf(Path.DirectorySeparatorChar, path.IndexOf("mods" + Path.DirectorySeparatorChar) + 5));
        }

        public static string RemoveParlessPath(string path)
        {
            path = path.Replace(".parless", "");

            return path.Substring(path.IndexOf("data" + Path.DirectorySeparatorChar) + 4);
        }

        public static string GetDataPathFrom(string path)
        {
            if (path.Contains(".parless"))
            {
                // Preserve .parless in path instead of removing it
                return path.Substring(path.IndexOf("data" + Path.DirectorySeparatorChar) + 4);
            }

            return RemoveModPath(path);
        }

        public static string GetModPathFromDataPath(string mod, string path)
        {
            return Path.Combine(GetModsPath(), mod, path.TrimStart(Path.DirectorySeparatorChar));
        }

        public static bool FileExistsInData(string path)
        {
            return File.Exists(Path.Combine(GetDataPath(), path.TrimStart(Path.DirectorySeparatorChar)));
        }

        public static bool DirectoryExistsInData(string path)
        {
            return Directory.Exists(Path.Combine(GetDataPath(), path.TrimStart(Path.DirectorySeparatorChar)));
        }

        public static string GetRootParPath(string path)
        {
            if (!path.Contains(Path.DirectorySeparatorChar))
            {
                return FileExistsInData(path) ? path : "";
            }

            if (FileExistsInData(path))
            {
                return path;
            }

            return GetRootParPath(path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar)) + ".par");
        }

        public static bool ExistsInDataAsParNested(string path)
        {
            if (path.Contains(".parless"))
            {
                // Remove ".parless"
                return GetRootParPath(RemoveParlessPath(path) + ".par") != "";
            }

            // Add ".par"
            return GetRootParPath(RemoveModPath(path) + ".par") != "";
        }

        public static bool ExistsInDataAsPar(string path)
        {
            if (path.Contains(".parless"))
            {
                // Remove ".parless"
                return FileExistsInData(RemoveParlessPath(path) + ".par");
            }

            // Add ".par"
            return FileExistsInData(RemoveModPath(path) + ".par");
        }

        public static bool IsXbox(string path)
        {
            return path.Contains("Xbox") || path.Contains("WindowsApps") || path.Contains(Path.DirectorySeparatorChar + "Content" + Path.DirectorySeparatorChar) || File.Exists(Path.Combine(path, "MicrosoftGame.config"));
        }

        public static Game GetGame()
        {
            if (!currentGame.HasValue)
            {
                foreach (string file in Directory.GetFiles(GetGamePath(), "*.exe"))
                {
                    if (Enum.TryParse(Path.GetFileNameWithoutExtension(file), out Game game))
                    {
                        currentGame = game;
                        break;
                    }
                }

                currentGame ??= Game.Unsupported;
            }

            return currentGame.Value;
        }

        public static string GetGameFriendlyName(Game g) {
            return g switch
            {
                Game.Yakuza3 => "Yakuza 3 Remastered",
                Game.Yakuza4 => "Yakuza 4 Remastered",
                Game.Yakuza5 => "Yakuza 5 Remastered",
                Game.Yakuza0 => "Yakuza 0",
                Game.yakuza0_dc => "Yakuza 0: Director's Cut",
                Game.YakuzaKiwami => "Yakuza Kiwami",
                Game.Yakuza6 => "Yakuza 6",
                Game.YakuzaKiwami2 => "Yakuza Kiwami 2",
                Game.YakuzaLikeADragon => "Yakuza: Like a Dragon",
                Game.Judgment => "Judgment",
                Game.LostJudgment => "Lost Judgment",
                Game.likeadragongaiden => "Like a Dragon Gaiden: The Man Who Erased His Name",
                Game.likeadragon8 => "Like a Dragon: Infinite Wealth",
                Game.likeadragonpirates => "Like a Dragon: Pirate Yakuza In Hawaii",
                Game.VFREVOBETA => "Virtua Fighter 5 R.E.V.O. Beta",
                Game.VFREVO => "Virtua Fighter 5 R.E.V.O.",
                Game.StrangerThanHeaven => "Stranger Than Heaven",
                Game.YakuzaKiwami3 => "Yakuza Kiwami 3",
                _ => "<unknown>"
            };
        }

        public static string GetGameExe()
        {
            return currentGame.ToString() + ".exe";
        }
    }
}
