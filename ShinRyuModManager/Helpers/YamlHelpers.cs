using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace ShinRyuModManager.Helpers;

public static class YamlHelpers
{
    /// <summary>
    /// Deserializes the text from the given path to the specified object type
    /// </summary>
    public static T DeserializeYaml<T>(string yamlString)
    {
        var deserializer = new DeserializerBuilder().Build();
        var yamlObject = deserializer.Deserialize<T>(yamlString);
        
        return yamlObject;
    }
    
    /// <summary>
    /// Deserializes the text from the given path to the specified object type
    /// </summary>
    public static T DeserializeYamlFromPath<T>(string path)
    {
        var yamlString = File.ReadAllText(path);
        var deserializer = new DeserializerBuilder().Build();
        var yamlObject = deserializer.Deserialize<T>(yamlString);
        
        return yamlObject;
    }
    
    /// <summary>
    /// Deserializes the text from the given path to the specified object type
    /// </summary>
    public static async Task<T> DeserializeYamlFromPathAsync<T>(string path)
    {
        var yamlString = await File.ReadAllTextAsync(path);
        var deserializer = new DeserializerBuilder().Build();
        var yamlObject = deserializer.Deserialize<T>(yamlString);
        
        return yamlObject;
    }
    
    // TODO: Might want to update from Plain to DoubleQuote. More robust.
    public static string SerializeObject(object obj, ScalarStyle style = ScalarStyle.Plain)
    {
        var serializer = new SerializerBuilder().WithDefaultScalarStyle(style).Build();
        
        return serializer.Serialize(obj);
    }
}
