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
        public enum LogLevel
        {
            NOTIFY,
            GENERAL,
            SUCCESS,
            ERROR,
        }

        public MainForm()
        {
            InitializeComponent();
        }

        void ExitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        void MainForm_Load(object sender, EventArgs e)
        {
            Text = Constants.APP_NAME + " - " + Constants.APP_VERSION;
            VersionLabel.Text = Constants.APP_VERSION;
            Log(Text + " успешно загружен", LogLevel.SUCCESS);

            // Готовим базу данных
            var database = new Database();

            // Запускаем веб сервер
            Log("Пытаемся запустить веб сервер на порт " + Constants.WEB_PORT + ". Он нужен для просмотра результатов сканирования в браузере в виде привычной веб страницы", LogLevel.NOTIFY);
            try
            {
                var listener = new WebListener(Constants.WEB_PORT);
                Log("Веб сервер подключен по адресу " + Constants.RESULTS_WEB_PAGE, LogLevel.SUCCESS);
            }
            catch (System.Exception ex)
            {
                Log("Веб сервер не подключен на порт " + Constants.WEB_PORT + ". Причина: " + ex.Message, LogLevel.ERROR);
            }
        }

        public void Log(string text, LogLevel logLevel = LogLevel.GENERAL)
        {
            Color? color = null;
            var prefix = string.Empty;

            switch (logLevel)
            {
                case LogLevel.ERROR:
                    color = Color.Red;
                    prefix = "Ошибка: ";
                    break;

                case LogLevel.NOTIFY:
                    color = Color.Gray;
                    break;

                case LogLevel.SUCCESS:
                    color = Color.DarkGreen;
                    break;
            }

            Log(prefix + text, color);
        }

        public void Log(string text, Color? color = null)
        {
            LogTextBox.SuspendLayout();

            var previousSelectionColor = LogTextBox.SelectionColor;
            if (color != null)
            {
                LogTextBox.SelectionColor = color.Value;
            }

            if (!string.IsNullOrWhiteSpace(LogTextBox.Text))
            {
                LogTextBox.AppendText($"{Environment.NewLine}{text}");
            }
            else
            {
                LogTextBox.AppendText(text);
            }

            LogTextBox.ScrollToCaret();
            LogTextBox.SelectionColor = previousSelectionColor;
            LogTextBox.ResumeLayout();
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
    }
}
