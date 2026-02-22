using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace ShinRyuModManager.UserInterface;

public static class UiHelpers
{
    private const string USER_INTERFACE_PATH = "avares://ShinRyuModManager/UserInterface/Assets/";
    
    /// <summary>
    /// <para>Loads the given resource as a <see cref="Bitmap"/>.</para>
    /// <para>NOTE: The path root is "UserInterface/Assets/"</para>
    /// </summary>
    public static Bitmap LoadResourceAsBitmap(string path)
    {
        var modImageUri = new Uri($"{USER_INTERFACE_PATH}{path}");
        
        using var modImageStream = AssetLoader.Open(modImageUri);
        
        return new Bitmap(modImageStream);
    }
    
    /// <summary>
    /// <para>Loads the given resource as a <see cref="Bitmap"/>.</para>
    /// <para>NOTE: The path root is "UserInterface/Assets/"</para>
    /// </summary>
    public static async Task<Bitmap> LoadResourceAsBitmapAsync(string path)
    {
        var modImageUri = new Uri($"{USER_INTERFACE_PATH}{path}");
        
        await using var modImageStream = AssetLoader.Open(modImageUri);
        
        return new Bitmap(modImageStream);
    }
    
    /// <summary>
    /// <para>Loads the given resource as a <see cref="MemoryStream"/>.</para>
    /// <para>NOTE: The path root is "UserInterface/Assets/"</para>
    /// </summary>
    public static MemoryStream LoadResourceAsStream(string path)
    {
        var ms = new MemoryStream();
        var modImageUri = new Uri($"{USER_INTERFACE_PATH}{path}");
        
        using var modImageStream = AssetLoader.Open(modImageUri);
        modImageStream.CopyTo(ms);
        
        ms.Seek(0, SeekOrigin.Begin);
        
        return ms;
    }
    
    /// <summary>
    /// <para>Loads the given resource as a <see cref="MemoryStream"/>.</para>
    /// <para>NOTE: The path root is "UserInterface/Assets/"</para>
    /// </summary>
    public static async Task<MemoryStream> LoadResourceAsStreamAsync(string path)
    {
        var ms = new MemoryStream();
        var modImageUri = new Uri($"{USER_INTERFACE_PATH}{path}");
        
        await using var modImageStream = AssetLoader.Open(modImageUri);
        await modImageStream.CopyToAsync(ms);
        
        ms.Seek(0, SeekOrigin.Begin);
        
        return ms;
    }
}
