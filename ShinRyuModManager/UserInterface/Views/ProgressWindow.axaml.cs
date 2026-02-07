using Avalonia.Controls;
using ShinRyuModManager.UserInterface.ViewModels;

namespace ShinRyuModManager.UserInterface.Views;

public partial class ProgressWindow : Window
{
    public ProgressWindow()
    {
        InitializeComponent();
    }
    
    public ProgressWindow(string text, bool isIndeterminate) : this()
    {
        DataContext = new ProgressWindowViewModel("Please wait...", text, isIndeterminate);
    }
    
    public new Task ShowDialog(Window owner)
    {
        Closing += OnClosing;
        
        return base.ShowDialog(owner);
    }
    
    public new void Close()
    {
        Closing -= OnClosing;
        
        base.Close();
    }
    
    private static void OnClosing(object sender, WindowClosingEventArgs e)
    {
        e.Cancel = true;
    }
}
