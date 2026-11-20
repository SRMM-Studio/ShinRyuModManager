using System.Text;
using IniParser;
using IniParser.Exceptions;
using IniParser.Model;
using Serilog;
using ShinRyuModManager.Config.Models;
using Utils;

namespace ShinRyuModManager.Config;

public static class ConfigManager
{
    private static readonly Lock LockObj = new();
    private static readonly FileIniDataParser IniParser = new();
    
    // Always replace, never mutate
    public static ParlessConfig Current { get; private set; } = new();
    
    public static event Action<ParlessConfig> Changed;
    
    public static void Load(string path = Constants.INI)
    {
        Log.Debug("Loading config file {Ini}", path);
        
        if (!File.Exists(path))
        {
            Log.Information("{Ini} was not found. Creating default ini...", path);
            
            Current = new ParlessConfig();
            
            Write(path, IniMapper.ToIni(Current));
            
            return;
        }
        
        IniData data;
        
        try
        {
            data = IniParser.ReadFile(path);
        }
        catch (ParsingException ex)
        {
            Log.Warning(ex, "{Ini} could not be read. Creating default ini...", path);
            
            var backupPath = $"{Path.GetFullPath(path)}.bak";
            
            File.Move(path, backupPath, true);
            
            Current = new ParlessConfig();
            
            Write(path, IniMapper.ToIni(Current));
            
            return;
        }
        
        var oldVersion = 0;
        
        if (data.TryGetKey("Parless.IniVersion", out var version) 
            && int.TryParse(version, out var num))
        {
            oldVersion = num;
        }
        
        var config = IniMapper.Load(data);
        
        if (oldVersion < ParlessConfig.CURRENT_VERSION)
        {
            Log.Information("{Ini} is outdated. Updating ini to the latest version...", path);
            
            Migrate(config, oldVersion);
        }
        
        config.Parless.IniVersion = ParlessConfig.CURRENT_VERSION;
        
        if (ConfigIsDifferent(config, path))
        {
            Write(path, IniMapper.ToIni(config));
            
            Log.Information("Updated {Ini}", path);
        }
        
        Current = config;
    }
    
    public static void Save(ParlessConfig config, string path = Constants.INI)
    {
        lock (LockObj)
        {
            config.Parless.IniVersion = ParlessConfig.CURRENT_VERSION;
            
            var changed = ConfigIsDifferent(config, path);
            
            Current = config;
            
            if (!changed)
                return;
            
            Write(path, IniMapper.ToIni(config));
            
            Changed?.Invoke(config);
        }
    }
    
    public static void Update(Action<ParlessConfig> edit)
    {
        lock (LockObj)
        {
            var config = CloneCurrent();
            
            edit(config);
            
            Save(config);
        }
    }
    
    public static ParlessConfig CloneCurrent()
    {
        return IniMapper.Load(IniMapper.ToIni(Current));
    }
    
    private static void Migrate(ParlessConfig config, int oldVersion)
    {
        // Update if ini version is old
        if (oldVersion <= 3)
        {
            config.Overrides.RebuildMlo = true;
        }
    }
    
    // "Atomic" write. If something goes wrong, don't corrupt existing config
    private static void Write(string path, IniData data)
    {
        var tmp = $"{path}.tmp";
        
        File.WriteAllText(tmp, data.ToString(), new UTF8Encoding(false));
        
        File.Move(tmp, path, true);
    }
    
    private static bool ConfigIsDifferent(ParlessConfig config, string path)
    {
        if (!File.Exists(path))
            return true;
        
        var normalized = IniMapper.ToIni(config);
        
        return Normalize(File.ReadAllText(path)) != Normalize(normalized.ToString());
    }
    
    private static string Normalize(string str)
    {
        return str.ReplaceLineEndings("\n").TrimEnd();
    }
}
