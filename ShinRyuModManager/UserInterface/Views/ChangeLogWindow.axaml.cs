using Avalonia.Controls;
using Avalonia.Interactivity;
using ShinRyuModManager.UserInterface.ViewModels;

namespace ShinRyuModManager.UserInterface.Views;

public partial class ChangeLogWindow : Window
{
    public ChangeLogWindow()
    {
        DataContext = new ChangeLogWindowViewModel();
        
        InitializeComponent();
    }
    
    private void Close_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
