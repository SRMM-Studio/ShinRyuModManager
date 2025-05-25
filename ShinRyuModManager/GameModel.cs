using ParLibrary;
using ParLibrary.Converter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utils;
using Yarhl.FileSystem;

namespace ShinRyuModManager
{
    public static class GameModel
    {
        public static bool SupportsUBIK(Game game)
        {
            return game >= Game.LostJudgment && game != Game.eve;
        }


        //Jhrino: Make adding new hact files (not replacing) less painful
        //Yakuza 5 is quirky in the sense that accessing the loose files wont be enough
        //Things have to be duplicated in places like this
        //1) Folder in root hact folder (hact/h1000_my_cool_new_hact)
        //2) Par in root hact folder (hact/h1000_my_cool_new_hact.par)
        //3) Folder + par in new hact folder (hact/h1000_my_cool_new_hact/000   AND   hact/h1000_my_cool_new_hact/000.par)
        //This can seriously bloat mod size. Instead of making modders duplicate their hacts like this
        //Lets do this for them (because we are nice)
        public static void DoY5HActProcedure(MLO mlo)
        {
            bool hasHacts = false;

            HashSet<string> hactDirs = new HashSet<string>();

            foreach (var kv in mlo.Files)
                if (kv.Item1.Contains("/hact/") && !kv.Item1.EndsWith(".par"))
                {
                    string mod = mlo.Mods[kv.Item2];

                    string filePath = "mods/" + mod + "/" + kv.Item1;

                    FileInfo file = new FileInfo(filePath);

                    //get the folder from a path like this -> data/hact/h5000_some_hact/cmn/cmn.bin
                    string hactDir = file.Directory.Parent.Name;

                    if (file.Directory.Parent.Parent.Name == "hact")
                    {
                        string hactName = file.Directory.Parent.Name;
                        string hactPath = file.Directory.Parent.FullName;

                        if (hactDirs.Contains(hactPath))
                            continue;

                        bool isVanillaHact = GamePath.ExistsInDataAsPar(file.Directory.Parent.FullName);

                        if (isVanillaHact)
                            continue;

                        hactDirs.Add(hactPath);
                    }
                    hasHacts = true;
                }

            if (!hasHacts)
                return;

            foreach (string hactDirPath in hactDirs)
            {
                DirectoryInfo hactDir = new DirectoryInfo(hactDirPath);
                DirectoryInfo parlessDir = new DirectoryInfo(Path.Combine(GamePath.GetModsPath(), "Parless", "hact", hactDir.Name));

                if (!parlessDir.Exists)
                    parlessDir.Create();

                foreach (DirectoryInfo dir in hactDir.GetDirectories())
                {
                    //We already repack ptc 
                    if (dir.Name == "ptc" && File.Exists(Path.Combine(hactDir.FullName, "ptc.par")))
                        continue;

                    string outputPath = Path.Combine(parlessDir.FullName, dir.Name + ".par");
                    Gibbed.Yakuza0.Pack.Program.Main(new string[] { dir.FullName }, outputPath);
                    //Miscellaneous.GibbedPar.GibbedPacker.Pack(dir.FullName, Path.Combine(parlessDir.FullName, dir.Name + ".par"));

                    /*
                    var parameters = new ParArchiveWriterParameters
                    {
                        CompressorVersion = 0,
                        OutputPath = Path.Combine(parlessDir.FullName, dir.Name + ".par"),
                        IncludeDots = true
                    };

                    string nodeName = dir.Name;
                    Node node = Util.ReadDirectory(dir.FullName, nodeName);
                    node.SortChildren((x, y) => string.CompareOrdinal(x.Name.ToLowerInvariant(), y.Name.ToLowerInvariant()));

                    var result = node.TransformWith<ParArchiveWriter, ParArchiveWriterParameters>(parameters);
                    result.Dispose();
                    */
                }


                Gibbed.Yakuza0.Pack.Program.Main(new string[] { parlessDir.FullName }, Path.Combine(parlessDir.Parent.FullName, hactDir.Name + ".par"));


                /*
                var finalPar = new ParArchiveWriterParameters
                {
                    CompressorVersion = 0,
                    OutputPath = Path.Combine(parlessDir.Parent.FullName, hactDir.Name + ".par"),
                    IncludeDots = true
                };

                Node rootDir = Util.ReadDirectory(parlessDir.FullName, hactDir.Name);
                rootDir.SortChildren((x, y) => string.CompareOrdinal(x.Name.ToLowerInvariant(), y.Name.ToLowerInvariant()));
                rootDir.TransformWith<ParArchiveWriter, ParArchiveWriterParameters>(finalPar);
                rootDir.Stream.Close();
                rootDir.Dispose();
                */

            }
        }

        //Gotta be a better way to do this
        public static void DoUBIKProcedure(MLO mlo)
        {
            string charaPath = Path.Combine("data/chara.par");

            if (!File.Exists(charaPath))
                return;

            bool hasUbiks = false;

            foreach (var kv in mlo.Files)
                if (kv.Item1.EndsWith(".ubik"))
                {
                    hasUbiks = true;
                    break;
                }

            if (!hasUbiks)
                return;

            string ubikDir = Path.Combine(Constants.PARLESS_MODS_PATH, "ubik");

            var Par = NodeFactory.FromFile(charaPath, "par");
            Par.TransformWith<ParArchiveReader, ParArchiveReaderParameters>(new ParArchiveReaderParameters() { Recursive = true });

            Node ubik = Navigator.IterateNodes(Par).FirstOrDefault(x => x.Path.EndsWith("ubik"));

            if (!Directory.Exists(Constants.PARLESS_MODS_PATH))
                Directory.CreateDirectory(Constants.PARLESS_MODS_PATH);

            if (!Directory.Exists(ubikDir))
                Directory.CreateDirectory(ubikDir);

            foreach (Node node in ubik.Children)
            {
                var ubikFile = node.GetFormatAs<ParFile>();

                if (ubikFile.IsCompressed)
                    node.TransformWith<ParLibrary.Sllz.Decompressor>();

                string filePath = Path.Combine(ubikDir, node.Name);

                if (node.Stream.Length > 0)
                    node.Stream.WriteTo(filePath);
            }

            foreach (var thing in mlo.Files)
            {
                if (thing.Item1.EndsWith(".ubik"))
                {
                    string path = Path.Combine("mods", mlo.Mods[thing.Item2] + thing.Item1);
                    File.Copy(path, Path.Combine(ubikDir, Path.GetFileName(thing.Item1)), true);
                }
            }

            Par.Dispose();
        }

    }
}
