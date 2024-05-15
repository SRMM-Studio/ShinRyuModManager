using System.Windows;

namespace ShinRyuModManager
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        private string text;
        private bool isIndeterminate;

        public ProgressWindow(string text = "", bool isIndeterminate = true)
        {
            InitializeComponent();
            this.text = text;
            this.isIndeterminate = isIndeterminate;
            Init();
        }


        private void Init()
        {
            if (text != "")
            {
                tb_Text.Text = text;
                tb_Text.Visibility = Visibility.Visible;
            }

            pb_ProgressBar.IsIndeterminate = isIndeterminate;
        }
    }
}
