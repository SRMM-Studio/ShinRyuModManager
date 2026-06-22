using CpkTools.Endian;
using CpkTools.Model;
using Serilog;
using ShinRyuModManager.Extensions;
using Utils;

namespace ShinRyuModManager.ModLoadOrder;

// Intended only for OE bgm/se.cpk
internal static class CpkPatcher
{
    public static async Task RepackDictionary(Dictionary<string, List<string>> cpkDict)
    {
        if (cpkDict.IsNullOrEmpty())
        {
            return;
        }
        
        Log.Information("Repacking CPKs...");
        
        Directory.CreateDirectory(GamePath.ParlessDir);
        
        foreach (var kvp in cpkDict)
        {
            var key = kvp.Key.Trim(Path.DirectorySeparatorChar);
            
            var cpkDir = Path.Combine(GamePath.ParlessDir, key);
            
            string origCpk;
            
            if (!key.Contains(".cpk"))
            {
                origCpk = Path.Combine(GamePath.DataPath, $"{key}.cpk");
            }
            else
            {
                origCpk = Path.Combine(GamePath.DataPath, key);
            }
            
            if (!Directory.Exists(cpkDir))
                Directory.CreateDirectory(cpkDir);
            
            foreach (var mod in kvp.Value)
            {
                //var modCpkDir = Path.Combine(GamePath.ModsPath, mod).Replace(".cpk", "");
                var searchDir = Path.Combine(GamePath.ModsPath, mod, key);
                var cpkFiles = Directory.EnumerateFiles(searchDir, "*.", SearchOption.AllDirectories);
                
                foreach (var file in cpkFiles)
                {
                    File.Copy(file, Path.Combine(cpkDir, Path.GetFileName(file)), true);
                }
            }
            
            Modify(origCpk, cpkDir, new DirectoryInfo(cpkDir).FullName + ".cpk");
        }
        
        await Task.CompletedTask;
    }
    
    private static void Modify(string inputCpk, string replaceDir, string outputCpk)
    {
        var cpk = new Cpk();
        
        cpk.ReadCpk(inputCpk);
        
        using var oldFile = new EndianReader(File.OpenRead(inputCpk), true);
        
        var files = Directory.EnumerateFiles(replaceDir, "*.");
        var fileNames = new HashSet<string>();
        
        foreach (var str in files.Select(Path.GetFileNameWithoutExtension))
        {
            fileNames.Add(str);
        }
        
        using var newCpk = new EndianWriter(File.OpenWrite(outputCpk), true);
        
        var entries = cpk.FileTable.OrderBy(x => x.FileOffset);
        
        foreach (var entry in entries)
        {
            if (entry.FileType == "FILE" && (ulong)newCpk.Position < cpk.ContentOffset)
            {
                var padLength = cpk.ContentOffset - (ulong)newCpk.Position;
                
                newCpk.Write(new byte[padLength]);
            }
            
            if (!fileNames.Contains(entry.FileName))
            {
                oldFile.Seek((long)entry.FileOffset, SeekOrigin.Begin);
                
                entry.FileOffset = (ulong)newCpk.Position;
                cpk.UpdateFileEntry(entry);
                
                _ = oldFile.ReadStreamInto(newCpk.BaseStream, (int)entry.FileSize);
            }
            else
            {
                using var newbie = File.OpenRead(Path.Combine(replaceDir, entry.FileName));
                
                entry.FileOffset = (ulong)newCpk.Position;
                entry.FileSize = (ulong)newbie.Length;
                entry.ExtractSize = (ulong)newbie.Length;
                cpk.UpdateFileEntry(entry);
                
                newCpk.Write(newbie);
            }
            
            if ((newCpk.Position % 0x800) > 0)
            {
                var padding = (int)(0x800 - (newCpk.Position % 0x800));
                
                newCpk.Write(new byte[padding]);
            }
        }
        
        cpk.WriteCpk(newCpk);
        cpk.WriteItoc(newCpk);
        cpk.WriteToc(newCpk);
        cpk.WriteEtoc(newCpk);
        cpk.WriteGtoc(newCpk);
        
        Log.Information("Writing {FileName}", Path.GetFileName(inputCpk));
    }
}
