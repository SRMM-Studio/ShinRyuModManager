using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ShinRyuModManager.Miscellaneous.Fools24
{
    /// <summary>
    /// Don't say anything if you find this ahead of time ;)
    /// </summary>
    public partial class Fools24Window : Window
    {
        HashSet<string> Licenses;
        int FailCounter = 0;
        int FailLimit = 5;

        public Fools24Window()
        {
            InitializeComponent();
            txt_LicenseKey.MaxLength = 23;

            Licenses = new HashSet<string>
            {
                "PXD00-02024-PROD2-85095",
                "BL1K3-A000D-R4G0N-02024",
                "RYU10-5ACBC-YK300-02026",
                "NAG00-MBA32-01965-STDIO",
                "A2902-M0900-P2401-T0907",
                "21374-12059-99N7E-17R11",
                "11037-TBMAM-TEIHH-DVIII",
                "99905-23052-3MUVC-103LV",
                "PASSW-ARDSA-LTSAL-T0000",
                "KEY00-NOT00-WORK0-HELP0",
                "NOT00-FUNNY-TOBEH-ONEST",
                "SHIGE-KIBAB-AMOVE-SET00",
                "BEYTA-YAGAM-I0IS0-BEST0",
                "WHYWO-ULDYO-UMAKE-THIS0",
                "AAAAA-AAAAA-AAAAA-AAAAA",
                "11111-11111-11111-11111",
                "00000-00000-00000-00000",
                "RHPQ2-RMFJH-74XYM-BH4JX",
                "JUSTL-ETMEI-NSTAL-LMODS",
                "ANGER-YBACK-WHEN0-PLS00",
                "KIRYU-SAVE0-ME000-PLEAZ",
                "DEAD0-SOULS-KIWAM-I0PLS",
            };
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (LicenseCheck()) e.Cancel = false;
            else e.Cancel = true;
        }


        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }


        private void txt_LicenseKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = txt_LicenseKey.Text.ToUpper();
            text = text.Replace("-", "");

            if (text.Length > 5)
            {
                for (int i = 5; i < text.Length; i += 6)
                {
                    text = text.Insert(i, "-");
                }
            }

            txt_LicenseKey.Text = text;
            txt_LicenseKey.CaretIndex = txt_LicenseKey.Text.Length;
        }


        private bool LicenseCheck()
        {
            if (Licenses.Contains(txt_LicenseKey.Text))
            {
                return true;
            }
            else
            {
                if (FailCounter >= FailLimit)
                {
                    return true;
                } else
                {
                    return false;
                }
            }
        }


        private void btn_ValidateLicense_Click(object sender, RoutedEventArgs e)
        {
            if (!LicenseCheck())
            {
                lbl_InvalidLicense.Visibility = Visibility.Visible;
                FailCounter++;
            }
            Close();
        }
    }
}
