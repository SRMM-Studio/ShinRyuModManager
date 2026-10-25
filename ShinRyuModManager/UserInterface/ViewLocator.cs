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
            LibraryDisplayControlViewModel => new LibraryDisplayControl(),
            _ => new TextBlock
            {
                Text = $"View not found for {data.GetType().Name}"
            }
        };
    }
    
    public bool Match(object data) => data is ViewModelBase;
}
