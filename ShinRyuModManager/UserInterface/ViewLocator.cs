using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ShinRyuModManager.UserInterface.UserControls;
using ShinRyuModManager.UserInterface.ViewModels;
using ShinRyuModManager.UserInterface.Views;

namespace ShinRyuModManager.UserInterface;

public class ViewLocator : IDataTemplate
{
    public Control Build(object data)
    {
        return data switch
        {
            MainWindowViewModel vm => new MainWindow
            {
                DataContext = vm
            },
            AboutWindowViewModel vm => new AboutWindow
            {
                DataContext = vm
            },
            ChangeLogWindowViewModel vm => new ChangeLogWindow
            {
                DataContext = vm
            },
            LibraryDisplayControlViewModel vm => new LibraryDisplayControl
            {
                DataContext = vm
            },
            LibraryManagerViewModel vm => new LibraryManagerWindow
            {
                DataContext = vm
            },
            MessageBoxWindowViewModel vm => new MessageBoxWindow
            {
                DataContext = vm
            },
            ProgressWindowViewModel vm => new ProgressWindow
            {
                DataContext = vm
            },
            _ => new TextBlock
            {
                Text = $"View not found for {data.GetType().Name}"
            }
        };
    }
    
    public bool Match(object data) => data is ViewModelBase;
}
