namespace ENV.UI
{
    partial class ExportDataDialog
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
            this.openFileCheckBox = new System.Windows.Forms.CheckBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.typeLabel = new System.Windows.Forms.Label();
            this.typeComboBox = new System.Windows.Forms.ComboBox();
            this.fileNameLabel = new System.Windows.Forms.Label();
            this.upButton = new System.Windows.Forms.Button();
            this.fileNameTextBox = new System.Windows.Forms.TextBox();
            this.selectedColumnsListBox = new System.Windows.Forms.ListBox();
            this.downButton = new System.Windows.Forms.Button();
            this.addButton = new System.Windows.Forms.Button();
            this.removeButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.columnsListBox = new System.Windows.Forms.ListBox();
            this.templateTextBox = new System.Windows.Forms.TextBox();
            this.templateLabel = new System.Windows.Forms.Label();
            this.templateBrowseButton = new System.Windows.Forms.Button();
            this.delimiterComboBox = new System.Windows.Forms.ComboBox();
            this.delimiterLabel = new System.Windows.Forms.Label();
            this.columnsLabel = new System.Windows.Forms.Label();
            this.selectedColumnsLabel = new System.Windows.Forms.Label();
            this.addAllButton = new System.Windows.Forms.Button();
            this.removeAllButton = new System.Windows.Forms.Button();
            this.xsdOrHeaderCheckbox = new System.Windows.Forms.CheckBox();
            this.txtOtherDelimeter = new System.Windows.Forms.TextBox();
            this.txtOtherStringIdentifier = new System.Windows.Forms.TextBox();
            this.cboStringIdentifier = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // openFileCheckBox
            // 
            this.openFileCheckBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.openFileCheckBox.AutoSize = true;
            this.openFileCheckBox.Checked = true;
            this.openFileCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.openFileCheckBox.Location = new System.Drawing.Point(394, 67);
            this.openFileCheckBox.Name = "openFileCheckBox";
            this.openFileCheckBox.Size = new System.Drawing.Size(71, 17);
            this.openFileCheckBox.TabIndex = 8;
            this.openFileCheckBox.Text = "Open File";
            this.openFileCheckBox.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(300, 323);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 21;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.button1.Location = new System.Drawing.Point(362, 63);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(24, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // typeLabel
            // 
            this.typeLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.typeLabel.AutoSize = true;
            this.typeLabel.Location = new System.Drawing.Point(34, 15);
            this.typeLabel.Name = "typeLabel";
            this.typeLabel.Size = new System.Drawing.Size(31, 13);
            this.typeLabel.TabIndex = 0;
            this.typeLabel.Text = "Type";
            // 
            // typeComboBox
            // 
            this.typeComboBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.typeComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            this.typeComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.typeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.typeComboBox.FormattingEnabled = true;
            this.typeComboBox.Location = new System.Drawing.Point(74, 12);
            this.typeComboBox.Name = "typeComboBox";
            this.typeComboBox.Size = new System.Drawing.Size(387, 21);
            this.typeComboBox.TabIndex = 1;
            this.typeComboBox.SelectedValueChanged += new System.EventHandler(this.typeComboBox_SelectedValueChanged);
            this.typeComboBox.TextChanged += new System.EventHandler(this.typeComboBox_TextChanged);
            // 
            // fileNameLabel
            // 
            this.fileNameLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.fileNameLabel.AutoSize = true;
            this.fileNameLabel.Location = new System.Drawing.Point(11, 69);
            this.fileNameLabel.Name = "fileNameLabel";
            this.fileNameLabel.Size = new System.Drawing.Size(54, 13);
            this.fileNameLabel.TabIndex = 2;
            this.fileNameLabel.Text = "File Name";
            // 
            // upButton
            // 
            this.upButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.upButton.Location = new System.Drawing.Point(245, 131);
            this.upButton.Name = "upButton";
            this.upButton.Size = new System.Drawing.Size(43, 23);
            this.upButton.TabIndex = 13;
            this.upButton.Text = "Up";
            this.upButton.UseVisualStyleBackColor = true;
            this.upButton.Click += new System.EventHandler(this.upButton_Click);
            // 
            // fileNameTextBox
            // 
            this.fileNameTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.fileNameTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.fileNameTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.fileNameTextBox.Location = new System.Drawing.Point(74, 66);
            this.fileNameTextBox.Name = "fileNameTextBox";
            this.fileNameTextBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.fileNameTextBox.Size = new System.Drawing.Size(282, 20);
            this.fileNameTextBox.TabIndex = 6;
            this.fileNameTextBox.TextChanged += new System.EventHandler(this.fileNameTextBox_TextChanged);
            // 
            // selectedColumnsListBox
            // 
            this.selectedColumnsListBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.selectedColumnsListBox.FormattingEnabled = true;
            this.selectedColumnsListBox.Location = new System.Drawing.Point(297, 131);
            this.selectedColumnsListBox.Name = "selectedColumnsListBox";
            this.selectedColumnsListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.selectedColumnsListBox.Size = new System.Drawing.Size(164, 186);
            this.selectedColumnsListBox.TabIndex = 19;
            this.selectedColumnsListBox.SelectedIndexChanged += new System.EventHandler(this.ListBox_SelectedIndexChanged);
            this.selectedColumnsListBox.DoubleClick += new System.EventHandler(this.selectedColumnsListBox_DoubleClick);
            // 
            // downButton
            // 
            this.downButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.downButton.Location = new System.Drawing.Point(245, 160);
            this.downButton.Name = "downButton";
            this.downButton.Size = new System.Drawing.Size(43, 23);
            this.downButton.TabIndex = 14;
            this.downButton.Text = "Down";
            this.downButton.UseVisualStyleBackColor = true;
            this.downButton.Click += new System.EventHandler(this.downButton_Click);
            // 
            // addButton
            // 
            this.addButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.addButton.Location = new System.Drawing.Point(245, 236);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(43, 23);
            this.addButton.TabIndex = 16;
            this.addButton.Text = ">";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.addButton_Click);
            // 
            // removeButton
            // 
            this.removeButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.removeButton.Location = new System.Drawing.Point(245, 265);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(43, 23);
            this.removeButton.TabIndex = 17;
            this.removeButton.Text = "<";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(381, 323);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 20;
            this.okButton.Text = "Ok";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // columnsListBox
            // 
            this.columnsListBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.columnsListBox.FormattingEnabled = true;
            this.columnsListBox.Location = new System.Drawing.Point(74, 131);
            this.columnsListBox.Name = "columnsListBox";
            this.columnsListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.columnsListBox.Size = new System.Drawing.Size(164, 186);
            this.columnsListBox.TabIndex = 12;
            this.columnsListBox.SelectedIndexChanged += new System.EventHandler(this.ListBox_SelectedIndexChanged);
            this.columnsListBox.DoubleClick += new System.EventHandler(this.columnsListBox_DoubleClick);
            // 
            // templateTextBox
            // 
            this.templateTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.templateTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.templateTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.templateTextBox.Location = new System.Drawing.Point(74, 92);
            this.templateTextBox.Name = "templateTextBox";
            this.templateTextBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.templateTextBox.Size = new System.Drawing.Size(282, 20);
            this.templateTextBox.TabIndex = 9;
            this.templateTextBox.TextChanged += new System.EventHandler(this.fileNameTextBox_TextChanged);
            // 
            // templateLabel
            // 
            this.templateLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.templateLabel.AutoSize = true;
            this.templateLabel.Location = new System.Drawing.Point(14, 95);
            this.templateLabel.Name = "templateLabel";
            this.templateLabel.Size = new System.Drawing.Size(51, 13);
            this.templateLabel.TabIndex = 6;
            this.templateLabel.Text = "Template";
            // 
            // templateBrowseButton
            // 
            this.templateBrowseButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.templateBrowseButton.Location = new System.Drawing.Point(362, 89);
            this.templateBrowseButton.Name = "templateBrowseButton";
            this.templateBrowseButton.Size = new System.Drawing.Size(24, 23);
            this.templateBrowseButton.TabIndex = 10;
            this.templateBrowseButton.Text = "...";
            this.templateBrowseButton.UseVisualStyleBackColor = true;
            this.templateBrowseButton.Click += new System.EventHandler(this.templateBrowseButton_Click);
            // 
            // delimiterComboBox
            // 
            this.delimiterComboBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.delimiterComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            this.delimiterComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.delimiterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.delimiterComboBox.FormattingEnabled = true;
            this.delimiterComboBox.Location = new System.Drawing.Point(74, 39);
            this.delimiterComboBox.Name = "delimiterComboBox";
            this.delimiterComboBox.Size = new System.Drawing.Size(125, 21);
            this.delimiterComboBox.TabIndex = 2;
            this.delimiterComboBox.TextChanged += new System.EventHandler(this.typeComboBox_TextChanged);
            // 
            // delimiterLabel
            // 
            this.delimiterLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.delimiterLabel.AutoSize = true;
            this.delimiterLabel.Location = new System.Drawing.Point(17, 42);
            this.delimiterLabel.Name = "delimiterLabel";
            this.delimiterLabel.Size = new System.Drawing.Size(47, 13);
            this.delimiterLabel.TabIndex = 0;
            this.delimiterLabel.Text = "Delimiter";
            // 
            // columnsLabel
            // 
            this.columnsLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.columnsLabel.AutoSize = true;
            this.columnsLabel.Location = new System.Drawing.Point(74, 115);
            this.columnsLabel.Name = "columnsLabel";
            this.columnsLabel.Size = new System.Drawing.Size(93, 13);
            this.columnsLabel.TabIndex = 6;
            this.columnsLabel.Text = "Available Columns";
            // 
            // selectedColumnsLabel
            // 
            this.selectedColumnsLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.selectedColumnsLabel.AutoSize = true;
            this.selectedColumnsLabel.Location = new System.Drawing.Point(297, 115);
            this.selectedColumnsLabel.Name = "selectedColumnsLabel";
            this.selectedColumnsLabel.Size = new System.Drawing.Size(92, 13);
            this.selectedColumnsLabel.TabIndex = 6;
            this.selectedColumnsLabel.Text = "Selected Columns";
            // 
            // addAllButton
            // 
            this.addAllButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.addAllButton.Location = new System.Drawing.Point(245, 207);
            this.addAllButton.Name = "addAllButton";
            this.addAllButton.Size = new System.Drawing.Size(43, 23);
            this.addAllButton.TabIndex = 15;
            this.addAllButton.Text = ">>";
            this.addAllButton.UseVisualStyleBackColor = true;
            this.addAllButton.Click += new System.EventHandler(this.addAllButton_Click);
            // 
            // removeAllButton
            // 
            this.removeAllButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.removeAllButton.Location = new System.Drawing.Point(245, 294);
            this.removeAllButton.Name = "removeAllButton";
            this.removeAllButton.Size = new System.Drawing.Size(43, 23);
            this.removeAllButton.TabIndex = 18;
            this.removeAllButton.Text = "<<";
            this.removeAllButton.UseVisualStyleBackColor = true;
            this.removeAllButton.Click += new System.EventHandler(this.removeAllButton_Click);
            // 
            // xsdOrHeaderCheckbox
            // 
            this.xsdOrHeaderCheckbox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.xsdOrHeaderCheckbox.AutoSize = true;
            this.xsdOrHeaderCheckbox.Location = new System.Drawing.Point(394, 93);
            this.xsdOrHeaderCheckbox.Name = "xsdOrHeaderCheckbox";
            this.xsdOrHeaderCheckbox.Size = new System.Drawing.Size(78, 17);
            this.xsdOrHeaderCheckbox.TabIndex = 11;
            this.xsdOrHeaderCheckbox.Text = "Create Xsd";
            this.xsdOrHeaderCheckbox.UseVisualStyleBackColor = true;
            // 
            // txtOtherDelimeter
            // 
            this.txtOtherDelimeter.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtOtherDelimeter.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtOtherDelimeter.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.txtOtherDelimeter.Location = new System.Drawing.Point(205, 40);
            this.txtOtherDelimeter.MaxLength = 1;
            this.txtOtherDelimeter.Name = "txtOtherDelimeter";
            this.txtOtherDelimeter.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtOtherDelimeter.Size = new System.Drawing.Size(15, 20);
            this.txtOtherDelimeter.TabIndex = 3;
            this.txtOtherDelimeter.TextChanged += new System.EventHandler(this.fileNameTextBox_TextChanged);
            // 
            // txtOtherStringIdentifier
            // 
            this.txtOtherStringIdentifier.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtOtherStringIdentifier.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtOtherStringIdentifier.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.txtOtherStringIdentifier.Location = new System.Drawing.Point(446, 40);
            this.txtOtherStringIdentifier.MaxLength = 1;
            this.txtOtherStringIdentifier.Name = "txtOtherStringIdentifier";
            this.txtOtherStringIdentifier.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtOtherStringIdentifier.Size = new System.Drawing.Size(15, 20);
            this.txtOtherStringIdentifier.TabIndex = 5;
            this.txtOtherStringIdentifier.TextChanged += new System.EventHandler(this.fileNameTextBox_TextChanged);
            // 
            // cboStringIdentifier
            // 
            this.cboStringIdentifier.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cboStringIdentifier.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            this.cboStringIdentifier.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboStringIdentifier.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStringIdentifier.FormattingEnabled = true;
            this.cboStringIdentifier.Location = new System.Drawing.Point(309, 39);
            this.cboStringIdentifier.Name = "cboStringIdentifier";
            this.cboStringIdentifier.Size = new System.Drawing.Size(131, 21);
            this.cboStringIdentifier.TabIndex = 4;
            this.cboStringIdentifier.TextChanged += new System.EventHandler(this.typeComboBox_TextChanged);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(226, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "String Identifier";
            // 
            // ExportDataDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(488, 357);
            this.Controls.Add(this.xsdOrHeaderCheckbox);
            this.Controls.Add(this.openFileCheckBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.templateBrowseButton);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.delimiterLabel);
            this.Controls.Add(this.cboStringIdentifier);
            this.Controls.Add(this.typeLabel);
            this.Controls.Add(this.delimiterComboBox);
            this.Controls.Add(this.typeComboBox);
            this.Controls.Add(this.selectedColumnsLabel);
            this.Controls.Add(this.columnsLabel);
            this.Controls.Add(this.templateLabel);
            this.Controls.Add(this.fileNameLabel);
            this.Controls.Add(this.upButton);
            this.Controls.Add(this.txtOtherStringIdentifier);
            this.Controls.Add(this.templateTextBox);
            this.Controls.Add(this.txtOtherDelimeter);
            this.Controls.Add(this.fileNameTextBox);
            this.Controls.Add(this.selectedColumnsListBox);
            this.Controls.Add(this.downButton);
            this.Controls.Add(this.removeAllButton);
            this.Controls.Add(this.addAllButton);
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.removeButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.columnsListBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportDataDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Export Data";
            this.Load += new System.EventHandler(this.ExportDataDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox openFileCheckBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label typeLabel;
        private System.Windows.Forms.ComboBox typeComboBox;
        private System.Windows.Forms.Label fileNameLabel;
        private System.Windows.Forms.Button upButton;
        private System.Windows.Forms.TextBox fileNameTextBox;
        private System.Windows.Forms.ListBox selectedColumnsListBox;
        private System.Windows.Forms.Button downButton;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.ListBox columnsListBox;
        private System.Windows.Forms.TextBox templateTextBox;
        private System.Windows.Forms.Label templateLabel;
        private System.Windows.Forms.Button templateBrowseButton;
        private System.Windows.Forms.ComboBox delimiterComboBox;
        private System.Windows.Forms.Label delimiterLabel;
        private System.Windows.Forms.Label columnsLabel;
        private System.Windows.Forms.Label selectedColumnsLabel;
        private System.Windows.Forms.Button addAllButton;
        private System.Windows.Forms.Button removeAllButton;
        private System.Windows.Forms.CheckBox xsdOrHeaderCheckbox;
        private System.Windows.Forms.TextBox txtOtherDelimeter;
        private System.Windows.Forms.TextBox txtOtherStringIdentifier;
        private System.Windows.Forms.ComboBox cboStringIdentifier;
        private System.Windows.Forms.Label label1;
    }
}