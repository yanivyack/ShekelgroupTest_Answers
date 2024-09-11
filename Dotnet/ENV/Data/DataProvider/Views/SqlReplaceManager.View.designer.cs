namespace ENV.Data.DataProvider.Views
{
    partial class SqlReplaceManagerView
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
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showMergedSQLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runAndCompareSQLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.grid1 = new ENV.UI.Grid();
            this.gcId = new ENV.UI.GridColumn();
            this.txtId = new ENV.UI.TextBox();
            this.gcLastRun = new ENV.UI.GridColumn();
            this.txtLastRun = new ENV.UI.TextBox();
            this.gcOriginalSQLTemplateKey = new ENV.UI.GridColumn();
            this.txtOriginalSQLTemplateKey = new ENV.UI.TextBox();
            this.gcComments = new ENV.UI.GridColumn();
            this.txtComments = new ENV.UI.TextBox();
            this.gcError = new ENV.UI.GridColumn();
            this.txtError = new ENV.UI.TextBox();
            this.gcChanged = new ENV.UI.GridColumn();
            this.txtChanged = new ENV.UI.TextBox();
            this.gcFixed1 = new ENV.UI.GridColumn();
            this.txtFixed1 = new ENV.UI.TextBox();
            this.gcFixed2 = new ENV.UI.GridColumn();
            this.txtFixed2 = new ENV.UI.TextBox();
            this.gcRefresh = new ENV.UI.GridColumn();
            this.txtRefresh = new ENV.UI.CheckBox();
            this.label1 = new ENV.UI.Label();
            this.tabControl1 = new ENV.UI.TabControl();
            this.textBox2 = new ENV.UI.TextBox();
            this.label6 = new ENV.UI.Label();
            this.textBox6 = new ENV.UI.TextBox();
            this.label5 = new ENV.UI.Label();
            this.textBox5 = new ENV.UI.TextBox();
            this.textBox3 = new ENV.UI.TextBox();
            this.label4 = new ENV.UI.Label();
            this.textBox4 = new ENV.UI.TextBox();
            this.label3 = new ENV.UI.Label();
            this.label2 = new ENV.UI.Label();
            this.textBox1 = new ENV.UI.TextBox();
            this.label7 = new ENV.UI.Label();
            this.textBox7 = new ENV.UI.TextBox();
            this.label8 = new ENV.UI.Label();
            this.textBox8 = new ENV.UI.TextBox();
            this.contextMenuStrip1.SuspendLayout();
            this.grid1.SuspendLayout();
            this.gcId.SuspendLayout();
            this.gcLastRun.SuspendLayout();
            this.gcOriginalSQLTemplateKey.SuspendLayout();
            this.gcComments.SuspendLayout();
            this.gcError.SuspendLayout();
            this.gcChanged.SuspendLayout();
            this.gcFixed1.SuspendLayout();
            this.gcFixed2.SuspendLayout();
            this.gcRefresh.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showMergedSQLToolStripMenuItem,
            this.runAndCompareSQLToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(195, 48);
            // 
            // showMergedSQLToolStripMenuItem
            // 
            this.showMergedSQLToolStripMenuItem.Name = "showMergedSQLToolStripMenuItem";
            this.showMergedSQLToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.showMergedSQLToolStripMenuItem.Text = "Show Merged SQL";
            this.showMergedSQLToolStripMenuItem.Click += new System.EventHandler(this.showMergedSQLToolStripMenuItem_Click);
            // 
            // runAndCompareSQLToolStripMenuItem
            // 
            this.runAndCompareSQLToolStripMenuItem.Name = "runAndCompareSQLToolStripMenuItem";
            this.runAndCompareSQLToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.runAndCompareSQLToolStripMenuItem.Text = "Run and Compare SQL";
            this.runAndCompareSQLToolStripMenuItem.Click += new System.EventHandler(this.runAndCompareSQLToolStripMenuItem_Click);
            // 
            // grid1
            // 
            this.grid1.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 0);
            this.grid1.Controls.Add(this.gcId);
            this.grid1.Controls.Add(this.gcLastRun);
            this.grid1.Controls.Add(this.gcOriginalSQLTemplateKey);
            this.grid1.Controls.Add(this.gcComments);
            this.grid1.Controls.Add(this.gcError);
            this.grid1.Controls.Add(this.gcChanged);
            this.grid1.Controls.Add(this.gcFixed1);
            this.grid1.Controls.Add(this.gcFixed2);
            this.grid1.Controls.Add(this.gcRefresh);
            this.grid1.EnableGridEnhancementsCodeSample = true;
            this.grid1.Location = new System.Drawing.Point(0, 0);
            this.grid1.Name = "grid1";
            this.grid1.Size = new System.Drawing.Size(897, 209);
            this.grid1.Text = "grid1";
            this.grid1.UnderConstructionNewGridLook = true;
            // 
            // gcId
            // 
            this.gcId.AutoResize = false;
            this.gcId.Controls.Add(this.txtId);
            this.gcId.Name = "gcId";
            this.gcId.Text = "Id";
            this.gcId.Width = 45;
            // 
            // txtId
            // 
            this.txtId.Location = new System.Drawing.Point(2, 1);
            this.txtId.Name = "txtId";
            this.txtId.Size = new System.Drawing.Size(37, 18);
            this.txtId.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtId.Data = this._controller.s.Id;
            // 
            // gcLastRun
            // 
            this.gcLastRun.AutoResize = false;
            this.gcLastRun.Controls.Add(this.txtLastRun);
            this.gcLastRun.Name = "gcLastRun";
            this.gcLastRun.Text = "LastRun";
            this.gcLastRun.Width = 114;
            // 
            // txtLastRun
            // 
            this.txtLastRun.Location = new System.Drawing.Point(2, 1);
            this.txtLastRun.Name = "txtLastRun";
            this.txtLastRun.Size = new System.Drawing.Size(109, 18);
            this.txtLastRun.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtLastRun.Data = this._controller.s.LastRun;
            // 
            // gcOriginalSQLTemplateKey
            // 
            this.gcOriginalSQLTemplateKey.Controls.Add(this.txtOriginalSQLTemplateKey);
            this.gcOriginalSQLTemplateKey.Name = "gcOriginalSQLTemplateKey";
            this.gcOriginalSQLTemplateKey.Text = "OriginalSQLTemplateKey";
            this.gcOriginalSQLTemplateKey.Width = 182;
            // 
            // txtOriginalSQLTemplateKey
            // 
            this.txtOriginalSQLTemplateKey.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 0);
            this.txtOriginalSQLTemplateKey.Location = new System.Drawing.Point(2, 1);
            this.txtOriginalSQLTemplateKey.Name = "txtOriginalSQLTemplateKey";
            this.txtOriginalSQLTemplateKey.ReadOnly = true;
            this.txtOriginalSQLTemplateKey.Size = new System.Drawing.Size(176, 18);
            this.txtOriginalSQLTemplateKey.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtOriginalSQLTemplateKey.Data = this._controller.s.OriginalSQLTemplateKey;
            // 
            // gcComments
            // 
            this.gcComments.AutoResize = false;
            this.gcComments.Controls.Add(this.txtComments);
            this.gcComments.Name = "gcComments";
            this.gcComments.Text = "Comments";
            this.gcComments.Width = 319;
            // 
            // txtComments
            // 
            this.txtComments.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 0);
            this.txtComments.Location = new System.Drawing.Point(2, 1);
            this.txtComments.Name = "txtComments";
            this.txtComments.Size = new System.Drawing.Size(313, 18);
            this.txtComments.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtComments.Data = this._controller.s.Comments;
            // 
            // gcError
            // 
            this.gcError.AutoResize = false;
            this.gcError.Controls.Add(this.txtError);
            this.gcError.Name = "gcError";
            this.gcError.Text = "Error";
            this.gcError.Width = 37;
            // 
            // txtError
            // 
            this.txtError.Location = new System.Drawing.Point(2, 1);
            this.txtError.Name = "txtError";
            this.txtError.ReadOnly = true;
            this.txtError.Size = new System.Drawing.Size(29, 18);
            this.txtError.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtError.Data = this._controller.Error;
            // 
            // gcChanged
            // 
            this.gcChanged.AutoResize = false;
            this.gcChanged.Controls.Add(this.txtChanged);
            this.gcChanged.Name = "gcChanged";
            this.gcChanged.Text = "Changed";
            this.gcChanged.Width = 51;
            // 
            // txtChanged
            // 
            this.txtChanged.Location = new System.Drawing.Point(2, 1);
            this.txtChanged.Name = "txtChanged";
            this.txtChanged.ReadOnly = true;
            this.txtChanged.Size = new System.Drawing.Size(45, 18);
            this.txtChanged.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtChanged.Data = this._controller.Changed;
            // 
            // gcFixed1
            // 
            this.gcFixed1.AutoResize = false;
            this.gcFixed1.Controls.Add(this.txtFixed1);
            this.gcFixed1.Name = "gcFixed1";
            this.gcFixed1.Text = "Fixed1";
            this.gcFixed1.Width = 39;
            // 
            // txtFixed1
            // 
            this.txtFixed1.Location = new System.Drawing.Point(2, 1);
            this.txtFixed1.Name = "txtFixed1";
            this.txtFixed1.Size = new System.Drawing.Size(31, 18);
            this.txtFixed1.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtFixed1.Data = this._controller.s.Fixed1;
            // 
            // gcFixed2
            // 
            this.gcFixed2.AutoResize = false;
            this.gcFixed2.Controls.Add(this.txtFixed2);
            this.gcFixed2.Name = "gcFixed2";
            this.gcFixed2.Text = "Fixed2";
            this.gcFixed2.Width = 41;
            // 
            // txtFixed2
            // 
            this.txtFixed2.Location = new System.Drawing.Point(2, 1);
            this.txtFixed2.Name = "txtFixed2";
            this.txtFixed2.Size = new System.Drawing.Size(36, 18);
            this.txtFixed2.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtFixed2.Data = this._controller.s.Fixed2;
            // 
            // gcRefresh
            // 
            this.gcRefresh.Controls.Add(this.txtRefresh);
            this.gcRefresh.Name = "gcRefresh";
            this.gcRefresh.Text = "Refresh";
            this.gcRefresh.Width = 45;
            // 
            // txtRefresh
            // 
            this.txtRefresh.Location = new System.Drawing.Point(2, 1);
            this.txtRefresh.Name = "txtRefresh";
            this.txtRefresh.Size = new System.Drawing.Size(40, 18);
            this.txtRefresh.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtRefresh.Data = this._controller.s.Refresh;
            // 
            // label1
            // 
            this.label1.BoundTo = new Firefly.Box.UI.ControlBinding(this.tabControl1, 0);
            this.label1.Location = new System.Drawing.Point(5, 240);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 14);
            this.label1.Text = "Target Template";
            // 
            // tabControl1
            // 
            this.tabControl1.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 100);
            this.tabControl1.Location = new System.Drawing.Point(0, 215);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 3;
            this.tabControl1.Size = new System.Drawing.Size(897, 216);
            this.tabControl1.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.tabControl1.Values = "Template,Last Run,Merged,Callstack";
            this.tabControl1.Data = this._controller.Tab;
            // 
            // textBox2
            // 
            this.textBox2.AcceptsReturn = true;
            this.textBox2.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(50, 50, 0, 100);
            this.textBox2.AllowHorizontalScroll = true;
            this.textBox2.AllowVerticalScroll = true;
            this.textBox2.BoundTo = new Firefly.Box.UI.ControlBinding(this.tabControl1, 0);
            this.textBox2.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox2.Location = new System.Drawing.Point(451, 257);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ScrollBars = true;
            this.textBox2.Size = new System.Drawing.Size(440, 170);
            this.textBox2.WordWrap = false;
            this.textBox2.Data = this._controller.s.OriginalSQLTemplate;
            // 
            // label6
            // 
            this.label6.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(50, 0, 0, 0);
            this.label6.BoundTo = new Firefly.Box.UI.ControlBinding(this.tabControl1, 2);
            this.label6.Location = new System.Drawing.Point(453, 239);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(100, 14);
            this.label6.Text = "Original Merged SQL";
            // 
            // textBox6
            // 
            this.textBox6.AcceptsReturn = true;
            this.textBox6.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 50, 0, 100);
            this.textBox6.AllowHorizontalScroll = true;
            this.textBox6.AllowVerticalScroll = true;
            this.textBox6.BoundTo = new Firefly.Box.UI.ControlBinding(this.tabControl1, 2);
            this.textBox6.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox6.Location = new System.Drawing.Point(7, 256);
            this.textBox6.Multiline = true;
            this.textBox6.Name = "textBox6";
            this.textBox6.ScrollBars = true;
            this.textBox6.Size = new System.Drawing.Size(440, 170);
            this.textBox6.WordWrap = false;
            this.textBox6.Data = this._controller.s.TargetMergedSQL;
            // 
            // label5
            // 
            this.label5.BoundTo = new Firefly.Box.UI.ControlBinding(this.tabControl1, 2);
            this.label5.Location = new System.Drawing.Point(7, 239);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(100, 14);
            this.label5.Text = "Target Merged SQL";
            // 
            // textBox5
            // 
            this.textBox5.AcceptsReturn = true;
            this.textBox5.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(50, 50, 0, 100);
            this.textBox5.AllowHorizontalScroll = true;
            this.textBox5.AllowVerticalScroll = true;
            this.textBox5.BoundTo = new Firefly.Box.UI.ControlBinding(this.tabControl1, 2);
            this.textBox5.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox5.Location = new System.Drawing.Point(453, 256);
            this.textBox5.Multiline = true;
            this.textBox5.Name = "textBox5";
            this.textBox5.ScrollBars = true;
            this.textBox5.Size = new System.Drawing.Size(440, 170);
            this.textBox5.WordWrap = false;
            this.textBox5.Data = this._controller.s.OriginalMergedSQL;
            // 
            // textBox3
            // 
            this.textBox3.AcceptsReturn = true;
            this.textBox3.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(50, 50, 0, 100);
            this.textBox3.AllowHorizontalScroll = true;
            this.textBox3.AllowVerticalScroll = true;
            this.textBox3.BoundTo = new Firefly.Box.UI.ControlBinding(this.tabControl1, 1);
            this.textBox3.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox3.Location = new System.Drawing.Point(450, 257);
            this.textBox3.Multiline = true;
            this.textBox3.Name = "textBox3";
            this.textBox3.ScrollBars = true;
            this.textBox3.Size = new System.Drawing.Size(440, 170);
            this.textBox3.WordWrap = false;
            this.textBox3.Data = this._controller.s.TheParams;
            // 
            // label4
            // 
            this.label4.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(50, 0, 0, 0);
            this.label4.BoundTo = new Firefly.Box.UI.ControlBinding(this.tabControl1, 1);
            this.label4.Location = new System.Drawing.Point(450, 240);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 14);
            this.label4.Text = "Parameters";
            // 
            // textBox4
            // 
            this.textBox4.AcceptsReturn = true;
            this.textBox4.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 50, 0, 100);
            this.textBox4.AllowHorizontalScroll = true;
            this.textBox4.AllowVerticalScroll = true;
            this.textBox4.BoundTo = new Firefly.Box.UI.ControlBinding(this.tabControl1, 1);
            this.textBox4.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox4.Location = new System.Drawing.Point(4, 257);
            this.textBox4.Multiline = true;
            this.textBox4.Name = "textBox4";
            this.textBox4.ScrollBars = true;
            this.textBox4.Size = new System.Drawing.Size(440, 170);
            this.textBox4.WordWrap = false;
            this.textBox4.Data = this._controller.s.LastError;
            // 
            // label3
            // 
            this.label3.BoundTo = new Firefly.Box.UI.ControlBinding(this.tabControl1, 1);
            this.label3.Location = new System.Drawing.Point(4, 240);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 14);
            this.label3.Text = "Last Error";
            // 
            // label2
            // 
            this.label2.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(50, 0, 0, 0);
            this.label2.BoundTo = new Firefly.Box.UI.ControlBinding(this.tabControl1, 0);
            this.label2.Location = new System.Drawing.Point(451, 240);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 14);
            this.label2.Text = "Original Template";
            // 
            // textBox1
            // 
            this.textBox1.AcceptsReturn = true;
            this.textBox1.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 50, 0, 100);
            this.textBox1.AllowHorizontalScroll = true;
            this.textBox1.AllowVerticalScroll = true;
            this.textBox1.BoundTo = new Firefly.Box.UI.ControlBinding(this.tabControl1, 0);
            this.textBox1.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(5, 257);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = true;
            this.textBox1.Size = new System.Drawing.Size(440, 170);
            this.textBox1.WordWrap = false;
            this.textBox1.Data = this._controller.s.TargetSQLTemplate;
            // 
            // label7
            // 
            this.label7.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(50, 0, 0, 0);
            this.label7.BoundTo = new Firefly.Box.UI.ControlBinding(this.tabControl1, 3);
            this.label7.Location = new System.Drawing.Point(453, 239);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(100, 14);
            this.label7.Text = "Magic Location";
            // 
            // textBox7
            // 
            this.textBox7.AcceptsReturn = true;
            this.textBox7.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 50, 0, 100);
            this.textBox7.AllowHorizontalScroll = true;
            this.textBox7.AllowVerticalScroll = true;
            this.textBox7.BoundTo = new Firefly.Box.UI.ControlBinding(this.tabControl1, 3);
            this.textBox7.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox7.Location = new System.Drawing.Point(7, 256);
            this.textBox7.Multiline = true;
            this.textBox7.Name = "textBox7";
            this.textBox7.ScrollBars = true;
            this.textBox7.Size = new System.Drawing.Size(440, 170);
            this.textBox7.WordWrap = false;
            this.textBox7.Data = this._controller.s.TheClass;
            // 
            // label8
            // 
            this.label8.BoundTo = new Firefly.Box.UI.ControlBinding(this.tabControl1, 3);
            this.label8.Location = new System.Drawing.Point(7, 239);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(100, 14);
            this.label8.Text = "Callstack";
            // 
            // textBox8
            // 
            this.textBox8.AcceptsReturn = true;
            this.textBox8.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(50, 50, 0, 100);
            this.textBox8.AllowHorizontalScroll = true;
            this.textBox8.AllowVerticalScroll = true;
            this.textBox8.BoundTo = new Firefly.Box.UI.ControlBinding(this.tabControl1, 3);
            this.textBox8.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox8.Location = new System.Drawing.Point(453, 256);
            this.textBox8.Multiline = true;
            this.textBox8.Name = "textBox8";
            this.textBox8.ScrollBars = true;
            this.textBox8.Size = new System.Drawing.Size(440, 170);
            this.textBox8.WordWrap = false;
            this.textBox8.Data = this._controller.s.LocationInMagic;
            // 
            // SqlReplaceManagerView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(900, 431);
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.grid1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.textBox8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBox7);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBox6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox5);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.tabControl1);
            this.FitToMDI = true;
            this.HorizontalExpressionFactor = 1D;
            this.HorizontalScale = 1D;
            this.Name = "SqlReplaceManagerView";
            this.Text = "SqlReplaceManager";
            this.VerticalExpressionFactor = 1D;
            this.VerticalScale = 1D;
            this.contextMenuStrip1.ResumeLayout(false);
            this.grid1.ResumeLayout(false);
            this.gcId.ResumeLayout(false);
            this.gcLastRun.ResumeLayout(false);
            this.gcOriginalSQLTemplateKey.ResumeLayout(false);
            this.gcComments.ResumeLayout(false);
            this.gcError.ResumeLayout(false);
            this.gcChanged.ResumeLayout(false);
            this.gcFixed1.ResumeLayout(false);
            this.gcFixed2.ResumeLayout(false);
            this.gcRefresh.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private UI.Grid grid1;
        private UI.GridColumn gcId;
        private UI.TextBox txtId;
        private UI.GridColumn gcLastRun;
        private UI.TextBox txtLastRun;
        private UI.GridColumn gcOriginalSQLTemplateKey;
        private UI.TextBox txtOriginalSQLTemplateKey;
        private UI.GridColumn gcComments;
        private UI.TextBox txtComments;
        private UI.GridColumn gcFixed1;
        private UI.TextBox txtFixed1;
        private UI.GridColumn gcFixed2;
        private UI.TextBox txtFixed2;
        private UI.TabControl tabControl1;
        private UI.GridColumn gcError;
        private UI.TextBox txtError;
        private UI.GridColumn gcChanged;
        private UI.TextBox txtChanged;
        private UI.GridColumn gcRefresh;
        private UI.CheckBox txtRefresh;
        private UI.TextBox textBox1;
        private UI.Label label1;
        private UI.TextBox textBox2;
        private UI.Label label2;
        private UI.Label label3;
        private UI.TextBox textBox3;
        private UI.Label label4;
        private UI.TextBox textBox4;
        private UI.Label label5;
        private UI.TextBox textBox5;
        private UI.Label label6;
        private UI.TextBox textBox6;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem showMergedSQLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runAndCompareSQLToolStripMenuItem;
        private UI.Label label7;
        private UI.TextBox textBox7;
        private UI.Label label8;
        private UI.TextBox textBox8;

    }
}