using Avalonia.Controls;
using ShinRyuModManager.UserInterface.ViewModels;

namespace ShinRyuModManager.UserInterface.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        DataContext = new AboutWindowViewModel();
        
        InitializeComponent();
    }
}
