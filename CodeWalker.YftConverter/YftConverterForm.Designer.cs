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
            this.DescriptionLabel = new System.Windows.Forms.Label();
            this.InputFolderLabel = new System.Windows.Forms.Label();
            this.InputFolderTextBox = new System.Windows.Forms.TextBox();
            this.InputFolderBrowseButton = new System.Windows.Forms.Button();
            this.OutputFolderLabel = new System.Windows.Forms.Label();
            this.OutputFolderTextBox = new System.Windows.Forms.TextBox();
            this.OutputFolderBrowseButton = new System.Windows.Forms.Button();
            this.OutputFormatGroupBox = new System.Windows.Forms.GroupBox();
            this.Gen8XmlRadioButton = new System.Windows.Forms.RadioButton();
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
            this.DescriptionLabel.Location = new System.Drawing.Point(12, 9);
            this.DescriptionLabel.Name = "DescriptionLabel";
            this.DescriptionLabel.Size = new System.Drawing.Size(610, 20);
            this.DescriptionLabel.TabIndex = 0;
            this.DescriptionLabel.Text = "Convert YFT files between different formats (Gen8 XML and Gen9 YFT Compressed).";
            // 
            // InputFolderLabel
            // 
            this.InputFolderLabel.AutoSize = true;
            this.InputFolderLabel.Location = new System.Drawing.Point(12, 39);
            this.InputFolderLabel.Name = "InputFolderLabel";
            this.InputFolderLabel.Size = new System.Drawing.Size(66, 13);
            this.InputFolderLabel.TabIndex = 1;
            this.InputFolderLabel.Text = "Input folder:";
            // 
            // InputFolderTextBox
            // 
            this.InputFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InputFolderTextBox.Location = new System.Drawing.Point(84, 36);
            this.InputFolderTextBox.Name = "InputFolderTextBox";
            this.InputFolderTextBox.Size = new System.Drawing.Size(457, 20);
            this.InputFolderTextBox.TabIndex = 2;
            // 
            // InputFolderBrowseButton
            // 
            this.InputFolderBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.InputFolderBrowseButton.Location = new System.Drawing.Point(547, 34);
            this.InputFolderBrowseButton.Name = "InputFolderBrowseButton";
            this.InputFolderBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.InputFolderBrowseButton.TabIndex = 3;
            this.InputFolderBrowseButton.Text = "...";
            this.InputFolderBrowseButton.UseVisualStyleBackColor = true;
            this.InputFolderBrowseButton.Click += new System.EventHandler(this.InputFolderBrowseButton_Click);
            // 
            // OutputFolderLabel
            // 
            this.OutputFolderLabel.AutoSize = true;
            this.OutputFolderLabel.Location = new System.Drawing.Point(12, 65);
            this.OutputFolderLabel.Name = "OutputFolderLabel";
            this.OutputFolderLabel.Size = new System.Drawing.Size(71, 13);
            this.OutputFolderLabel.TabIndex = 4;
            this.OutputFolderLabel.Text = "Output folder:";
            // 
            // OutputFolderTextBox
            // 
            this.OutputFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputFolderTextBox.Location = new System.Drawing.Point(84, 62);
            this.OutputFolderTextBox.Name = "OutputFolderTextBox";
            this.OutputFolderTextBox.Size = new System.Drawing.Size(457, 20);
            this.OutputFolderTextBox.TabIndex = 5;
            // 
            // OutputFolderBrowseButton
            // 
            this.OutputFolderBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputFolderBrowseButton.Location = new System.Drawing.Point(547, 60);
            this.OutputFolderBrowseButton.Name = "OutputFolderBrowseButton";
            this.OutputFolderBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.OutputFolderBrowseButton.TabIndex = 6;
            this.OutputFolderBrowseButton.Text = "...";
            this.OutputFolderBrowseButton.UseVisualStyleBackColor = true;
            this.OutputFolderBrowseButton.Click += new System.EventHandler(this.OutputFolderBrowseButton_Click);
            // 
            // OutputFormatGroupBox
            // 
            this.OutputFormatGroupBox.Controls.Add(this.Gen8XmlRadioButton);
            this.OutputFormatGroupBox.Controls.Add(this.Gen9YftRadioButton);
            this.OutputFormatGroupBox.Location = new System.Drawing.Point(15, 89);
            this.OutputFormatGroupBox.Name = "OutputFormatGroupBox";
            this.OutputFormatGroupBox.Size = new System.Drawing.Size(300, 65);
            this.OutputFormatGroupBox.TabIndex = 7;
            this.OutputFormatGroupBox.TabStop = false;
            this.OutputFormatGroupBox.Text = "Output Format";
            // 
            // Gen8XmlRadioButton
            // 
            this.Gen8XmlRadioButton.AutoSize = true;
            this.Gen8XmlRadioButton.Location = new System.Drawing.Point(10, 19);
            this.Gen8XmlRadioButton.Name = "Gen8XmlRadioButton";
            this.Gen8XmlRadioButton.Size = new System.Drawing.Size(77, 17);
            this.Gen8XmlRadioButton.TabIndex = 0;
            this.Gen8XmlRadioButton.Text = "Gen8 XML";
            this.Gen8XmlRadioButton.UseVisualStyleBackColor = true;
            // 
            // Gen9YftRadioButton
            // 
            this.Gen9YftRadioButton.AutoSize = true;
            this.Gen9YftRadioButton.Checked = true;
            this.Gen9YftRadioButton.Location = new System.Drawing.Point(10, 40);
            this.Gen9YftRadioButton.Name = "Gen9YftRadioButton";
            this.Gen9YftRadioButton.Size = new System.Drawing.Size(146, 17);
            this.Gen9YftRadioButton.TabIndex = 1;
            this.Gen9YftRadioButton.TabStop = true;
            this.Gen9YftRadioButton.Text = "Gen9 YFT (Compressed)";
            this.Gen9YftRadioButton.UseVisualStyleBackColor = true;
            // 
            // OptionsGroupBox
            // 
            this.OptionsGroupBox.Controls.Add(this.IncludeSubfoldersCheckBox);
            this.OptionsGroupBox.Controls.Add(this.OverwriteFilesCheckBox);
            this.OptionsGroupBox.Location = new System.Drawing.Point(321, 89);
            this.OptionsGroupBox.Name = "OptionsGroupBox";
            this.OptionsGroupBox.Size = new System.Drawing.Size(301, 65);
            this.OptionsGroupBox.TabIndex = 8;
            this.OptionsGroupBox.TabStop = false;
            this.OptionsGroupBox.Text = "Options";
            // 
            // IncludeSubfoldersCheckBox
            // 
            this.IncludeSubfoldersCheckBox.AutoSize = true;
            this.IncludeSubfoldersCheckBox.Checked = true;
            this.IncludeSubfoldersCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.IncludeSubfoldersCheckBox.Location = new System.Drawing.Point(10, 19);
            this.IncludeSubfoldersCheckBox.Name = "IncludeSubfoldersCheckBox";
            this.IncludeSubfoldersCheckBox.Size = new System.Drawing.Size(114, 17);
            this.IncludeSubfoldersCheckBox.TabIndex = 0;
            this.IncludeSubfoldersCheckBox.Text = "Include subfolders";
            this.IncludeSubfoldersCheckBox.UseVisualStyleBackColor = true;
            // 
            // OverwriteFilesCheckBox
            // 
            this.OverwriteFilesCheckBox.AutoSize = true;
            this.OverwriteFilesCheckBox.Checked = true;
            this.OverwriteFilesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.OverwriteFilesCheckBox.Location = new System.Drawing.Point(10, 40);
            this.OverwriteFilesCheckBox.Name = "OverwriteFilesCheckBox";
            this.OverwriteFilesCheckBox.Size = new System.Drawing.Size(130, 17);
            this.OverwriteFilesCheckBox.TabIndex = 1;
            this.OverwriteFilesCheckBox.Text = "Overwrite existing files";
            this.OverwriteFilesCheckBox.UseVisualStyleBackColor = true;
            // 
            // ProcessButton
            // 
            this.ProcessButton.Location = new System.Drawing.Point(15, 160);
            this.ProcessButton.Name = "ProcessButton";
            this.ProcessButton.Size = new System.Drawing.Size(75, 23);
            this.ProcessButton.TabIndex = 9;
            this.ProcessButton.Text = "Process";
            this.ProcessButton.UseVisualStyleBackColor = true;
            this.ProcessButton.Click += new System.EventHandler(this.ProcessButton_Click);
            // 
            // StatusProgressBar
            // 
            this.StatusProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.StatusProgressBar.Location = new System.Drawing.Point(96, 160);
            this.StatusProgressBar.Name = "StatusProgressBar";
            this.StatusProgressBar.Size = new System.Drawing.Size(526, 23);
            this.StatusProgressBar.TabIndex = 10;
            this.StatusProgressBar.Visible = false;
            // 
            // LogTextBox
            // 
            this.LogTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LogTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.LogTextBox.Location = new System.Drawing.Point(15, 189);
            this.LogTextBox.Multiline = true;
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.ReadOnly = true;
            this.LogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogTextBox.Size = new System.Drawing.Size(607, 160);
            this.LogTextBox.TabIndex = 11;
            // 
            // YftConverterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 361);
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
            this.MinimumSize = new System.Drawing.Size(650, 400);
            this.Name = "YftConverterForm";
            this.Text = "YFT Converter - CodeWalker by dexyfex";
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
        private System.Windows.Forms.RadioButton Gen9YftRadioButton;
        private System.Windows.Forms.GroupBox OptionsGroupBox;
        private System.Windows.Forms.CheckBox IncludeSubfoldersCheckBox;
        private System.Windows.Forms.CheckBox OverwriteFilesCheckBox;
        private System.Windows.Forms.Button ProcessButton;
        private System.Windows.Forms.ProgressBar StatusProgressBar;
        private System.Windows.Forms.TextBox LogTextBox;
    }
}