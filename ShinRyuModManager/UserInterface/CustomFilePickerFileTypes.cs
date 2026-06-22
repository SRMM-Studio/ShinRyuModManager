using Avalonia.Platform.Storage;

namespace ShinRyuModManager.UserInterface;

public static class CustomFilePickerFileTypes
{
    public static FilePickerFileType CompressedZip { get; } = new("Zip Archive")
    {
        Patterns = ["*.zip", "*.7z"],
        AppleUniformTypeIdentifiers = ["public.zip-archive", "org.7-zip.7-zip-archive"],
        MimeTypes = ["application/zip", "application/x-7z-compressed"]
    };
    
    public static FilePickerFileType CompressedRar { get; } = new("Rar Archive")
    {
        Patterns = ["*.rar"],
        AppleUniformTypeIdentifiers = ["com.rarlab.rar-archive"],
        MimeTypes = ["application/x-rar-compressed", "application/vnd.rar", "application/x-rar"]
    };
    
    public static FilePickerFileType CompressedGzip { get; } = new("Gzip Archive")
    {
        Patterns = ["*.gz"],
        AppleUniformTypeIdentifiers = ["org.gnu.gnu-zip-archive"],
        MimeTypes = ["application/gzip"]
    };
    
    public static FilePickerFileType YamlFile { get; } = new("YAML File")
    {
        Patterns = ["*.yaml"],
        AppleUniformTypeIdentifiers = ["public.yaml"],
        MimeTypes = ["application/yaml"]
    };
}
