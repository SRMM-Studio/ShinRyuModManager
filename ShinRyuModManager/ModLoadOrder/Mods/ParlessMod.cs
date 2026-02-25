using Serilog;
using Utils;

namespace ShinRyuModManager.ModLoadOrder.Mods;

public class ParlessMod() : Mod(Constants.PARLESS_NAME)
{
    public List<ParlessFolder> ParlessFolders { get; } = [];
    
    public new void PrintInfo()
    {
        if (ParlessFolders.Count > 0)
        {
            Log.Information("Added {ParlessFoldersCount} .parless path(s)", ParlessFolders.Count);
        }
        
        base.PrintInfo();
    }
    
    public new void AddFiles(string path, string check)
    {
        if (string.IsNullOrEmpty(check))
        {
            check = CheckFolder(path);
        }
        
        var index = path.IndexOf(".parless", StringComparison.Ordinal);
        
        if (index != -1)
        {
            // Call the base class AddFiles method
            base.AddFiles(path, check);
            
            // Remove ".parless" from the path
            path = path.Remove(index, 8);
            
            // Add .parless folders to the list to make it easier to check for them in the ASI
            var loosePath = GamePath.RemoveParlessPath(path);
            var folder = new ParlessFolder(loosePath, index - GamePath.DataPath.Length);
            
            ParlessFolders.Add(folder);
            
            Log.Verbose("Adding .parless path: {LoosePath}", loosePath);
        }
        else
        {
            // Continue recursing until we find the next ".parless"
            foreach (var folder in Directory.GetDirectories(path))
            {
                AddFiles(folder, check);
            }
        }
    }
}
