namespace VK_Unicorn
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.ExitButton = new System.Windows.Forms.Button();
            this.LogTextBox = new System.Windows.Forms.RichTextBox();
            this.StartWorkingButton = new System.Windows.Forms.Button();
            this.LogGroupBox = new System.Windows.Forms.GroupBox();
            this.VersionLabel = new System.Windows.Forms.Label();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.TrayNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.LogGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // ExitButton
            // 
            this.ExitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ExitButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ExitButton.Location = new System.Drawing.Point(743, 447);
            this.ExitButton.Name = "ExitButton";
            this.ExitButton.Size = new System.Drawing.Size(118, 38);
            this.ExitButton.TabIndex = 0;
            this.ExitButton.TabStop = false;
            this.ExitButton.Text = "Выйти";
            this.ExitButton.UseVisualStyleBackColor = true;
            this.ExitButton.Click += new System.EventHandler(this.ExitButton_Click);
            // 
            // LogTextBox
            // 
            this.LogTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.LogTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogTextBox.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.LogTextBox.Location = new System.Drawing.Point(3, 16);
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.ReadOnly = true;
            this.LogTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.LogTextBox.Size = new System.Drawing.Size(856, 425);
            this.LogTextBox.TabIndex = 1;
            this.LogTextBox.TabStop = false;
            this.LogTextBox.Text = "";
            this.LogTextBox.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.LogTextBox_LinkClicked);
            // 
            // StartWorkingButton
            // 
            this.StartWorkingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.StartWorkingButton.Enabled = false;
            this.StartWorkingButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.StartWorkingButton.Location = new System.Drawing.Point(620, 447);
            this.StartWorkingButton.Name = "StartWorkingButton";
            this.StartWorkingButton.Size = new System.Drawing.Size(118, 38);
            this.StartWorkingButton.TabIndex = 2;
            this.StartWorkingButton.TabStop = false;
            this.StartWorkingButton.Text = "Начать работу";
            this.StartWorkingButton.UseVisualStyleBackColor = true;
            this.StartWorkingButton.Click += new System.EventHandler(this.StartWorkingButton_Click);
            // 
            // LogGroupBox
            // 
            this.LogGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LogGroupBox.Controls.Add(this.LogTextBox);
            this.LogGroupBox.Location = new System.Drawing.Point(0, 0);
            this.LogGroupBox.Name = "LogGroupBox";
            this.LogGroupBox.Size = new System.Drawing.Size(862, 444);
            this.LogGroupBox.TabIndex = 3;
            this.LogGroupBox.TabStop = false;
            this.LogGroupBox.Text = "Лог";
            // 
            // VersionLabel
            // 
            this.VersionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.VersionLabel.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.VersionLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.VersionLabel.Location = new System.Drawing.Point(1, 467);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(56, 16);
            this.VersionLabel.TabIndex = 4;
            this.VersionLabel.Text = "Версия";
            this.VersionLabel.Click += new System.EventHandler(this.VersionLabel_Click);
            // 
            // StatusLabel
            // 
            this.StatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.StatusLabel.AutoSize = true;
            this.StatusLabel.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.StatusLabel.ForeColor = System.Drawing.Color.Green;
            this.StatusLabel.Location = new System.Drawing.Point(1, 448);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(56, 16);
            this.StatusLabel.TabIndex = 5;
            this.StatusLabel.Text = "Статус";
            // 
            // TrayNotifyIcon
            // 
            this.TrayNotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("TrayNotifyIcon.Icon")));
            this.TrayNotifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.TrayNotifyIcon_MouseClick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(862, 487);
            this.Controls.Add(this.StatusLabel);
            this.Controls.Add(this.VersionLabel);
            this.Controls.Add(this.LogGroupBox);
            this.Controls.Add(this.StartWorkingButton);
            this.Controls.Add(this.ExitButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VK Unicorn";
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.LogGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ExitButton;
        private System.Windows.Forms.RichTextBox LogTextBox;
        private System.Windows.Forms.Button StartWorkingButton;
        private System.Windows.Forms.GroupBox LogGroupBox;
        private System.Windows.Forms.Label VersionLabel;
        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.NotifyIcon TrayNotifyIcon;
    }
}

