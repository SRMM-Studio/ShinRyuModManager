using Avalonia.Controls;
using Avalonia.Interactivity;
using ShinRyuModManager.UserInterface.ViewModels;

namespace ShinRyuModManager.UserInterface.Views;

// Avalonia doesn't have a native MessageBox concept (after 9 years...). So we get to create our own.
public partial class MessageBoxWindow : Window
{
    private bool _result;
    
    // ReSharper disable once MemberCanBePrivate.Global
    public MessageBoxWindow()
    {
        InitializeComponent();
    }
    
    private MessageBoxWindow(string message, bool showCancel = false) : this()
    {
        DataContext = new MessageBoxWindowViewModel(message, showCancel);
    }
    
    public static async Task<bool> Show(Window owner, string title, string message, bool showCancel = false)
    {
        var win = new MessageBoxWindow(message, showCancel)
        {
            Title = title,
            Icon = owner.Icon
        };
        
        return await win.ShowDialog<bool>(owner);
    }
    
    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        _result = true;
        Close(_result);
    }
    
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        _result = false;
        Close(_result);
    }
}
