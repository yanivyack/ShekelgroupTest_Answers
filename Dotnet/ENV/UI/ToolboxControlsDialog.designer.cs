using System.Windows.Forms;

namespace ENV.UI
{
    partial class ToolboxControlsDialog
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
            this.tableLayoutMain = new System.Windows.Forms.TableLayoutPanel();
            this.treeView = new System.Windows.Forms.TreeView();
            this.selectedList = new System.Windows.Forms.ListView();
            this.tableLayoutMiddleButtons = new System.Windows.Forms.TableLayoutPanel();
            this.btnRight = new System.Windows.Forms.Button();
            this.btnLeft = new System.Windows.Forms.Button();
            this.tableLayoutBottomButtons = new System.Windows.Forms.TableLayoutPanel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAddToToolbox = new System.Windows.Forms.Button();
            this.btnAddToForm = new System.Windows.Forms.Button();
            this.tableLayoutMain.SuspendLayout();
            this.tableLayoutMiddleButtons.SuspendLayout();
            this.tableLayoutBottomButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutMain
            // 
            this.tableLayoutMain.ColumnCount = 3;
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutMain.Controls.Add(this.treeView, 0, 0);
            this.tableLayoutMain.Controls.Add(this.selectedList, 2, 0);
            this.tableLayoutMain.Controls.Add(this.tableLayoutMiddleButtons, 1, 0);
            this.tableLayoutMain.Controls.Add(this.tableLayoutBottomButtons, 1, 1);
            this.tableLayoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutMain.Name = "tableLayoutMain";
            this.tableLayoutMain.RowCount = 2;
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutMain.Size = new System.Drawing.Size(501, 343);
            this.tableLayoutMain.TabIndex = 0;
            // 
            // treeView
            // 
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.Location = new System.Drawing.Point(3, 3);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(214, 302);
            this.treeView.TabIndex = 0;
            this.treeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tree_BeforeExpand);
            this.treeView.DoubleClick += new System.EventHandler(this.tree_DoubleClick);
            this.treeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tree_KeyDown);
            // 
            // selectedList
            // 
            this.selectedList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectedList.Location = new System.Drawing.Point(283, 3);
            this.selectedList.Name = "selectedList";
            this.selectedList.Size = new System.Drawing.Size(215, 302);
            this.selectedList.TabIndex = 1;
            this.selectedList.UseCompatibleStateImageBehavior = false;
            this.selectedList.View = System.Windows.Forms.View.List;
            this.selectedList.SelectedIndexChanged += new System.EventHandler(this.selectedList_SelectedIndexChanged);
            // 
            // tableLayoutMiddleButtons
            // 
            this.tableLayoutMiddleButtons.ColumnCount = 1;
            this.tableLayoutMiddleButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMiddleButtons.Controls.Add(this.btnRight, 0, 1);
            this.tableLayoutMiddleButtons.Controls.Add(this.btnLeft, 0, 2);
            this.tableLayoutMiddleButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutMiddleButtons.Location = new System.Drawing.Point(223, 3);
            this.tableLayoutMiddleButtons.Name = "tableLayoutMiddleButtons";
            this.tableLayoutMiddleButtons.RowCount = 4;
            this.tableLayoutMiddleButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutMiddleButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tableLayoutMiddleButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tableLayoutMiddleButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutMiddleButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutMiddleButtons.Size = new System.Drawing.Size(54, 302);
            this.tableLayoutMiddleButtons.TabIndex = 2;
            // 
            // btnRight
            // 
            this.btnRight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRight.Location = new System.Drawing.Point(3, 125);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(48, 23);
            this.btnRight.TabIndex = 1;
            this.btnRight.Text = ">";
            this.btnRight.UseVisualStyleBackColor = true;
            this.btnRight.Click += new System.EventHandler(this.btnRight_Click);
            // 
            // btnLeft
            // 
            this.btnLeft.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLeft.Enabled = false;
            this.btnLeft.Location = new System.Drawing.Point(3, 158);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(48, 23);
            this.btnLeft.TabIndex = 0;
            this.btnLeft.Text = "<";
            this.btnLeft.UseVisualStyleBackColor = true;
            this.btnLeft.Click += new System.EventHandler(this.btnLeft_Click);
            // 
            // tableLayoutBottomButtons
            // 
            this.tableLayoutBottomButtons.ColumnCount = 3;
            this.tableLayoutMain.SetColumnSpan(this.tableLayoutBottomButtons, 2);
            this.tableLayoutBottomButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutBottomButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutBottomButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutBottomButtons.Controls.Add(this.btnAddToForm, 0, 0);
            this.tableLayoutBottomButtons.Controls.Add(this.btnCancel, 2, 0);
            this.tableLayoutBottomButtons.Controls.Add(this.btnAddToToolbox, 1, 0);
            this.tableLayoutBottomButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutBottomButtons.Location = new System.Drawing.Point(223, 311);
            this.tableLayoutBottomButtons.Name = "tableLayoutBottomButtons";
            this.tableLayoutBottomButtons.RowCount = 1;
            this.tableLayoutBottomButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutBottomButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutBottomButtons.Size = new System.Drawing.Size(275, 29);
            this.tableLayoutBottomButtons.TabIndex = 3;
            // 
            // btnCancel
            // 
            this.btnCancel.AutoSize = true;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Location = new System.Drawing.Point(222, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(50, 23);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnAddToToolbox
            // 
            this.btnAddToToolbox.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnAddToToolbox.Enabled = false;
            this.btnAddToToolbox.Location = new System.Drawing.Point(117, 3);
            this.btnAddToToolbox.Name = "btnAddToToolbox";
            this.btnAddToToolbox.Size = new System.Drawing.Size(94, 23);
            this.btnAddToToolbox.TabIndex = 1;
            this.btnAddToToolbox.Text = "Add to Toolbox";
            this.btnAddToToolbox.UseVisualStyleBackColor = true;
            this.btnAddToToolbox.Click += new System.EventHandler(this.btnAddToToolbox_Click);
            // 
            // button1
            // 
            this.btnAddToForm.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnAddToForm.Enabled = false;
            this.btnAddToForm.Location = new System.Drawing.Point(3, 3);
            this.btnAddToForm.Name = "button1";
            this.btnAddToForm.Size = new System.Drawing.Size(101, 23);
            this.btnAddToForm.TabIndex = 2;
            this.btnAddToForm.Text = "Add to Form";
            this.btnAddToForm.UseVisualStyleBackColor = true;
            this.btnAddToForm.Click += new System.EventHandler(this.Button1_Click);
            // 
            // ToolboxControlsDialog
            // 
            this.AcceptButton = this.btnAddToToolbox;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(501, 343);
            this.Controls.Add(this.tableLayoutMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(517, 377);
            this.Name = "ToolboxControlsDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Add Controls to Toolbox";
            this.tableLayoutMain.ResumeLayout(false);
            this.tableLayoutMiddleButtons.ResumeLayout(false);
            this.tableLayoutBottomButtons.ResumeLayout(false);
            this.tableLayoutBottomButtons.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private TableLayoutPanel tableLayoutMain;
        private System.Windows.Forms.TreeView treeView;
        private ListView selectedList;
        private TableLayoutPanel tableLayoutMiddleButtons;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.Button btnRight;
        private TableLayoutPanel tableLayoutBottomButtons;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnAddToToolbox;
        private System.Windows.Forms.Button btnAddToForm;
    }
}