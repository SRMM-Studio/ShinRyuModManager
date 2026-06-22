using System.Diagnostics;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using ShinRyuModManager.UserInterface.ViewModels;

namespace ShinRyuModManager.UserInterface.Views;

// Avalonia doesn't have a native MessageBox concept (after 9 years...). So we get to create our own.
public partial class MessageBoxWindow : Window
{
    [GeneratedRegex(@"\[([^\]]+)\]\((https?://[^\)]+)\)")] // Matches "[link text](link)"
    private static partial Regex LinkRegex();
    
    // ReSharper disable once MemberCanBePrivate.Global
    public MessageBoxWindow()
    {
        InitializeComponent();
    }
    
    private MessageBoxWindow(string message, bool showCancel = false, bool dontRemind = false) : this()
    {
        DataContext = new MessageBoxWindowViewModel(showCancel, dontRemind);
        
        BuildMessage(message);
    }
    
    public static async Task<MessageBoxResult> Show(Window owner, string title, string message, bool showCancel, bool dontRemind = false)
    {
        var win = new MessageBoxWindow(message, showCancel, dontRemind)
        {
            Title = title,
            Icon = owner.Icon,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        
        if (owner.Icon != null)
        {
            win.Icon = owner.Icon;
        }
        
        return await win.ShowDialog<MessageBoxResult>(owner);
    }
    
    public static async Task Show(Window owner, string title, string message)
    {
        await Show(owner, title, message, false);
    }
    
    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        Close(MessageBoxResult.Ok);
    }
    
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close(MessageBoxResult.Cancel);
    }
    
    private void DontRemind_Click(object sender, RoutedEventArgs e)
    {
        Close(MessageBoxResult.DontRemind);
    }
    
    // Builds the message allowing for regex-like links
    private void BuildMessage(string message)
    {
        MessageTextBlock.Inlines?.Clear();
        
        var lastIndex = 0;
        
        foreach (Match match in LinkRegex().Matches(message))
        {
            // Add plain text first
            if (match.Index > lastIndex)
            {
                MessageTextBlock.Inlines?.Add(new Run(message[lastIndex..match.Index]));
            }
            
            var displayText = match.Groups[1].Value;
            var url = match.Groups[2].Value;
            
            var linkText = new TextBlock
            {
                Text = displayText,
                Foreground = new SolidColorBrush(Color.Parse("#FF6CB2F7")),
                TextDecorations = TextDecorations.Underline,
                Cursor = new Cursor(StandardCursorType.Hand)
            };
            
            linkText.PointerPressed += (_, _) =>
            {
                Process.Start(new ProcessStartInfo(url)
                {
                    UseShellExecute = true
                });
            };
            
            var link = new InlineUIContainer
            {
                Child = linkText,
                BaselineAlignment = BaselineAlignment.Bottom
            };
            
            MessageTextBlock.Inlines?.Add(link);
            
            lastIndex = match.Index + match.Length;
        }
        
        // Add remaining text
        if (lastIndex < message.Length)
        {
            MessageTextBlock.Inlines?.Add(new Run(message[lastIndex..]));
        }
    }
}
