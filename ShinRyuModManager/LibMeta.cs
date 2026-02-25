using JetBrains.Annotations;
using ShinRyuModManager.Helpers;
using Utils;

namespace ShinRyuModManager;

[UsedImplicitly]
public class LibMeta
{
    public Guid GUID { get; private set; }
    public string Name { get; set; }
    public string Version { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public bool CanBeDisabled { get; set; }
    public bool IsDisabled { get; set; }
    
    /// <summary>
    /// A semicolon (;) separated list of game executable names.
    /// </summary>
    public string TargetGames { get; set; }
    
    public string Source { get; set; }
    public string Download { get; set; }
    public string MainBinary { get; set; }
    
    private const string URL = $"https://raw.githubusercontent.com/{Constants.LIBRARIES_INFO_REPO_OWNER}/{Constants.LIBRARIES_INFO_REPO}/main/{Constants.LIBRARIES_INFO_REPO_FILE_PATH}";
    
    public static LibMeta ReadLibMeta(string yamlString)
    {
        return YamlHelpers.DeserializeYaml<LibMeta>(yamlString);
    }
    
    public static List<LibMeta> Fetch()
    {
        var yamlString = Utils.Client.GetStringAsync(URL).GetAwaiter().GetResult();
        
        var localManifestCopyPath = GamePath.LocalLibrariesPath;
        
        if (!File.Exists(localManifestCopyPath) && !Utils.IsFileLocked(localManifestCopyPath))
        {
            File.WriteAllText(localManifestCopyPath!, yamlString);
        }
        
        return ReadLibMetaManifest(yamlString);
    }
    
    public static async Task<List<LibMeta>> FetchAsync()
    {
        var yamlString = await Utils.Client.GetStringAsync(URL);
        
        var localManifestCopyPath = GamePath.LocalLibrariesPath;
        
        if (!File.Exists(localManifestCopyPath) && !Utils.IsFileLocked(localManifestCopyPath))
        {
            await File.WriteAllTextAsync(localManifestCopyPath!, yamlString);
        }
        
        return ReadLibMetaManifest(yamlString);
    }
    
    public static List<LibMeta> ReadLibMetaManifest(string yamlString)
    {
        var returnList = new List<LibMeta>();
        var yamlObject = YamlHelpers.DeserializeYaml<Dictionary<string, LibMeta>>(yamlString);
        
        foreach (var key in yamlObject.Keys)
        {
            var meta = yamlObject[key];
            
            meta.GUID = Guid.Parse(key);
            
            returnList.Add(meta);
        }
        
        Program.LibraryMetaCache = returnList;
        
        return returnList;
    }
}
