using System;
using System.IO;
using System.Windows;

namespace ShinRyuModManager
{
    /// <summary>
    /// Interaction logic for ChangelogWindow.xaml
    /// </summary>
    public partial class ChangelogWindow : Window
    {
        public ChangelogWindow()
        {
            InitializeComponent();
            SetChangelog();
        }


        private void SetChangelog()
        {
            var uri = new Uri("pack://application:,,,/Resources/changelog.md");
            var resourceStream = Application.GetResourceStream(uri);
            using (var reader = new StreamReader(resourceStream.Stream))
            {
                var text = reader.ReadToEnd();
                mdview_Changelog.Markdown = text;
            }
        }


        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
