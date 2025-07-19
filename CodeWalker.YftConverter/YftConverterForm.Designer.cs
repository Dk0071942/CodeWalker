namespace CodeWalker.YftConverter
{
    partial class YftConverterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(YftConverterForm));
            this.DescriptionLabel = new System.Windows.Forms.Label();
            this.InputFolderLabel = new System.Windows.Forms.Label();
            this.InputFolderTextBox = new System.Windows.Forms.TextBox();
            this.InputFolderBrowseButton = new System.Windows.Forms.Button();
            this.OutputFolderLabel = new System.Windows.Forms.Label();
            this.OutputFolderTextBox = new System.Windows.Forms.TextBox();
            this.OutputFolderBrowseButton = new System.Windows.Forms.Button();
            this.OutputFormatGroupBox = new System.Windows.Forms.GroupBox();
            this.Gen8XmlRadioButton = new System.Windows.Forms.RadioButton();
            this.Gen8YftRadioButton = new System.Windows.Forms.RadioButton();
            this.Gen9YftRadioButton = new System.Windows.Forms.RadioButton();
            this.OptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.IncludeSubfoldersCheckBox = new System.Windows.Forms.CheckBox();
            this.OverwriteFilesCheckBox = new System.Windows.Forms.CheckBox();
            this.ProcessButton = new System.Windows.Forms.Button();
            this.StatusProgressBar = new System.Windows.Forms.ProgressBar();
            this.LogTextBox = new System.Windows.Forms.TextBox();
            this.OutputFormatGroupBox.SuspendLayout();
            this.OptionsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // DescriptionLabel
            // 
            this.DescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DescriptionLabel.Location = new System.Drawing.Point(18, 14);
            this.DescriptionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.DescriptionLabel.Name = "DescriptionLabel";
            this.DescriptionLabel.Size = new System.Drawing.Size(915, 31);
            this.DescriptionLabel.TabIndex = 0;
            this.DescriptionLabel.Text = "在不同格式之间转换YFT文件（Gen8 YFT、Gen9 YFT和XML）。";
            // 
            // InputFolderLabel
            // 
            this.InputFolderLabel.AutoSize = true;
            this.InputFolderLabel.Location = new System.Drawing.Point(18, 60);
            this.InputFolderLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.InputFolderLabel.Name = "InputFolderLabel";
            this.InputFolderLabel.Size = new System.Drawing.Size(93, 20);
            this.InputFolderLabel.TabIndex = 1;
            this.InputFolderLabel.Text = "输入文件夹:";
            // 
            // InputFolderTextBox
            // 
            this.InputFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InputFolderTextBox.Location = new System.Drawing.Point(126, 55);
            this.InputFolderTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.InputFolderTextBox.Name = "InputFolderTextBox";
            this.InputFolderTextBox.Size = new System.Drawing.Size(684, 26);
            this.InputFolderTextBox.TabIndex = 2;
            // 
            // InputFolderBrowseButton
            // 
            this.InputFolderBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.InputFolderBrowseButton.Location = new System.Drawing.Point(820, 52);
            this.InputFolderBrowseButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.InputFolderBrowseButton.Name = "InputFolderBrowseButton";
            this.InputFolderBrowseButton.Size = new System.Drawing.Size(112, 35);
            this.InputFolderBrowseButton.TabIndex = 3;
            this.InputFolderBrowseButton.Text = "浏览...";
            this.InputFolderBrowseButton.UseVisualStyleBackColor = true;
            this.InputFolderBrowseButton.Click += new System.EventHandler(this.InputFolderBrowseButton_Click);
            // 
            // OutputFolderLabel
            // 
            this.OutputFolderLabel.AutoSize = true;
            this.OutputFolderLabel.Location = new System.Drawing.Point(18, 100);
            this.OutputFolderLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.OutputFolderLabel.Name = "OutputFolderLabel";
            this.OutputFolderLabel.Size = new System.Drawing.Size(93, 20);
            this.OutputFolderLabel.TabIndex = 4;
            this.OutputFolderLabel.Text = "输出文件夹:";
            // 
            // OutputFolderTextBox
            // 
            this.OutputFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputFolderTextBox.Location = new System.Drawing.Point(126, 95);
            this.OutputFolderTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.OutputFolderTextBox.Name = "OutputFolderTextBox";
            this.OutputFolderTextBox.Size = new System.Drawing.Size(684, 26);
            this.OutputFolderTextBox.TabIndex = 5;
            // 
            // OutputFolderBrowseButton
            // 
            this.OutputFolderBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputFolderBrowseButton.Location = new System.Drawing.Point(820, 92);
            this.OutputFolderBrowseButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.OutputFolderBrowseButton.Name = "OutputFolderBrowseButton";
            this.OutputFolderBrowseButton.Size = new System.Drawing.Size(112, 35);
            this.OutputFolderBrowseButton.TabIndex = 6;
            this.OutputFolderBrowseButton.Text = "浏览...";
            this.OutputFolderBrowseButton.UseVisualStyleBackColor = true;
            this.OutputFolderBrowseButton.Click += new System.EventHandler(this.OutputFolderBrowseButton_Click);
            // 
            // OutputFormatGroupBox
            // 
            this.OutputFormatGroupBox.Controls.Add(this.Gen8XmlRadioButton);
            this.OutputFormatGroupBox.Controls.Add(this.Gen8YftRadioButton);
            this.OutputFormatGroupBox.Controls.Add(this.Gen9YftRadioButton);
            this.OutputFormatGroupBox.Location = new System.Drawing.Point(22, 137);
            this.OutputFormatGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.OutputFormatGroupBox.Name = "OutputFormatGroupBox";
            this.OutputFormatGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.OutputFormatGroupBox.Size = new System.Drawing.Size(450, 131);
            this.OutputFormatGroupBox.TabIndex = 7;
            this.OutputFormatGroupBox.TabStop = false;
            this.OutputFormatGroupBox.Text = "输出格式";
            // 
            // Gen8XmlRadioButton
            // 
            this.Gen8XmlRadioButton.AutoSize = true;
            this.Gen8XmlRadioButton.Location = new System.Drawing.Point(15, 94);
            this.Gen8XmlRadioButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Gen8XmlRadioButton.Name = "Gen8XmlRadioButton";
            this.Gen8XmlRadioButton.Size = new System.Drawing.Size(67, 24);
            this.Gen8XmlRadioButton.TabIndex = 2;
            this.Gen8XmlRadioButton.Text = "XML";
            this.Gen8XmlRadioButton.UseVisualStyleBackColor = true;
            // 
            // Gen8YftRadioButton
            // 
            this.Gen8YftRadioButton.AutoSize = true;
            this.Gen8YftRadioButton.Location = new System.Drawing.Point(15, 29);
            this.Gen8YftRadioButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Gen8YftRadioButton.Name = "Gen8YftRadioButton";
            this.Gen8YftRadioButton.Size = new System.Drawing.Size(154, 24);
            this.Gen8YftRadioButton.TabIndex = 0;
            this.Gen8YftRadioButton.Text = "Gen8 YFT (压缩)";
            this.Gen8YftRadioButton.UseVisualStyleBackColor = true;
            // 
            // Gen9YftRadioButton
            // 
            this.Gen9YftRadioButton.AutoSize = true;
            this.Gen9YftRadioButton.Checked = true;
            this.Gen9YftRadioButton.Location = new System.Drawing.Point(15, 62);
            this.Gen9YftRadioButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Gen9YftRadioButton.Name = "Gen9YftRadioButton";
            this.Gen9YftRadioButton.Size = new System.Drawing.Size(154, 24);
            this.Gen9YftRadioButton.TabIndex = 1;
            this.Gen9YftRadioButton.TabStop = true;
            this.Gen9YftRadioButton.Text = "Gen9 YFT (压缩)";
            this.Gen9YftRadioButton.UseVisualStyleBackColor = true;
            // 
            // OptionsGroupBox
            // 
            this.OptionsGroupBox.Controls.Add(this.IncludeSubfoldersCheckBox);
            this.OptionsGroupBox.Controls.Add(this.OverwriteFilesCheckBox);
            this.OptionsGroupBox.Location = new System.Drawing.Point(482, 137);
            this.OptionsGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.OptionsGroupBox.Name = "OptionsGroupBox";
            this.OptionsGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.OptionsGroupBox.Size = new System.Drawing.Size(452, 131);
            this.OptionsGroupBox.TabIndex = 8;
            this.OptionsGroupBox.TabStop = false;
            this.OptionsGroupBox.Text = "选项";
            // 
            // IncludeSubfoldersCheckBox
            // 
            this.IncludeSubfoldersCheckBox.AutoSize = true;
            this.IncludeSubfoldersCheckBox.Checked = true;
            this.IncludeSubfoldersCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.IncludeSubfoldersCheckBox.Location = new System.Drawing.Point(15, 29);
            this.IncludeSubfoldersCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.IncludeSubfoldersCheckBox.Name = "IncludeSubfoldersCheckBox";
            this.IncludeSubfoldersCheckBox.Size = new System.Drawing.Size(227, 24);
            this.IncludeSubfoldersCheckBox.TabIndex = 0;
            this.IncludeSubfoldersCheckBox.Text = "输入文件搜索包含子文件夹";
            this.IncludeSubfoldersCheckBox.UseVisualStyleBackColor = true;
            // 
            // OverwriteFilesCheckBox
            // 
            this.OverwriteFilesCheckBox.AutoSize = true;
            this.OverwriteFilesCheckBox.Checked = true;
            this.OverwriteFilesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.OverwriteFilesCheckBox.Location = new System.Drawing.Point(15, 62);
            this.OverwriteFilesCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.OverwriteFilesCheckBox.Name = "OverwriteFilesCheckBox";
            this.OverwriteFilesCheckBox.Size = new System.Drawing.Size(195, 24);
            this.OverwriteFilesCheckBox.TabIndex = 1;
            this.OverwriteFilesCheckBox.Text = "输出文件覆盖现有文件";
            this.OverwriteFilesCheckBox.UseVisualStyleBackColor = true;
            // 
            // ProcessButton
            // 
            this.ProcessButton.Location = new System.Drawing.Point(22, 277);
            this.ProcessButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ProcessButton.Name = "ProcessButton";
            this.ProcessButton.Size = new System.Drawing.Size(112, 35);
            this.ProcessButton.TabIndex = 9;
            this.ProcessButton.Text = "处理";
            this.ProcessButton.UseVisualStyleBackColor = true;
            this.ProcessButton.Click += new System.EventHandler(this.ProcessButton_Click);
            // 
            // StatusProgressBar
            // 
            this.StatusProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.StatusProgressBar.Location = new System.Drawing.Point(144, 277);
            this.StatusProgressBar.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.StatusProgressBar.Name = "StatusProgressBar";
            this.StatusProgressBar.Size = new System.Drawing.Size(789, 35);
            this.StatusProgressBar.TabIndex = 10;
            this.StatusProgressBar.Visible = false;
            // 
            // LogTextBox
            // 
            this.LogTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LogTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.LogTextBox.Location = new System.Drawing.Point(22, 322);
            this.LogTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.LogTextBox.Multiline = true;
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.ReadOnly = true;
            this.LogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogTextBox.Size = new System.Drawing.Size(908, 244);
            this.LogTextBox.TabIndex = 11;
            // 
            // YftConverterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(951, 555);
            this.Controls.Add(this.LogTextBox);
            this.Controls.Add(this.StatusProgressBar);
            this.Controls.Add(this.ProcessButton);
            this.Controls.Add(this.OptionsGroupBox);
            this.Controls.Add(this.OutputFormatGroupBox);
            this.Controls.Add(this.OutputFolderBrowseButton);
            this.Controls.Add(this.OutputFolderTextBox);
            this.Controls.Add(this.OutputFolderLabel);
            this.Controls.Add(this.InputFolderBrowseButton);
            this.Controls.Add(this.InputFolderTextBox);
            this.Controls.Add(this.InputFolderLabel);
            this.Controls.Add(this.DescriptionLabel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MinimumSize = new System.Drawing.Size(964, 585);
            this.Name = "YftConverterForm";
            this.Text = "GTA模型转换器 作者DK QQ2118473619 B站:https://space.bilibili.com/11780268";
            this.Load += new System.EventHandler(this.YftConverterForm_Load);
            this.OutputFormatGroupBox.ResumeLayout(false);
            this.OutputFormatGroupBox.PerformLayout();
            this.OptionsGroupBox.ResumeLayout(false);
            this.OptionsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label DescriptionLabel;
        private System.Windows.Forms.Label InputFolderLabel;
        private System.Windows.Forms.TextBox InputFolderTextBox;
        private System.Windows.Forms.Button InputFolderBrowseButton;
        private System.Windows.Forms.Label OutputFolderLabel;
        private System.Windows.Forms.TextBox OutputFolderTextBox;
        private System.Windows.Forms.Button OutputFolderBrowseButton;
        private System.Windows.Forms.GroupBox OutputFormatGroupBox;
        private System.Windows.Forms.RadioButton Gen8XmlRadioButton;
        private System.Windows.Forms.RadioButton Gen8YftRadioButton;
        private System.Windows.Forms.RadioButton Gen9YftRadioButton;
        private System.Windows.Forms.GroupBox OptionsGroupBox;
        private System.Windows.Forms.CheckBox IncludeSubfoldersCheckBox;
        private System.Windows.Forms.CheckBox OverwriteFilesCheckBox;
        private System.Windows.Forms.Button ProcessButton;
        private System.Windows.Forms.ProgressBar StatusProgressBar;
        private System.Windows.Forms.TextBox LogTextBox;
    }
}