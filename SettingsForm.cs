using System;
using System.Windows.Forms;

namespace VK_Unicorn
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        void SettingsForm_Shown(object sender, EventArgs e)
        {
            StopWordsTextBox.Text = Constants.DEFAULT_STOP_WORDS;

            Database.Instance.ForSettings((settings) =>
            {
                ApplicationIdTextBox.Text = settings.ApplicationId != null ? settings.ApplicationId : "";
                LoginTextBox.Text = settings.Login != null ? settings.Login : "";
                PasswordTextBox.Text = settings.Password != null ? settings.Password : "";
                CityIdNumericUpDown.Value = settings.CityId;
                StopWordsTextBox.Text = settings.StopWords != null ? settings.StopWords : Constants.DEFAULT_STOP_WORDS;
            });
        }

        void ApplyButton_Click(object sender, EventArgs e)
        {
            // Save settings
            Database.Instance.SaveSettings(new Database.Settings
            {
                ApplicationId = ApplicationIdTextBox.Text,
                Login = LoginTextBox.Text,
                Password = PasswordTextBox.Text,
                CityId = Decimal.ToInt32(CityIdNumericUpDown.Value),
                StopWords = StopWordsTextBox.Text,
            });

            Close();
        }

        void CancelAndCloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        void CreateAppLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://vk.com/editapp?act=create");
        }

        void CityIdLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://forum.botreg.ru/index.php?/topic/330-kak-uznat-id-goroda-vkontakte/");
        }

        void SettingsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainForm.Instance.ShowErrorIfSettingsAreInvalid();
        }
    }
}