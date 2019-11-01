using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VK_Unicorn
{
    public partial class MainForm : Form
    {
        public static MainForm Instance { get; private set; }

        public MainForm()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            InitializeComponent();
        }

        void MainForm_Load(object sender, EventArgs e)
        {
            Text = Constants.APP_NAME + " - " + Constants.APP_VERSION;
            VersionLabel.Text = Constants.APP_VERSION;
            Utils.Log(Text + " успешно загружен", LogLevel.SUCCESS);

            // Готовим базу данных
            Utils.Log("Подготавливаем базу данных к работе", LogLevel.NOTIFY);
            var database = new Database();

            // Статистика
            StatisticsLabel.Text = "Профилей: " + database.GetProfilesCount() + " Групп: " + database.GetGroupsCount();

            // Запускаем веб сервер
            Utils.Log("Пытаемся запустить веб сервер на порт " + Constants.WEB_PORT + ". Он нужен для просмотра результатов сканирования в браузере в виде привычной веб страницы", LogLevel.NOTIFY);
            try
            {
                var listener = new WebListener(Constants.WEB_PORT);
                Utils.Log("Веб сервер подключен по адресу " + Constants.RESULTS_WEB_PAGE, LogLevel.SUCCESS);
            }
            catch (System.Exception ex)
            {
                Utils.Log("Веб сервер не подключен на порт " + Constants.WEB_PORT + ". Причина: " + ex.Message, LogLevel.ERROR);
            }
        }

        void ExitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ShowResults_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Constants.RESULTS_WEB_PAGE);
        }

        private void VersionLabel_Click(object sender, EventArgs e)
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
    }
}
