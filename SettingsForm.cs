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
            // Значения по умолчанию для некоторых полей
            StopWordsTextBox.Text = Constants.DEFAULT_STOP_WORDS;
            SearchByCityRadioButton.Checked = true;

            // Загружаем настройки
            Database.Instance.ForSettings((settings) =>
            {
                ApplicationIdTextBox.Text = settings.ApplicationId != null ? settings.ApplicationId : "";
                LoginTextBox.Text = settings.Login != null ? settings.Login : "";
                PasswordTextBox.Text = settings.Password != null ? settings.Password : "";
                CityIdNumericUpDown.Value = settings.CityId;
                StopWordsTextBox.Text = settings.StopWords != null ? settings.StopWords : Constants.DEFAULT_STOP_WORDS;

                switch (settings.SearchMethod)
                {
                    case Database.SearchMethod.SMART:
                        SearchSmartRadioButton.Checked = true;
                        break;

                    case Database.SearchMethod.ALL_FEMALES:
                        SearchAllRadioButton.Checked = true;
                        break;

                    default:
                        SearchByCityRadioButton.Checked = true;
                        break;
                }
            });
        }

        void ApplyButton_Click(object sender, EventArgs e)
        {
            // Определяем метод поиска профилей
            var searchMethod = Database.SearchMethod.BY_CITY;
            if (SearchSmartRadioButton.Checked)
            {
                searchMethod = Database.SearchMethod.SMART;
            }
            if (SearchAllRadioButton.Checked)
            {
                searchMethod = Database.SearchMethod.ALL_FEMALES;
            }

            // Сохраняем настройки в базу
            Database.Instance.SaveSettings(new Database.Settings
            {
                ApplicationId = ApplicationIdTextBox.Text,
                Login = LoginTextBox.Text,
                Password = PasswordTextBox.Text,
                CityId = Decimal.ToInt32(CityIdNumericUpDown.Value),
                StopWords = StopWordsTextBox.Text,
                SearchMethod = searchMethod,
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