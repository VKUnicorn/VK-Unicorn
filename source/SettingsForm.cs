using System;
using System.Windows.Forms;

namespace VK_Unicorn
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();

            // Отключаем ненужные up\down элементы
            ApplicationIdUpDown.Controls[0].Enabled = false;
            CityIdNumericUpDown.Controls[0].Enabled = false;
        }

        void SettingsForm_Shown(object sender, EventArgs e)
        {
            PasswordWarningLabel.Text += " " + Constants.DATABASE_FILENAME;

            // Значения по умолчанию для некоторых полей
            StopWordsTextBox.Text = Constants.DEFAULT_STOP_WORDS;
            SearchByCityRadioButton.Checked = true;

            // Загружаем настройки
            Database.Instance.For<Database.Settings>(Database.INTERNAL_DB_MARKER, (settings) =>
            {
                ApplicationIdUpDown.Value = settings.ApplicationId;
                LoginTextBox.Text = settings.Login != null ? settings.Login : "";
                PasswordTextBox.Text = settings.Password != null ? settings.Password : "";
                CityIdNumericUpDown.Value = settings.CityId;
                StopWordsTextBox.Text = settings.StopWords != null ? settings.StopWords : Constants.DEFAULT_STOP_WORDS;

                switch (settings.SearchMethod)
                {
                    case Database.Settings.SearchMethodType.SMART:
                        SearchSmartRadioButton.Checked = true;
                        break;

                    case Database.Settings.SearchMethodType.ALL_OF_TARGET_SEX:
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
            // Определяем метод поиска пользователей
            var searchMethod = Database.Settings.SearchMethodType.BY_CITY;
            if (SearchSmartRadioButton.Checked)
            {
                searchMethod = Database.Settings.SearchMethodType.SMART;
            }
            if (SearchAllRadioButton.Checked)
            {
                searchMethod = Database.Settings.SearchMethodType.ALL_OF_TARGET_SEX;
            }

            // Сохраняем настройки в базу
            Database.Instance.InsertOrReplace(new Database.Settings
            {
                Id = Database.INTERNAL_DB_MARKER,
                ApplicationId = Decimal.ToInt64(ApplicationIdUpDown.Value),
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