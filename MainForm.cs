using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VK_Unicorn
{
    public partial class MainForm : Form
    {
        public static MainForm Instance { get; private set; }

        Database database;

        public MainForm()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            InitializeComponent();
        }

        void MainForm_Shown(object sender, EventArgs e)
        {
            Text = Constants.APP_NAME + " - " + Constants.APP_VERSION;
            VersionLabel.Text = Constants.APP_VERSION;
            Utils.Log(Text + " успешно загружен", LogLevel.SUCCESS);

            // Готовим базу данных
            Utils.Log("Подготавливаем базу данных к работе", LogLevel.NOTIFY);
            database = new Database();

            // Статистика
            database.ShowStatistics();

            // Запускаем веб сервер
            Utils.Log("Пытаемся запустить веб сервер на порт " + Constants.WEB_PORT + ". Веб сервер нужен для просмотра результатов сканирования в браузере в виде привычной веб страницы", LogLevel.NOTIFY);
            try
            {
                var listener = new WebListener(Constants.WEB_PORT);
                ShowResults.Enabled = true;
                Utils.Log("Веб сервер подключен по адресу " + Constants.RESULTS_WEB_PAGE, LogLevel.SUCCESS);
            }
            catch (System.Exception ex)
            {
                Utils.Log("Веб сервер не подключен на порт " + Constants.WEB_PORT + ". Причина: " + ex.Message, LogLevel.ERROR);
            }

            // Запускаем основной поток выполнения
            var worker = new Worker();
            worker.RunMainThread();
        }

        void ExitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        void SettingsButton_Click(object sender, EventArgs e)
        {
            OpenSettingsWindow();
        }

        void ShowResults_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Constants.RESULTS_WEB_PAGE);
        }

        void VersionLabel_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Constants.PROJECT_WEB_PAGE);
        }

        void LogTextBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        public RichTextBox GetLogTextBox()
        {
            return LogTextBox;
        }

        public void OpenSettingsWindow()
        {
            using (var settingsForm = new SettingsForm())
            {
                settingsForm.ShowInTaskbar = false;
                settingsForm.ShowDialog(this);
            }
        }

        public void ShowErrorIfSettingsAreInvalid()
        {
            if (!database.IsSettingsValid())
            {
                Utils.Log("Неправильные настройки программы. Сканирование начнётся только после установки правильных настроек", LogLevel.ERROR);
            }
        }

        public void SetStatus(string status, StatusType statusType)
        {
            var color = Color.Black;
            switch (statusType)
            {
                case StatusType.SUCCESS:
                    color = Color.DarkGreen;
                    break;

                case StatusType.ERROR:
                    color = Color.Red;
                    break;
            }

            StatusLabel.Text = "Статус: " + status;
            StatusLabel.ForeColor = color;
        }
    }
}
