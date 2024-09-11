namespace Northwind.Views.Views
{
    partial class GridView
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
            this.grid1 = new Northwind.Shared.Theme.Controls.Grid();
            this.gcProductID = new Northwind.Shared.Theme.Controls.GridColumn();
            this.txtProductID = new Northwind.Shared.Theme.Controls.TextBox();
            this.gcProductName = new Northwind.Shared.Theme.Controls.GridColumn();
            this.txtProductName = new Northwind.Shared.Theme.Controls.TextBox();
            this.gcQuantityPerUnit = new Northwind.Shared.Theme.Controls.GridColumn();
            this.txtQuantityPerUnit = new Northwind.Shared.Theme.Controls.TextBox();
            this.gcUnitPrice = new Northwind.Shared.Theme.Controls.GridColumn();
            this.txtUnitPrice = new Northwind.Shared.Theme.Controls.TextBox();
            this.gcUnitsInStock = new Northwind.Shared.Theme.Controls.GridColumn();
            this.txtUnitsInStock = new Northwind.Shared.Theme.Controls.TextBox();
            this.gcUnitsOnOrder = new Northwind.Shared.Theme.Controls.GridColumn();
            this.txtUnitsOnOrder = new Northwind.Shared.Theme.Controls.TextBox();
            this.gcDiscontinued = new Northwind.Shared.Theme.Controls.GridColumn();
            this.txtDiscontinued = new Northwind.Shared.Theme.Controls.TextBox();
            this.grid1.SuspendLayout();
            this.gcProductID.SuspendLayout();
            this.gcProductName.SuspendLayout();
            this.gcQuantityPerUnit.SuspendLayout();
            this.gcUnitPrice.SuspendLayout();
            this.gcUnitsInStock.SuspendLayout();
            this.gcUnitsOnOrder.SuspendLayout();
            this.gcDiscontinued.SuspendLayout();
            this.SuspendLayout();
            // 
            // grid1
            // 
            this.grid1.Controls.Add(this.gcProductID);
            this.grid1.Controls.Add(this.gcProductName);
            this.grid1.Controls.Add(this.gcQuantityPerUnit);
            this.grid1.Controls.Add(this.gcUnitPrice);
            this.grid1.Controls.Add(this.gcUnitsInStock);
            this.grid1.Controls.Add(this.gcUnitsOnOrder);
            this.grid1.Controls.Add(this.gcDiscontinued);
            this.grid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid1.Location = new System.Drawing.Point(0, 0);
            this.grid1.Name = "grid1";
            this.grid1.Size = new System.Drawing.Size(1091, 431);
            this.grid1.Text = "grid1";
            // 
            // gcProductID
            // 
            this.gcProductID.Controls.Add(this.txtProductID);
            this.gcProductID.Name = "gcProductID";
            this.gcProductID.Text = "ProductID";
            this.gcProductID.Width = 131;
            // 
            // txtProductID
            // 
            this.txtProductID.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 100);
            this.txtProductID.Location = new System.Drawing.Point(2, 1);
            this.txtProductID.Name = "txtProductID";
            this.txtProductID.Size = new System.Drawing.Size(125, 21);
            this.txtProductID.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtProductID.Data = this._controller.products.ProductID;
            // 
            // gcProductName
            // 
            this.gcProductName.Controls.Add(this.txtProductName);
            this.gcProductName.Name = "gcProductName";
            this.gcProductName.Text = "ProductName";
            this.gcProductName.Width = 285;
            // 
            // txtProductName
            // 
            this.txtProductName.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 100);
            this.txtProductName.Location = new System.Drawing.Point(2, 1);
            this.txtProductName.Name = "txtProductName";
            this.txtProductName.Size = new System.Drawing.Size(279, 21);
            this.txtProductName.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtProductName.Data = this._controller.products.ProductName;
            // 
            // gcQuantityPerUnit
            // 
            this.gcQuantityPerUnit.Controls.Add(this.txtQuantityPerUnit);
            this.gcQuantityPerUnit.Name = "gcQuantityPerUnit";
            this.gcQuantityPerUnit.Text = "QuantityPerUnit";
            this.gcQuantityPerUnit.Width = 230;
            // 
            // txtQuantityPerUnit
            // 
            this.txtQuantityPerUnit.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 100);
            this.txtQuantityPerUnit.Location = new System.Drawing.Point(2, 1);
            this.txtQuantityPerUnit.Name = "txtQuantityPerUnit";
            this.txtQuantityPerUnit.Size = new System.Drawing.Size(224, 21);
            this.txtQuantityPerUnit.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtQuantityPerUnit.Data = this._controller.products.QuantityPerUnit;
            // 
            // gcUnitPrice
            // 
            this.gcUnitPrice.Controls.Add(this.txtUnitPrice);
            this.gcUnitPrice.Name = "gcUnitPrice";
            this.gcUnitPrice.Text = "UnitPrice";
            this.gcUnitPrice.Width = 164;
            // 
            // txtUnitPrice
            // 
            this.txtUnitPrice.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 100);
            this.txtUnitPrice.Location = new System.Drawing.Point(2, 1);
            this.txtUnitPrice.Name = "txtUnitPrice";
            this.txtUnitPrice.Size = new System.Drawing.Size(158, 21);
            this.txtUnitPrice.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtUnitPrice.Data = this._controller.products.UnitPrice;
            // 
            // gcUnitsInStock
            // 
            this.gcUnitsInStock.Controls.Add(this.txtUnitsInStock);
            this.gcUnitsInStock.Name = "gcUnitsInStock";
            this.gcUnitsInStock.Text = "UnitsInStock";
            this.gcUnitsInStock.Width = 82;
            // 
            // txtUnitsInStock
            // 
            this.txtUnitsInStock.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 100);
            this.txtUnitsInStock.Location = new System.Drawing.Point(2, 1);
            this.txtUnitsInStock.Name = "txtUnitsInStock";
            this.txtUnitsInStock.Size = new System.Drawing.Size(70, 21);
            this.txtUnitsInStock.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtUnitsInStock.Data = this._controller.products.UnitsInStock;
            // 
            // gcUnitsOnOrder
            // 
            this.gcUnitsOnOrder.Controls.Add(this.txtUnitsOnOrder);
            this.gcUnitsOnOrder.Name = "gcUnitsOnOrder";
            this.gcUnitsOnOrder.Text = "UnitsOnOrder";
            this.gcUnitsOnOrder.Width = 89;
            // 
            // txtUnitsOnOrder
            // 
            this.txtUnitsOnOrder.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 100);
            this.txtUnitsOnOrder.Location = new System.Drawing.Point(2, 1);
            this.txtUnitsOnOrder.Name = "txtUnitsOnOrder";
            this.txtUnitsOnOrder.Size = new System.Drawing.Size(70, 21);
            this.txtUnitsOnOrder.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtUnitsOnOrder.Data = this._controller.products.UnitsOnOrder;
            // 
            // gcDiscontinued
            // 
            this.gcDiscontinued.Controls.Add(this.txtDiscontinued);
            this.gcDiscontinued.Name = "gcDiscontinued";
            this.gcDiscontinued.Text = "Discontinued";
            this.gcDiscontinued.Width = 86;
            // 
            // txtDiscontinued
            // 
            this.txtDiscontinued.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 100);
            this.txtDiscontinued.Location = new System.Drawing.Point(2, 1);
            this.txtDiscontinued.Name = "txtDiscontinued";
            this.txtDiscontinued.Size = new System.Drawing.Size(59, 21);
            this.txtDiscontinued.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtDiscontinued.Data = this._controller.products.Discontinued;
            // 
            // GridView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1091, 431);
            this.Controls.Add(this.grid1);
            this.HorizontalExpressionFactor = 1D;
            this.Name = "GridView";
            this.Text = "Grid";
            this.VerticalExpressionFactor = 1D;
            this.grid1.ResumeLayout(false);
            this.gcProductID.ResumeLayout(false);
            this.gcProductName.ResumeLayout(false);
            this.gcQuantityPerUnit.ResumeLayout(false);
            this.gcUnitPrice.ResumeLayout(false);
            this.gcUnitsInStock.ResumeLayout(false);
            this.gcUnitsOnOrder.ResumeLayout(false);
            this.gcDiscontinued.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Shared.Theme.Controls.Grid grid1;
        private Shared.Theme.Controls.GridColumn gcProductID;
        private Shared.Theme.Controls.TextBox txtProductID;
        private Shared.Theme.Controls.GridColumn gcProductName;
        private Shared.Theme.Controls.TextBox txtProductName;
        private Shared.Theme.Controls.GridColumn gcQuantityPerUnit;
        private Shared.Theme.Controls.TextBox txtQuantityPerUnit;
        private Shared.Theme.Controls.GridColumn gcUnitPrice;
        private Shared.Theme.Controls.TextBox txtUnitPrice;
        private Shared.Theme.Controls.GridColumn gcUnitsInStock;
        private Shared.Theme.Controls.TextBox txtUnitsInStock;
        private Shared.Theme.Controls.GridColumn gcUnitsOnOrder;
        private Shared.Theme.Controls.TextBox txtUnitsOnOrder;
        private Shared.Theme.Controls.GridColumn gcDiscontinued;
        private Shared.Theme.Controls.TextBox txtDiscontinued;
    }
}