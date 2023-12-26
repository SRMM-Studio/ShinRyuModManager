using System.IO;
using System;
using System.Windows;

namespace RyuGUI
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            SetVersion();
            SetCredits();
        }

        private void SetVersion()
        {
            txt_Version.Text = $"v{Util.GetAppVersion()}";
        }

        private void SetCredits()
        {
            var uri = new Uri("pack://application:,,,/credits.txt");
            var resourceStream = Application.GetResourceStream(uri);
            using (var reader = new StreamReader(resourceStream.Stream))
            {
                var text = reader.ReadToEnd();
                txt_Credits.Text = text;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }
    }
}
