namespace Utils;

public static class Flags
{
    public static bool CheckFlag(string flagName)
    {
        var flagFilePath = Path.Combine(Environment.CurrentDirectory, flagName);
        
        return File.Exists(flagFilePath);
    }
    
    public static void CreateFlag(string flagName)
    {
        if (CheckFlag(flagName))
            return;
        
        var flagFilePath = Path.Combine(Environment.CurrentDirectory, flagName);
        
        File.Create(flagFilePath);
        File.SetAttributes(flagFilePath, File.GetAttributes(flagFilePath) | FileAttributes.Hidden);
    }
    
    public static void DeleteFlag(string flagName)
    {
        if (!CheckFlag(flagName))
            return;
        
        var flagFilePath = Path.Combine(Environment.CurrentDirectory, flagName);
        
        File.Delete(flagFilePath);
    }
}
