using Firefly.Box.UI;
using System.Drawing;
using Firefly.Box.UI.Advanced;
using Firefly.Box;
using Northwind.Shared.Theme;
namespace Northwind.Orders.Views
{
    partial class ShowOrdersDetails
    {
        System.ComponentModel.IContainer components;
        Shared.Theme.Colors.DefaultHelpWindow clrDefaultHelpWindow;
        Northwind.Views.Controls.V9CompatibleDefaultTable grd;
        Shared.Theme.Controls.CompatibleGridColumn gclProduct;
        Shared.Theme.Controls.CompatibleGridColumn gclUnitPrice;
        Shared.Theme.Controls.CompatibleGridColumn gclQuantity;
        Shared.Theme.Controls.CompatibleGridColumn gclDiscount;
        Shared.Theme.Controls.CompatibleGridColumn gclTotal;
        Shared.Theme.Controls.CompatibleTextBox txtOrder_Details_UnitPrice;
        Shared.Theme.Controls.CompatibleTextBox txtOrder_Details_Quantity;
        Shared.Theme.Controls.CompatibleTextBox txtOrder_Details_Discount;
        Shared.Theme.Controls.CompatibleTextBox txtExp_3;
        Shared.Theme.Controls.ComboBox cboOrder_Details_ProductID;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.clrDefaultHelpWindow = new Northwind.Shared.Theme.Colors.DefaultHelpWindow();
            this.grd = new Northwind.Views.Controls.V9CompatibleDefaultTable();
            this.gclProduct = new Northwind.Shared.Theme.Controls.CompatibleGridColumn();
            this.cboOrder_Details_ProductID = new Northwind.Shared.Theme.Controls.ComboBox();
            this.gclUnitPrice = new Northwind.Shared.Theme.Controls.CompatibleGridColumn();
            this.txtOrder_Details_UnitPrice = new Northwind.Shared.Theme.Controls.CompatibleTextBox();
            this.gclQuantity = new Northwind.Shared.Theme.Controls.CompatibleGridColumn();
            this.txtOrder_Details_Quantity = new Northwind.Shared.Theme.Controls.CompatibleTextBox();
            this.gclDiscount = new Northwind.Shared.Theme.Controls.CompatibleGridColumn();
            this.txtOrder_Details_Discount = new Northwind.Shared.Theme.Controls.CompatibleTextBox();
            this.gclTotal = new Northwind.Shared.Theme.Controls.CompatibleGridColumn();
            this.txtExp_3 = new Northwind.Shared.Theme.Controls.CompatibleTextBox();
            TabOrderMode = Firefly.Box.UI.TabOrderMode.ManualIgnoringContainerHeirarchy;
            this.grd.SuspendLayout();
            this.gclProduct.SuspendLayout();
            this.gclUnitPrice.SuspendLayout();
            this.gclQuantity.SuspendLayout();
            this.gclDiscount.SuspendLayout();
            this.gclTotal.SuspendLayout();
            this.SuspendLayout();
            // 
            // grd
            // 
            this.grd.AllowMultiSelect = false;
            this.grd.Controls.Add(this.gclProduct);
            this.grd.Controls.Add(this.gclUnitPrice);
            this.grd.Controls.Add(this.gclQuantity);
            this.grd.Controls.Add(this.gclDiscount);
            this.grd.Controls.Add(this.gclTotal);
            this.grd.Location = new System.Drawing.Point(8, 8);
            this.grd.Name = "grd";
            this.grd.RowHeight = 23;
            this.grd.Size = new System.Drawing.Size(445, 143);
            // 
            // gclProduct
            // 
            this.gclProduct.Controls.Add(this.cboOrder_Details_ProductID);
            this.gclProduct.Name = "gclProduct";
            this.gclProduct.Text = "Product";
            this.gclProduct.Width = 212;
            // 
            // cboOrder_Details_ProductID
            // 
            this.cboOrder_Details_ProductID.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 0);
            this.cboOrder_Details_ProductID.IgnoreEmptyBoundValues = true;
            this.cboOrder_Details_ProductID.Location = new System.Drawing.Point(3, 1);
            this.cboOrder_Details_ProductID.Name = "cboOrder_Details_ProductID";
            this.cboOrder_Details_ProductID.Size = new System.Drawing.Size(202, 19);
            this.cboOrder_Details_ProductID.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.cboOrder_Details_ProductID.TabIndex = 1;
            this.cboOrder_Details_ProductID.Tag = "ProductID";
            this.cboOrder_Details_ProductID.BindListSource += new Firefly.Box.UI.Advanced.BindingEventHandler<System.EventArgs>(this.cboOrder_Details_ProductID_BindListSource);
            this.cboOrder_Details_ProductID.Data = this._controller.Order_Details.ProductID;
            // 
            // gclUnitPrice
            // 
            this.gclUnitPrice.Controls.Add(this.txtOrder_Details_UnitPrice);
            this.gclUnitPrice.Name = "gclUnitPrice";
            this.gclUnitPrice.Text = "Unit Price";
            this.gclUnitPrice.Width = 55;
            // 
            // txtOrder_Details_UnitPrice
            // 
            this.txtOrder_Details_UnitPrice.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 0);
            this.txtOrder_Details_UnitPrice.Alignment = System.Drawing.ContentAlignment.MiddleRight;
            this.txtOrder_Details_UnitPrice.Format = "5.2CZ +$;";
            this.txtOrder_Details_UnitPrice.Location = new System.Drawing.Point(2, 1);
            this.txtOrder_Details_UnitPrice.Name = "txtOrder_Details_UnitPrice";
            this.txtOrder_Details_UnitPrice.Size = new System.Drawing.Size(47, 16);
            this.txtOrder_Details_UnitPrice.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtOrder_Details_UnitPrice.TabIndex = 2;
            this.txtOrder_Details_UnitPrice.Tag = "UnitPrice";
            this.txtOrder_Details_UnitPrice.Data = this._controller.Order_Details.UnitPrice;
            // 
            // gclQuantity
            // 
            this.gclQuantity.Controls.Add(this.txtOrder_Details_Quantity);
            this.gclQuantity.Name = "gclQuantity";
            this.gclQuantity.Text = "Quantity";
            this.gclQuantity.Width = 49;
            // 
            // txtOrder_Details_Quantity
            // 
            this.txtOrder_Details_Quantity.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 0);
            this.txtOrder_Details_Quantity.Alignment = System.Drawing.ContentAlignment.MiddleRight;
            this.txtOrder_Details_Quantity.Location = new System.Drawing.Point(2, 1);
            this.txtOrder_Details_Quantity.Name = "txtOrder_Details_Quantity";
            this.txtOrder_Details_Quantity.Size = new System.Drawing.Size(40, 16);
            this.txtOrder_Details_Quantity.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtOrder_Details_Quantity.TabIndex = 3;
            this.txtOrder_Details_Quantity.Tag = "Quantity";
            this.txtOrder_Details_Quantity.Data = this._controller.Order_Details.Quantity;
            // 
            // gclDiscount
            // 
            this.gclDiscount.Controls.Add(this.txtOrder_Details_Discount);
            this.gclDiscount.Name = "gclDiscount";
            this.gclDiscount.Text = "Discount";
            this.gclDiscount.Width = 61;
            // 
            // txtOrder_Details_Discount
            // 
            this.txtOrder_Details_Discount.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 0);
            this.txtOrder_Details_Discount.Alignment = System.Drawing.ContentAlignment.MiddleRight;
            this.txtOrder_Details_Discount.Format = "5.2CZ +$;";
            this.txtOrder_Details_Discount.Location = new System.Drawing.Point(2, 1);
            this.txtOrder_Details_Discount.Name = "txtOrder_Details_Discount";
            this.txtOrder_Details_Discount.Size = new System.Drawing.Size(52, 16);
            this.txtOrder_Details_Discount.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtOrder_Details_Discount.TabIndex = 4;
            this.txtOrder_Details_Discount.Tag = "Discount";
            this.txtOrder_Details_Discount.Data = this._controller.Order_Details.Discount;
            // 
            // gclTotal
            // 
            this.gclTotal.Controls.Add(this.txtExp_3);
            this.gclTotal.Name = "gclTotal";
            this.gclTotal.Text = "Total";
            this.gclTotal.Width = 47;
            // 
            // txtExp_3
            // 
            this.txtExp_3.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 0);
            this.txtExp_3.Format = "5.2CZ +$;";
            this.txtExp_3.Location = new System.Drawing.Point(2, 1);
            this.txtExp_3.Name = "txtExp_3";
            this.txtExp_3.Size = new System.Drawing.Size(42, 16);
            this.txtExp_3.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtExp_3.TabIndex = 5;
            this.txtExp_3.Data = Firefly.Box.UI.Advanced.ControlData.FromNumber(_controller.Exp_3);
            // 
            // ShowOrdersDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(5F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Border = Firefly.Box.UI.ControlBorderStyle.Thin;
            this.ChildWindow = true;
            this.ClientSize = new System.Drawing.Size(467, 158);
            this.ColorScheme = this.clrDefaultHelpWindow;
            this.Controls.Add(this.grd);
            this.HorizontalExpressionFactor = 4D;
            this.HorizontalScale = 5D;
            this.Location = new System.Drawing.Point(26, 153);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ShowOrdersDetails";
            this.SystemMenu = false;
            this.Text = "Details";
            this.TitleBar = false;
            this.VerticalExpressionFactor = 8D;
            this.VerticalScale = 13D;
            this.grd.ResumeLayout(false);
            this.gclProduct.ResumeLayout(false);
            this.gclUnitPrice.ResumeLayout(false);
            this.gclQuantity.ResumeLayout(false);
            this.gclDiscount.ResumeLayout(false);
            this.gclTotal.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
