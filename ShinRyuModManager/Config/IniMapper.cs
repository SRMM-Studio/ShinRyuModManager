using System.Globalization;
using System.Reflection;
using IniParser.Model;
using ShinRyuModManager.Attributes;
using ShinRyuModManager.Config.Models;

namespace ShinRyuModManager.Config;

public static class IniMapper
{
    public static ParlessConfig Load(IniData data)
    {
        var config = new ParlessConfig();
        
        foreach (var (sectionProp, sectionAttr) in GetSections())
        {
            var sectionObj = sectionProp.GetValue(config)!;
            
            foreach (var (keyProp, keyAttr) in GetKeys(sectionProp))
            {
                if (data.TryGetKey($"{sectionAttr.Name}.{keyAttr.Name}", out var raw)
                    && TryParse(raw, keyProp.PropertyType, out var value))
                {
                    keyProp.SetValue(sectionObj, value);
                }
            }
        }
        
        return config;
    }
    
    public static IniData ToIni(ParlessConfig config)
    {
        var data = new IniData
        {
            Configuration =
            {
                AssigmentSpacer = string.Empty
            }
        };
        
        foreach (var (sectionProp, sectionAttr) in GetSections())
        {
            var sectionObj = sectionProp.GetValue(config)!;
            var defaults = Activator.CreateInstance(sectionProp.PropertyType)!;
            
            var section = new SectionData(sectionAttr.Name);
            
            section.Comments.AddRange(Lines(sectionAttr.Comment));
            
            foreach (var (keyProp, keyAttr) in GetKeys(sectionProp))
            {
                var key = new KeyData(keyAttr.Name)
                {
                    Value = Serialize(keyProp.GetValue(sectionObj))
                };
                
                key.Comments.AddRange(Lines(keyAttr.Comment));
                key.Comments.Add($" Default={Serialize(keyProp.GetValue(defaults))}");
                
                section.Keys.AddKey(key);
            }
            
            data.Sections.Add(section);
        }
        
        return data;
    }
    
    private static string Serialize(object val)
    {
        return val switch
        {
            bool b => b ? "1" : "0",
            Enum e => Convert.ToInt32(e).ToString(),
            _ => Convert.ToString(val, CultureInfo.InvariantCulture) ?? string.Empty
        };
    }
    
    private static bool TryParse(string raw, Type type, out object value)
    {
        value = null;
        
        if (type == typeof(bool))
        {
            if (raw is not ("0" or "1"))
                return false;
            
            value = raw is "1";
            
            return true;
        }
        
        if (type.IsEnum)
        {
            if (!int.TryParse(raw, out var i) || !Enum.IsDefined(type, i))
                return false;
            
            value = Enum.ToObject(type, i);
            
            return true;
        }
        
        if (type == typeof(int))
        {
            if (!int.TryParse(raw, out var num))
                return false;
            
            value = num;
            
            return true;
        }
        
        if (type == typeof(string))
        {
            value = raw;
            
            return true;
        }
        
        return false;
    }
    
    private static IEnumerable<string> Lines(string str)
    {
        return (str ?? string.Empty)
               .Split('\n', StringSplitOptions.RemoveEmptyEntries)
               .Select(x => " " + x);
    }
    
    private static IEnumerable<(PropertyInfo Prop, IniSectionAttribute Attr)> GetSections()
    {
        var properties = typeof(ParlessConfig).GetProperties().OrderBy(x => x.MetadataToken);
        
        return properties
               .Select(x => (x, x.GetCustomAttribute<IniSectionAttribute>()))
               .Where(x => x.Item2 is not null);
    }
    
    private static IEnumerable<(PropertyInfo Prop, IniKeyAttribute Attr)> GetKeys(PropertyInfo section)
    {
        var properties = section.PropertyType.GetProperties().OrderBy(x => x.MetadataToken);
        
        return properties
               .Select(x => (x, x.GetCustomAttribute<IniKeyAttribute>()))
               .Where(x => x.Item2 is not null);
    }
}
