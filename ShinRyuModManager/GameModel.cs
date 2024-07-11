using ParLibrary;
using ParLibrary.Converter;
using System;
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

            foreach(var thing in mlo.Files)
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
