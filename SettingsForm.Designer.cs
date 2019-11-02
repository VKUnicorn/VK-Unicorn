namespace VK_Unicorn
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.CancelAndCloseButton = new System.Windows.Forms.Button();
            this.ApplyButton = new System.Windows.Forms.Button();
            this.ApplicationIdGroupBox = new System.Windows.Forms.GroupBox();
            this.WelcomeLabel = new System.Windows.Forms.Label();
            this.ApplicationIdHelperLabel = new System.Windows.Forms.Label();
            this.CreateAppLinkLabel = new System.Windows.Forms.LinkLabel();
            this.AppSettingsLabel = new System.Windows.Forms.Label();
            this.CopyApplicationIdLabel = new System.Windows.Forms.Label();
            this.ApplicationIdTextBox = new System.Windows.Forms.TextBox();
            this.ApplicationIdLabel = new System.Windows.Forms.Label();
            this.AccountCredentialsGroupBox = new System.Windows.Forms.GroupBox();
            this.AccountCredentialsHelpLabel = new System.Windows.Forms.Label();
            this.LoginLabel = new System.Windows.Forms.Label();
            this.LoginTextBox = new System.Windows.Forms.TextBox();
            this.PasswordLabel = new System.Windows.Forms.Label();
            this.PasswordTextBox = new System.Windows.Forms.TextBox();
            this.CopyApplicationIdPictureBox = new System.Windows.Forms.PictureBox();
            this.AddApplicationPictureBox = new System.Windows.Forms.PictureBox();
            this.SettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.CityIdNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.CityIdLabel = new System.Windows.Forms.Label();
            this.CityIdLinkLabel = new System.Windows.Forms.LinkLabel();
            this.StopWordsLabel = new System.Windows.Forms.Label();
            this.StopWordsTextBox = new System.Windows.Forms.TextBox();
            this.CityIdHelperLabel1 = new System.Windows.Forms.Label();
            this.CityIdHelperLabel2 = new System.Windows.Forms.Label();
            this.CityIdHelperLabel3 = new System.Windows.Forms.Label();
            this.CityIdHelperLabel4 = new System.Windows.Forms.Label();
            this.ApplicationIdGroupBox.SuspendLayout();
            this.AccountCredentialsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CopyApplicationIdPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AddApplicationPictureBox)).BeginInit();
            this.SettingsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CityIdNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // CancelAndCloseButton
            // 
            this.CancelAndCloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelAndCloseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.CancelAndCloseButton.Location = new System.Drawing.Point(712, 747);
            this.CancelAndCloseButton.Name = "CancelAndCloseButton";
            this.CancelAndCloseButton.Size = new System.Drawing.Size(118, 38);
            this.CancelAndCloseButton.TabIndex = 1;
            this.CancelAndCloseButton.TabStop = false;
            this.CancelAndCloseButton.Text = "Отмена";
            this.CancelAndCloseButton.UseVisualStyleBackColor = true;
            this.CancelAndCloseButton.Click += new System.EventHandler(this.CancelAndCloseButton_Click);
            // 
            // ApplyButton
            // 
            this.ApplyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ApplyButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ApplyButton.Location = new System.Drawing.Point(588, 747);
            this.ApplyButton.Name = "ApplyButton";
            this.ApplyButton.Size = new System.Drawing.Size(118, 38);
            this.ApplyButton.TabIndex = 2;
            this.ApplyButton.TabStop = false;
            this.ApplyButton.Text = "Применить";
            this.ApplyButton.UseVisualStyleBackColor = true;
            this.ApplyButton.Click += new System.EventHandler(this.ApplyButton_Click);
            // 
            // ApplicationIdGroupBox
            // 
            this.ApplicationIdGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ApplicationIdGroupBox.Controls.Add(this.ApplicationIdLabel);
            this.ApplicationIdGroupBox.Controls.Add(this.ApplicationIdTextBox);
            this.ApplicationIdGroupBox.Controls.Add(this.CopyApplicationIdPictureBox);
            this.ApplicationIdGroupBox.Controls.Add(this.CopyApplicationIdLabel);
            this.ApplicationIdGroupBox.Controls.Add(this.AppSettingsLabel);
            this.ApplicationIdGroupBox.Controls.Add(this.AddApplicationPictureBox);
            this.ApplicationIdGroupBox.Controls.Add(this.CreateAppLinkLabel);
            this.ApplicationIdGroupBox.Controls.Add(this.ApplicationIdHelperLabel);
            this.ApplicationIdGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ApplicationIdGroupBox.Location = new System.Drawing.Point(1, 42);
            this.ApplicationIdGroupBox.Name = "ApplicationIdGroupBox";
            this.ApplicationIdGroupBox.Size = new System.Drawing.Size(829, 470);
            this.ApplicationIdGroupBox.TabIndex = 3;
            this.ApplicationIdGroupBox.TabStop = false;
            this.ApplicationIdGroupBox.Text = "ID приложения";
            // 
            // WelcomeLabel
            // 
            this.WelcomeLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.WelcomeLabel.AutoSize = true;
            this.WelcomeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.WelcomeLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(102)))), ((int)(((byte)(0)))));
            this.WelcomeLabel.Location = new System.Drawing.Point(240, 0);
            this.WelcomeLabel.Name = "WelcomeLabel";
            this.WelcomeLabel.Size = new System.Drawing.Size(350, 39);
            this.WelcomeLabel.TabIndex = 4;
            this.WelcomeLabel.Text = "Добро пожаловать!";
            // 
            // ApplicationIdHelperLabel
            // 
            this.ApplicationIdHelperLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ApplicationIdHelperLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ApplicationIdHelperLabel.Location = new System.Drawing.Point(6, 16);
            this.ApplicationIdHelperLabel.Name = "ApplicationIdHelperLabel";
            this.ApplicationIdHelperLabel.Size = new System.Drawing.Size(817, 133);
            this.ApplicationIdHelperLabel.TabIndex = 0;
            this.ApplicationIdHelperLabel.Text = resources.GetString("ApplicationIdHelperLabel.Text");
            // 
            // CreateAppLinkLabel
            // 
            this.CreateAppLinkLabel.AutoSize = true;
            this.CreateAppLinkLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.CreateAppLinkLabel.Location = new System.Drawing.Point(6, 145);
            this.CreateAppLinkLabel.Name = "CreateAppLinkLabel";
            this.CreateAppLinkLabel.Size = new System.Drawing.Size(580, 16);
            this.CreateAppLinkLabel.TabIndex = 1;
            this.CreateAppLinkLabel.Text = "1. Открываем эту ссылку или вручную заходим в \"Управление\" - \"Создать приложение\"" +
    "";
            this.CreateAppLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.CreateAppLinkLabel_LinkClicked);
            // 
            // AppSettingsLabel
            // 
            this.AppSettingsLabel.AutoSize = true;
            this.AppSettingsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.AppSettingsLabel.Location = new System.Drawing.Point(6, 163);
            this.AppSettingsLabel.Name = "AppSettingsLabel";
            this.AppSettingsLabel.Size = new System.Drawing.Size(694, 16);
            this.AppSettingsLabel.TabIndex = 3;
            this.AppSettingsLabel.Text = "2. Устанавливаем настройки как тут и жмём \"Подключить приложение\". Название можно" +
    " выбрать любое";
            // 
            // CopyApplicationIdLabel
            // 
            this.CopyApplicationIdLabel.AutoSize = true;
            this.CopyApplicationIdLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.CopyApplicationIdLabel.Location = new System.Drawing.Point(6, 304);
            this.CopyApplicationIdLabel.Name = "CopyApplicationIdLabel";
            this.CopyApplicationIdLabel.Size = new System.Drawing.Size(236, 16);
            this.CopyApplicationIdLabel.TabIndex = 4;
            this.CopyApplicationIdLabel.Text = "3. Копируем ID приложения отсюда";
            // 
            // ApplicationIdTextBox
            // 
            this.ApplicationIdTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ApplicationIdTextBox.Location = new System.Drawing.Point(9, 436);
            this.ApplicationIdTextBox.MaxLength = 9;
            this.ApplicationIdTextBox.Name = "ApplicationIdTextBox";
            this.ApplicationIdTextBox.Size = new System.Drawing.Size(225, 26);
            this.ApplicationIdTextBox.TabIndex = 6;
            this.ApplicationIdTextBox.WordWrap = false;
            // 
            // ApplicationIdLabel
            // 
            this.ApplicationIdLabel.AutoSize = true;
            this.ApplicationIdLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ApplicationIdLabel.Location = new System.Drawing.Point(6, 417);
            this.ApplicationIdLabel.Name = "ApplicationIdLabel";
            this.ApplicationIdLabel.Size = new System.Drawing.Size(191, 16);
            this.ApplicationIdLabel.TabIndex = 7;
            this.ApplicationIdLabel.Text = "4. Вставляем ID приложения";
            // 
            // AccountCredentialsGroupBox
            // 
            this.AccountCredentialsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AccountCredentialsGroupBox.Controls.Add(this.PasswordLabel);
            this.AccountCredentialsGroupBox.Controls.Add(this.PasswordTextBox);
            this.AccountCredentialsGroupBox.Controls.Add(this.LoginLabel);
            this.AccountCredentialsGroupBox.Controls.Add(this.AccountCredentialsHelpLabel);
            this.AccountCredentialsGroupBox.Controls.Add(this.LoginTextBox);
            this.AccountCredentialsGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.AccountCredentialsGroupBox.Location = new System.Drawing.Point(1, 514);
            this.AccountCredentialsGroupBox.Name = "AccountCredentialsGroupBox";
            this.AccountCredentialsGroupBox.Size = new System.Drawing.Size(829, 105);
            this.AccountCredentialsGroupBox.TabIndex = 5;
            this.AccountCredentialsGroupBox.TabStop = false;
            this.AccountCredentialsGroupBox.Text = "Данные аккаунта";
            // 
            // AccountCredentialsHelpLabel
            // 
            this.AccountCredentialsHelpLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AccountCredentialsHelpLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.AccountCredentialsHelpLabel.Location = new System.Drawing.Point(6, 16);
            this.AccountCredentialsHelpLabel.Name = "AccountCredentialsHelpLabel";
            this.AccountCredentialsHelpLabel.Size = new System.Drawing.Size(817, 36);
            this.AccountCredentialsHelpLabel.TabIndex = 0;
            this.AccountCredentialsHelpLabel.Text = resources.GetString("AccountCredentialsHelpLabel.Text");
            // 
            // LoginLabel
            // 
            this.LoginLabel.AutoSize = true;
            this.LoginLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.LoginLabel.Location = new System.Drawing.Point(8, 52);
            this.LoginLabel.Name = "LoginLabel";
            this.LoginLabel.Size = new System.Drawing.Size(47, 16);
            this.LoginLabel.TabIndex = 9;
            this.LoginLabel.Text = "Логин";
            // 
            // LoginTextBox
            // 
            this.LoginTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.LoginTextBox.Location = new System.Drawing.Point(9, 71);
            this.LoginTextBox.MaxLength = 40;
            this.LoginTextBox.Name = "LoginTextBox";
            this.LoginTextBox.Size = new System.Drawing.Size(225, 26);
            this.LoginTextBox.TabIndex = 8;
            this.LoginTextBox.WordWrap = false;
            // 
            // PasswordLabel
            // 
            this.PasswordLabel.AutoSize = true;
            this.PasswordLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.PasswordLabel.Location = new System.Drawing.Point(244, 52);
            this.PasswordLabel.Name = "PasswordLabel";
            this.PasswordLabel.Size = new System.Drawing.Size(57, 16);
            this.PasswordLabel.TabIndex = 11;
            this.PasswordLabel.Text = "Пароль";
            // 
            // PasswordTextBox
            // 
            this.PasswordTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.PasswordTextBox.Location = new System.Drawing.Point(246, 71);
            this.PasswordTextBox.MaxLength = 40;
            this.PasswordTextBox.Name = "PasswordTextBox";
            this.PasswordTextBox.Size = new System.Drawing.Size(225, 26);
            this.PasswordTextBox.TabIndex = 10;
            this.PasswordTextBox.UseSystemPasswordChar = true;
            this.PasswordTextBox.WordWrap = false;
            // 
            // CopyApplicationIdPictureBox
            // 
            this.CopyApplicationIdPictureBox.Image = global::VK_Unicorn.Properties.Resources.app_id;
            this.CopyApplicationIdPictureBox.Location = new System.Drawing.Point(9, 323);
            this.CopyApplicationIdPictureBox.Name = "CopyApplicationIdPictureBox";
            this.CopyApplicationIdPictureBox.Size = new System.Drawing.Size(421, 88);
            this.CopyApplicationIdPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.CopyApplicationIdPictureBox.TabIndex = 5;
            this.CopyApplicationIdPictureBox.TabStop = false;
            // 
            // AddApplicationPictureBox
            // 
            this.AddApplicationPictureBox.Image = global::VK_Unicorn.Properties.Resources.app_settings;
            this.AddApplicationPictureBox.Location = new System.Drawing.Point(9, 183);
            this.AddApplicationPictureBox.Name = "AddApplicationPictureBox";
            this.AddApplicationPictureBox.Size = new System.Drawing.Size(297, 117);
            this.AddApplicationPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.AddApplicationPictureBox.TabIndex = 2;
            this.AddApplicationPictureBox.TabStop = false;
            // 
            // SettingsGroupBox
            // 
            this.SettingsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SettingsGroupBox.Controls.Add(this.CityIdHelperLabel4);
            this.SettingsGroupBox.Controls.Add(this.CityIdHelperLabel3);
            this.SettingsGroupBox.Controls.Add(this.CityIdHelperLabel2);
            this.SettingsGroupBox.Controls.Add(this.CityIdHelperLabel1);
            this.SettingsGroupBox.Controls.Add(this.StopWordsLabel);
            this.SettingsGroupBox.Controls.Add(this.StopWordsTextBox);
            this.SettingsGroupBox.Controls.Add(this.CityIdLinkLabel);
            this.SettingsGroupBox.Controls.Add(this.CityIdLabel);
            this.SettingsGroupBox.Controls.Add(this.CityIdNumericUpDown);
            this.SettingsGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.SettingsGroupBox.Location = new System.Drawing.Point(1, 621);
            this.SettingsGroupBox.Name = "SettingsGroupBox";
            this.SettingsGroupBox.Size = new System.Drawing.Size(829, 123);
            this.SettingsGroupBox.TabIndex = 6;
            this.SettingsGroupBox.TabStop = false;
            this.SettingsGroupBox.Text = "Настройки";
            // 
            // CityIdNumericUpDown
            // 
            this.CityIdNumericUpDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.CityIdNumericUpDown.Location = new System.Drawing.Point(9, 37);
            this.CityIdNumericUpDown.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.CityIdNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.CityIdNumericUpDown.Name = "CityIdNumericUpDown";
            this.CityIdNumericUpDown.Size = new System.Drawing.Size(225, 26);
            this.CityIdNumericUpDown.TabIndex = 1;
            this.CityIdNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // CityIdLabel
            // 
            this.CityIdLabel.AutoSize = true;
            this.CityIdLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.CityIdLabel.Location = new System.Drawing.Point(8, 18);
            this.CityIdLabel.Name = "CityIdLabel";
            this.CityIdLabel.Size = new System.Drawing.Size(70, 16);
            this.CityIdLabel.TabIndex = 10;
            this.CityIdLabel.Text = "ID города";
            // 
            // CityIdLinkLabel
            // 
            this.CityIdLinkLabel.AutoSize = true;
            this.CityIdLinkLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.CityIdLinkLabel.Location = new System.Drawing.Point(147, 18);
            this.CityIdLinkLabel.Name = "CityIdLinkLabel";
            this.CityIdLinkLabel.Size = new System.Drawing.Size(87, 16);
            this.CityIdLinkLabel.TabIndex = 11;
            this.CityIdLinkLabel.Text = "Как узнать?";
            this.CityIdLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.CityIdLinkLabel_LinkClicked);
            // 
            // StopWordsLabel
            // 
            this.StopWordsLabel.AutoSize = true;
            this.StopWordsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.StopWordsLabel.Location = new System.Drawing.Point(7, 70);
            this.StopWordsLabel.Name = "StopWordsLabel";
            this.StopWordsLabel.Size = new System.Drawing.Size(387, 16);
            this.StopWordsLabel.TabIndex = 13;
            this.StopWordsLabel.Text = "Стоп слова. Разделитель по умолчанию - точка с запятой";
            // 
            // StopWordsTextBox
            // 
            this.StopWordsTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.StopWordsTextBox.Location = new System.Drawing.Point(9, 89);
            this.StopWordsTextBox.Name = "StopWordsTextBox";
            this.StopWordsTextBox.Size = new System.Drawing.Size(462, 26);
            this.StopWordsTextBox.TabIndex = 12;
            this.StopWordsTextBox.WordWrap = false;
            // 
            // CityIdHelperLabel1
            // 
            this.CityIdHelperLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CityIdHelperLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.CityIdHelperLabel1.ForeColor = System.Drawing.SystemColors.GrayText;
            this.CityIdHelperLabel1.Location = new System.Drawing.Point(244, 12);
            this.CityIdHelperLabel1.Name = "CityIdHelperLabel1";
            this.CityIdHelperLabel1.Size = new System.Drawing.Size(112, 51);
            this.CityIdHelperLabel1.TabIndex = 14;
            this.CityIdHelperLabel1.Text = "Москва - 1\r\nСанкт-Петербург - 2\r\nКиев - 314\r\nМинск - 282";
            // 
            // CityIdHelperLabel2
            // 
            this.CityIdHelperLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CityIdHelperLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.CityIdHelperLabel2.ForeColor = System.Drawing.SystemColors.GrayText;
            this.CityIdHelperLabel2.Location = new System.Drawing.Point(352, 12);
            this.CityIdHelperLabel2.Name = "CityIdHelperLabel2";
            this.CityIdHelperLabel2.Size = new System.Drawing.Size(122, 51);
            this.CityIdHelperLabel2.TabIndex = 15;
            this.CityIdHelperLabel2.Text = "Новосибирск - 99\r\nЕкатеринбург - 49\r\nНижний Новгород - 95\r\nКазань - 60";
            // 
            // CityIdHelperLabel3
            // 
            this.CityIdHelperLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CityIdHelperLabel3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.CityIdHelperLabel3.ForeColor = System.Drawing.SystemColors.GrayText;
            this.CityIdHelperLabel3.Location = new System.Drawing.Point(473, 12);
            this.CityIdHelperLabel3.Name = "CityIdHelperLabel3";
            this.CityIdHelperLabel3.Size = new System.Drawing.Size(122, 51);
            this.CityIdHelperLabel3.TabIndex = 16;
            this.CityIdHelperLabel3.Text = "Челябинск - 158\r\nОмск - 104\r\nСамара - 123\r\nРостов-на-Дону - 119";
            // 
            // CityIdHelperLabel4
            // 
            this.CityIdHelperLabel4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CityIdHelperLabel4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.CityIdHelperLabel4.ForeColor = System.Drawing.SystemColors.GrayText;
            this.CityIdHelperLabel4.Location = new System.Drawing.Point(587, 12);
            this.CityIdHelperLabel4.Name = "CityIdHelperLabel4";
            this.CityIdHelperLabel4.Size = new System.Drawing.Size(95, 51);
            this.CityIdHelperLabel4.TabIndex = 17;
            this.CityIdHelperLabel4.Text = "Уфа - 151\r\nКрасноярск - 73\r\nПермь - 110\r\nВолгоград - 10";
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(831, 787);
            this.Controls.Add(this.SettingsGroupBox);
            this.Controls.Add(this.AccountCredentialsGroupBox);
            this.Controls.Add(this.WelcomeLabel);
            this.Controls.Add(this.ApplicationIdGroupBox);
            this.Controls.Add(this.ApplyButton);
            this.Controls.Add(this.CancelAndCloseButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Настройки";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SettingsForm_FormClosed);
            this.Shown += new System.EventHandler(this.SettingsForm_Shown);
            this.ApplicationIdGroupBox.ResumeLayout(false);
            this.ApplicationIdGroupBox.PerformLayout();
            this.AccountCredentialsGroupBox.ResumeLayout(false);
            this.AccountCredentialsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CopyApplicationIdPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AddApplicationPictureBox)).EndInit();
            this.SettingsGroupBox.ResumeLayout(false);
            this.SettingsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CityIdNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button CancelAndCloseButton;
        private System.Windows.Forms.Button ApplyButton;
        private System.Windows.Forms.GroupBox ApplicationIdGroupBox;
        private System.Windows.Forms.Label WelcomeLabel;
        private System.Windows.Forms.Label ApplicationIdHelperLabel;
        private System.Windows.Forms.LinkLabel CreateAppLinkLabel;
        private System.Windows.Forms.PictureBox AddApplicationPictureBox;
        private System.Windows.Forms.Label AppSettingsLabel;
        private System.Windows.Forms.PictureBox CopyApplicationIdPictureBox;
        private System.Windows.Forms.Label CopyApplicationIdLabel;
        private System.Windows.Forms.TextBox ApplicationIdTextBox;
        private System.Windows.Forms.Label ApplicationIdLabel;
        private System.Windows.Forms.GroupBox AccountCredentialsGroupBox;
        private System.Windows.Forms.Label AccountCredentialsHelpLabel;
        private System.Windows.Forms.Label PasswordLabel;
        private System.Windows.Forms.TextBox PasswordTextBox;
        private System.Windows.Forms.Label LoginLabel;
        private System.Windows.Forms.TextBox LoginTextBox;
        private System.Windows.Forms.GroupBox SettingsGroupBox;
        private System.Windows.Forms.NumericUpDown CityIdNumericUpDown;
        private System.Windows.Forms.LinkLabel CityIdLinkLabel;
        private System.Windows.Forms.Label CityIdLabel;
        private System.Windows.Forms.Label StopWordsLabel;
        private System.Windows.Forms.TextBox StopWordsTextBox;
        private System.Windows.Forms.Label CityIdHelperLabel1;
        private System.Windows.Forms.Label CityIdHelperLabel2;
        private System.Windows.Forms.Label CityIdHelperLabel3;
        private System.Windows.Forms.Label CityIdHelperLabel4;
    }
}