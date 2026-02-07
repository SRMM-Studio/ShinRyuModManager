using Avalonia.Controls;
using Serilog;
using ShinRyuModManager.Helpers;
using ShinRyuModManager.UserInterface.UserControls;
using ShinRyuModManager.UserInterface.ViewModels;
using Utils;

namespace ShinRyuModManager.UserInterface.Views;

public partial class LibraryManagerWindow : Window
{
    public LibraryManagerWindow()
    {
        DataContext = new LibraryManagerViewModel();
        
        InitializeComponent();
        
        PopulateLibraryList();
    }
    
    private void PopulateLibraryList()
    {
        if (DataContext is not LibraryManagerViewModel viewModel)
            return;
        
        viewModel.Library.Clear();
        
        try
        {
            foreach (var meta in DownloadLibraryData())
            {
                if (!string.IsNullOrEmpty(meta.TargetGames))
                {
                    var game = GamePath.GameExe.ToLowerInvariant().Replace(".exe", "");
                    var targets = meta.TargetGames.ToLowerInvariant().Replace(" ", "").Split(';');
                    
                    if (targets.Contains(game))
                    {
                        viewModel.Library.Add(new LibraryDisplayControl(meta));
                    }
                }
                else
                {
                    // No targets specified. Assume it works with everything (?)
                    viewModel.Library.Add(new LibraryDisplayControl(meta));
                }
            }
        }
        catch (Exception ex)
        {
            // Fetching library data from GitHub failed. Connection issues or server down?
            // Populate the list with data from the already installed libraries in case the user wants to uninstall or disable any
            
            Log.Fatal(ex, "Failed to fetch library data!");
            
            var metaList = new List<LibMeta>();
            
            foreach (var dir in Directory.GetDirectories(GamePath.LibrariesPath))
            {
                var path = Path.Combine(dir, Constants.LIBRARIES_LIBMETA_FILE_NAME);
                
                if (!File.Exists(path))
                    continue;
                
                var meta = YamlHelpers.DeserializeYamlFromPath<LibMeta>(path);
                
                metaList.Add(meta);
            }
            
            foreach (var meta in metaList)
            {
                viewModel.Library.Add(new LibraryDisplayControl(meta));
            }
        }
    }
    
    private static List<LibMeta> DownloadLibraryData()
    {
        return LibMeta.Fetch();
    }
}
