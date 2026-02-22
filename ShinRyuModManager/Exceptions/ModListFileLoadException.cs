namespace ShinRyuModManager.Exceptions;

public class ModListFileLoadException : Exception
{
    public ModListFileLoadException() { }
    
    public ModListFileLoadException(string message) : base(message) { }
    
    public ModListFileLoadException(string message, Exception inner) : base(message, inner) { }
}
