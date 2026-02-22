using ParLibrary.Converter;
using Serilog;
using ShinRyuModManager.Extensions;
using Utils;
using Yarhl.FileSystem;
using Yarhl.IO;

namespace ShinRyuModManager;

public static class ParRepacker
{
    private static void DeleteDirectory(string targetDirectory)
    {
        File.SetAttributes(targetDirectory, FileAttributes.Normal);
        
        var files = Directory.GetFiles(targetDirectory);
        var dirs = Directory.GetDirectories(targetDirectory);
        
        foreach (var file in files)
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }
        
        foreach (var dir in dirs)
        {
            DeleteDirectory(dir);
        }
        
        Directory.Delete(targetDirectory, false);
    }
    
    public static void RemoveOldRepackedPars()
    {
        if (!Directory.Exists(GamePath.ParlessDir))
            return;
        
        Log.Information("Removing old pars...");
        
        try
        {
            DeleteDirectory(GamePath.ParlessDir);
        }
        catch
        {
            Log.Warning("Failed to remove old pars! {PathToParlessMods}", GamePath.ParlessDir);
        }
        
        Log.Information("Removed old pars.");
    }
    
    public static async Task RepackDictionary(Dictionary<string, List<string>> parDictionary)
    {
        var parTasks = new List<Task>();
        
        if (parDictionary.IsNullOrEmpty())
        {
            Log.Information("No pars to repack.");
            
            return;
        }
        
        Log.Information("Repacking pars...");
        
        foreach (var parModPair in parDictionary)
        {
            parTasks.Add(Task.Run(() => RepackPar(parModPair.Key, parModPair.Value)));
        }
        
        await Task.WhenAll(parTasks);
        
        Log.Information("Repacked {ParDictionaryCount} par(s)!", parDictionary.Count);
    }
    
    private static void RepackPar(string parPath, List<string> mods)
    {
        parPath = parPath.TrimStart(Path.DirectorySeparatorChar);
        
        var parPathReal = GamePath.GetRootParPath(parPath + ".par");
        var pathToPar = Path.Combine(GamePath.DataPath, parPathReal);
        var pathToModPar = Path.Combine(GamePath.ParlessDir, parPath + ".par");
        
        if (string.IsNullOrEmpty(parPathReal))
        {
            Log.Information("ParRepacker: ParPathReal is empty for {ParPath} - skipping", parPath);
            
            return;
        }
        
        // Check if actual repackable par is nested
        if (parPath + ".par" != parPathReal)
        {
            // -4 to skip ".par", +1 to skip the directory separator
            parPathReal = parPath[(parPathReal.Length - 4 + 1)..] + ".par";
        }
        else
        {
            // Add the directory separators to properly search for the nodes
            parPathReal = Path.DirectorySeparatorChar + parPathReal;
        }
        
        // Normalize directory separators
        parPathReal = Utils.NormalizeToNodePath(parPathReal);
        
        // Dictionary of fileInPar, ModName
        var fileDict = new Dictionary<string, string>();
        
        Log.Information("Repacking {ParPath}.par...", parPath);
        
        // Populate fileDict with the files inside each mod
        foreach (var mod in mods)
        {
            foreach (var modFile in GetModFiles(parPath, mod))
            {
                fileDict.TryAdd(modFile, mod);
            }
        }
        
        var pathToTempPar = pathToModPar + "temp";
        
        // Make sure that the .partemp directory is empty
        if (File.Exists(pathToModPar))
        {
            File.Delete(pathToModPar);
        }
        
        // Make sure that the .partemp directory is empty
        if (Directory.Exists(pathToTempPar))
        {
            DeleteDirectory(pathToTempPar);
        }
        
        Directory.CreateDirectory(pathToTempPar);
        
        // Copy each file in the mods to the .partemp directory
        foreach (var fileModPair in fileDict)
        {
            string fileInModFolder;
            
            if (fileModPair.Value.StartsWith(Constants.PARLESS_NAME))
            {
                // 15 = ParlessMod.NAME.Length + 1
                fileInModFolder = Path.Combine(GamePath.DataPath, parPath.Insert(int.Parse(fileModPair.Value[15..]) - 1, ".parless"), fileModPair.Key);
            }
            else
            {
                fileInModFolder = GamePath.GetModPathFromDataPath(fileModPair.Value, Path.Combine(parPath, fileModPair.Key));
            }
            
            var fileInTempFolder = Path.Combine(pathToTempPar, fileModPair.Key);
            
            Directory.GetParent(fileInTempFolder)?.Create();
            File.Copy(fileInModFolder, fileInTempFolder, true);
        }
        
        var readerParams = new ParArchiveReaderParameters
        {
            Recursive = true
        };
        
        var writerParams = new ParArchiveWriterParameters
        {
            CompressorVersion = 0,
            OutputPath = pathToModPar,
            IncludeDots = true
        };
        
        // Create a node from the .partemp directory and write the par to pathToModPar
        var nodeName = new DirectoryInfo(pathToTempPar).Name;
        var node = ReadDirectory(pathToTempPar, nodeName);
        
        // Store a reference to the nodes in the container to dispose of them later, as they are not disposed properly
        var containerNode = node.GetFormatAs<NodeContainerFormat>();
        var par = NodeFactory.FromFile(pathToPar, FileOpenMode.Read);
        
        par.TransformWith<ParArchiveReader, ParArchiveReaderParameters>(readerParams);
        
        Node searchResult = null;
        
        // Search for the par if it's not the actual loaded par
        if (par.Name != new FileInfo(parPathReal).Name)
        {
            searchResult = SearchParNode(par, parPathReal.ToLowerInvariant());
            
            if (searchResult == null)
            {
                // Create empty node to transfer the children to
                searchResult = NodeFactory.CreateContainer("empty");
                searchResult.Add(NodeFactory.CreateContainer("."));
            }
            
            // Swap the search result and its parent
            (par, searchResult) = (searchResult, par);
        }
        
        writerParams.IncludeDots = par.Children[0].Name == ".";
        writerParams.ResetFileDates = true;
        
        containerNode!.MoveChildrenTo(writerParams.IncludeDots ? par.Children[0] : par, true);
        par.SortChildren((x, y) => string.CompareOrdinal(x.Name.ToLowerInvariant(), y.Name.ToLowerInvariant()));
        
        writerParams.IncludeDots = false;
        
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(pathToModPar))!);
        par.TransformWith<ParArchiveWriter, ParArchiveWriterParameters>(writerParams);
        
        par.Dispose();
        searchResult?.Dispose();
        node.Dispose();
        containerNode.Dispose();
        
        // Remove the .partemp directory
        DeleteDirectory(pathToTempPar);
        
        Log.Information("Repacked {FileDictCount} file(s) in {ParPath}!", fileDict.Count, parPath + ".par");
    }
    
    private static List<string> GetModFiles(string par, string mod)
    {
        List<string> result;
        
        if (mod.StartsWith(Constants.PARLESS_NAME))
        {
            // Get index of ".parless" in par path
            // 15 = ParlessMod.NAME.Length + 1
            result = GetModFiles(Path.Combine(GamePath.DataPath, par.Insert(int.Parse(mod[15..]) - 1, ".parless")));
        }
        else
        {
            result = GetModFiles(GamePath.GetModPathFromDataPath(mod, par));
        }
        
        // Get file path relative to par
        return result.Select(f => f.Replace(".parless", "")[(f.Replace(".parless", "").IndexOf(par, StringComparison.Ordinal) + par.Length + 1)..]).ToList();
    }
    
    private static List<string> GetModFiles(string path)
    {
        List<string> files = [];
        
        // Add files in current directory
        foreach (var p in Directory.EnumerateFiles(path).Where(f => !f.EndsWith(Constants.VORTEX_MANAGED_FILE)).Select(GamePath.GetDataPathFrom))
        {
            files.Add(p);
            Log.Verbose("Adding file: {File}", p);
        }
        
        // Get files for all subdirectories
        foreach (var folder in Directory.EnumerateDirectories(path))
        {
            files.AddRange(GetModFiles(folder));
        }
        
        return files;
    }
    
    public static Node ReadDirectory(string dirPath, string nodeName = "")
    {
        dirPath = Path.GetFullPath(dirPath);
        
        if (string.IsNullOrEmpty(nodeName))
        {
            nodeName = Path.GetFileName(dirPath);
        }
        
        var container = NodeFactory.CreateContainer(nodeName);
        var directoryInfo = new DirectoryInfo(dirPath);
        
        container.Tags["DirectoryInfo"] = directoryInfo;
        
        foreach (var file in directoryInfo.EnumerateFiles())
        {
            var fileNode = NodeFactory.FromFile(file.FullName);
            
            container.Add(fileNode);
        }
        
        foreach (var directory in directoryInfo.EnumerateDirectories())
        {
            var directoryNode = ReadDirectory(directory.FullName);
            
            container.Add(directoryNode);
        }
        
        return container;
    }
    
    private static Node SearchParNode(Node node, string path)
    {
        var paths = path.Split(NodeSystem.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
        
        return paths.Length == 0
            ? null
            : SearchParNode(node, paths, 0);
    }
    
    private static Node SearchParNode(Node node, string[] paths, int pathIndex)
    {
        if (pathIndex >= paths.Length)
        {
            return node;
        }
        
        var path = paths[pathIndex];
        
        foreach (var child in node.Children)
        {
            if (child.Name == ".")
            {
                return SearchParNode(child, paths, pathIndex);
            }
            
            if (child.Name == path || child.Name == path + ".par")
            {
                return SearchParNode(child, paths, pathIndex + 1);
            }
        }
        
        return null;
    }
}
