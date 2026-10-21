using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ShinRyuModManager;

public partial class DllSearchHardening
{
#if WINDOWS || WINDOWS_SLIM
    internal static string InitError { get; private set; } 
    
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetDefaultDllDirectories(int directoryFlags);
    
    private static readonly HashSet<string> KnownInjectionDlls = new(StringComparer.OrdinalIgnoreCase)
    {
        "d3d8", "d3d9", "d3d10", "d3d11", "d3d12", "dxgi",
        "ddraw", "dinput", "dinput8", "dsound",
        "msacm32", "msvfw32", "version", "wininet", "winmm", "winhttp",
        "xlive", "binkw32", "bink2w32", "binkw64", "bink2w64", "vorbisFile",
        "xinput1_1", "xinput1_2", "xinput1_3", "xinput1_4", "xinput9_1_0", "xinputuap"
    };
    
    private const int LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800;
    
    [ModuleInitializer]
    internal static void Init()
    {
        AppDomain.CurrentDomain.AssemblyLoad += (_, e) => Hook(e.LoadedAssembly);
        
        // Try to prevent DLL hijacking by limiting the DLL search path to System32. This should avoid the GUI from crashing due to mod injections.
        // https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-setdefaultdlldirectories
        if (!SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_SYSTEM32))
        {
            InitError = $"Failed to set DLL search path.\nError: {Marshal.GetLastWin32Error()}";
        }
        
        // Explicitly force DLLs used by Ultimate ASI Loader to load from %windows%\system32
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Hook(assembly);
        }
    }
    
    private static void Hook(Assembly assembly)
    {
        try
        {
            NativeLibrary.SetDllImportResolver(assembly, Resolve);
        }
        catch (InvalidOperationException)
        {
            // Ignore
        }
    }
    
    private static IntPtr Resolve(string name, Assembly assembly, DllImportSearchPath? path)
    {
        var bareDll = Path.GetFileNameWithoutExtension(name);
        
        if (!KnownInjectionDlls.Contains(bareDll))
            return IntPtr.Zero;
        
        var sysPath = Path.Combine(Environment.SystemDirectory, $"{bareDll}.dll");
        
        return NativeLibrary.TryLoad(sysPath, out var handle) ? handle : IntPtr.Zero;
    }
#endif 
}