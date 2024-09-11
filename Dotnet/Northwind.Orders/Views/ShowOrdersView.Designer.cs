using Firefly.Box.UI;
using System.Drawing;
using Firefly.Box.UI.Advanced;
using Firefly.Box;
using Northwind.Shared.Theme;
namespace Northwind.Orders.Views
{
    partial class ShowOrdersView
    {
        System.ComponentModel.IContainer components;
        Shared.Theme.Colors.DefaultHelpWindow clrDefaultHelpWindow;
        Shared.Theme.Controls.CompatibleLabel lblOrder;
        Shared.Theme.Controls.CompatibleTextBox txtOrders_OrderID;
        Shared.Theme.Controls.CompatibleLabel lblCustomer;
        Shared.Theme.Controls.GroupBox grp;
        Shared.Theme.Controls.CompatibleLabel lblOrderDate;
        Shared.Theme.Controls.CompatibleTextBox txtOrders_OrderDate;
        Shared.Theme.Controls.CompatibleLabel lblShipAddress;
        Shared.Theme.Controls.CompatibleTextBox txtOrders_ShipAddress;
        Shared.Theme.Controls.CompatibleLabel lblRequiredDate;
        Shared.Theme.Controls.CompatibleTextBox txtOrders_RequiredDate;
        Shared.Theme.Controls.CompatibleLabel lblShipCity;
        Shared.Theme.Controls.CompatibleTextBox txtOrders_ShipCity;
        Shared.Theme.Controls.CompatibleLabel lblShippedDate;
        Shared.Theme.Controls.CompatibleTextBox txtOrders_ShippedDate;
        Shared.Theme.Controls.CompatibleLabel lblShipCountry;
        Shared.Theme.Controls.CompatibleTextBox txtOrders_ShipCountry;
        Shared.Theme.Controls.GroupBox grp_;
        Shared.Theme.Controls.CompatibleTextBox txtV_Total;
        Shared.Theme.Controls.CompatibleLabel lblTotal;
        Shared.Theme.Controls.Button btn;
        Shared.Theme.Controls.ComboBox cboOrders_CustomerID;
        Shared.Theme.Controls.Button btnPrint;
        Northwind.Views.Controls.Close btn_;
        Shared.Theme.Fonts.DefaultDialogText fntDefaultDialogText;
        Shared.Theme.Fonts.DefaultTableEditField fntDefaultTableEditField;
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
            this.fntDefaultDialogText = new Northwind.Shared.Theme.Fonts.DefaultDialogText();
            this.fntDefaultTableEditField = new Northwind.Shared.Theme.Fonts.DefaultTableEditField();
            this.lblOrder = new Northwind.Shared.Theme.Controls.CompatibleLabel();
            this.txtOrders_OrderID = new Northwind.Shared.Theme.Controls.CompatibleTextBox();
            this.lblCustomer = new Northwind.Shared.Theme.Controls.CompatibleLabel();
            this.grp = new Northwind.Shared.Theme.Controls.GroupBox();
            this.lblOrderDate = new Northwind.Shared.Theme.Controls.CompatibleLabel();
            this.txtOrders_OrderDate = new Northwind.Shared.Theme.Controls.CompatibleTextBox();
            this.lblShipAddress = new Northwind.Shared.Theme.Controls.CompatibleLabel();
            this.txtOrders_ShipAddress = new Northwind.Shared.Theme.Controls.CompatibleTextBox();
            this.lblRequiredDate = new Northwind.Shared.Theme.Controls.CompatibleLabel();
            this.txtOrders_RequiredDate = new Northwind.Shared.Theme.Controls.CompatibleTextBox();
            this.lblShipCity = new Northwind.Shared.Theme.Controls.CompatibleLabel();
            this.txtOrders_ShipCity = new Northwind.Shared.Theme.Controls.CompatibleTextBox();
            this.lblShippedDate = new Northwind.Shared.Theme.Controls.CompatibleLabel();
            this.txtOrders_ShippedDate = new Northwind.Shared.Theme.Controls.CompatibleTextBox();
            this.lblShipCountry = new Northwind.Shared.Theme.Controls.CompatibleLabel();
            this.txtOrders_ShipCountry = new Northwind.Shared.Theme.Controls.CompatibleTextBox();
            this.grp_ = new Northwind.Shared.Theme.Controls.GroupBox();
            this.txtV_Total = new Northwind.Shared.Theme.Controls.CompatibleTextBox();
            this.lblTotal = new Northwind.Shared.Theme.Controls.CompatibleLabel();
            this.cboOrders_CustomerID = new Northwind.Shared.Theme.Controls.ComboBox();
            this.btn = new Northwind.Shared.Theme.Controls.Button();
            this.btnPrint = new Northwind.Shared.Theme.Controls.Button();
            this.btn_ = new Northwind.Views.Controls.Close();
            this.SuspendLayout();
            // 
            // lblOrder
            // 
            this.lblOrder.ColorScheme = this.clrDefaultHelpWindow;
            this.lblOrder.FontScheme = this.fntDefaultDialogText;
            this.lblOrder.Location = new System.Drawing.Point(48, 8);
            this.lblOrder.Name = "lblOrder";
            this.lblOrder.Rtf = "Order:";
            this.lblOrder.Size = new System.Drawing.Size(40, 20);
            this.lblOrder.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.lblOrder.Text = "Order:";
            // 
            // txtOrders_OrderID
            // 
            this.txtOrders_OrderID.Alignment = System.Drawing.ContentAlignment.MiddleRight;
            this.txtOrders_OrderID.AllowFocus = false;
            this.txtOrders_OrderID.Location = new System.Drawing.Point(88, 8);
            this.txtOrders_OrderID.Name = "txtOrders_OrderID";
            this.txtOrders_OrderID.Size = new System.Drawing.Size(70, 20);
            this.txtOrders_OrderID.Tag = "OrderID";
            this.txtOrders_OrderID.Data = this._controller.Orders.OrderID;
            // 
            // lblCustomer
            // 
            this.lblCustomer.ColorScheme = this.clrDefaultHelpWindow;
            this.lblCustomer.FontScheme = this.fntDefaultDialogText;
            this.lblCustomer.Location = new System.Drawing.Point(188, 8);
            this.lblCustomer.Name = "lblCustomer";
            this.lblCustomer.Rtf = "Customer:";
            this.lblCustomer.Size = new System.Drawing.Size(57, 20);
            this.lblCustomer.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.lblCustomer.Text = "Customer:";
            // 
            // grp
            // 
            this.grp.ColorScheme = this.clrDefaultHelpWindow;
            this.grp.ForceTransparentBackgroundOnStandardStyle = true;
            this.grp.Location = new System.Drawing.Point(45, 46);
            this.grp.Name = "grp";
            this.grp.Size = new System.Drawing.Size(424, 100);
            this.grp.Style = Firefly.Box.UI.ControlStyle.Standard;
            this.grp.Text = "";
            // 
            // lblOrderDate
            // 
            this.lblOrderDate.BoundTo = new Firefly.Box.UI.ControlBinding(this.grp);
            this.lblOrderDate.ColorScheme = this.clrDefaultHelpWindow;
            this.lblOrderDate.FontScheme = this.fntDefaultDialogText;
            this.lblOrderDate.Location = new System.Drawing.Point(59, 68);
            this.lblOrderDate.Name = "lblOrderDate";
            this.lblOrderDate.Rtf = "Order Date:";
            this.lblOrderDate.Size = new System.Drawing.Size(52, 13);
            this.lblOrderDate.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.lblOrderDate.Text = "Order Date:";
            // 
            // txtOrders_OrderDate
            // 
            this.txtOrders_OrderDate.BoundTo = new Firefly.Box.UI.ControlBinding(this.grp);
            this.txtOrders_OrderDate.Location = new System.Drawing.Point(133, 68);
            this.txtOrders_OrderDate.Name = "txtOrders_OrderDate";
            this.txtOrders_OrderDate.Size = new System.Drawing.Size(76, 17);
            this.txtOrders_OrderDate.Tag = "OrderDate";
            this.txtOrders_OrderDate.Data = this._controller.Orders.OrderDate;
            // 
            // lblShipAddress
            // 
            this.lblShipAddress.BoundTo = new Firefly.Box.UI.ControlBinding(this.grp);
            this.lblShipAddress.ColorScheme = this.clrDefaultHelpWindow;
            this.lblShipAddress.FontScheme = this.fntDefaultDialogText;
            this.lblShipAddress.Location = new System.Drawing.Point(251, 68);
            this.lblShipAddress.Name = "lblShipAddress";
            this.lblShipAddress.Rtf = "Ship Address:";
            this.lblShipAddress.Size = new System.Drawing.Size(63, 13);
            this.lblShipAddress.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.lblShipAddress.Text = "Ship Address:";
            // 
            // txtOrders_ShipAddress
            // 
            this.txtOrders_ShipAddress.BoundTo = new Firefly.Box.UI.ControlBinding(this.grp);
            this.txtOrders_ShipAddress.Location = new System.Drawing.Point(325, 68);
            this.txtOrders_ShipAddress.Name = "txtOrders_ShipAddress";
            this.txtOrders_ShipAddress.Size = new System.Drawing.Size(109, 17);
            this.txtOrders_ShipAddress.Tag = "ShipAddress";
            this.txtOrders_ShipAddress.Data = this._controller.Orders.ShipAddress;
            // 
            // lblRequiredDate
            // 
            this.lblRequiredDate.BoundTo = new Firefly.Box.UI.ControlBinding(this.grp);
            this.lblRequiredDate.ColorScheme = this.clrDefaultHelpWindow;
            this.lblRequiredDate.FontScheme = this.fntDefaultDialogText;
            this.lblRequiredDate.Location = new System.Drawing.Point(59, 91);
            this.lblRequiredDate.Name = "lblRequiredDate";
            this.lblRequiredDate.Rtf = "Required Date:";
            this.lblRequiredDate.Size = new System.Drawing.Size(69, 13);
            this.lblRequiredDate.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.lblRequiredDate.Text = "Required Date:";
            // 
            // txtOrders_RequiredDate
            // 
            this.txtOrders_RequiredDate.BoundTo = new Firefly.Box.UI.ControlBinding(this.grp);
            this.txtOrders_RequiredDate.Location = new System.Drawing.Point(133, 91);
            this.txtOrders_RequiredDate.Name = "txtOrders_RequiredDate";
            this.txtOrders_RequiredDate.Size = new System.Drawing.Size(76, 16);
            this.txtOrders_RequiredDate.Tag = "RequiredDate";
            this.txtOrders_RequiredDate.Data = this._controller.Orders.RequiredDate;
            // 
            // lblShipCity
            // 
            this.lblShipCity.BoundTo = new Firefly.Box.UI.ControlBinding(this.grp);
            this.lblShipCity.ColorScheme = this.clrDefaultHelpWindow;
            this.lblShipCity.FontScheme = this.fntDefaultDialogText;
            this.lblShipCity.Location = new System.Drawing.Point(251, 91);
            this.lblShipCity.Name = "lblShipCity";
            this.lblShipCity.Rtf = "Ship City:";
            this.lblShipCity.Size = new System.Drawing.Size(42, 13);
            this.lblShipCity.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.lblShipCity.Text = "Ship City:";
            // 
            // txtOrders_ShipCity
            // 
            this.txtOrders_ShipCity.BoundTo = new Firefly.Box.UI.ControlBinding(this.grp);
            this.txtOrders_ShipCity.Location = new System.Drawing.Point(325, 91);
            this.txtOrders_ShipCity.Name = "txtOrders_ShipCity";
            this.txtOrders_ShipCity.Size = new System.Drawing.Size(109, 16);
            this.txtOrders_ShipCity.Tag = "ShipCity";
            this.txtOrders_ShipCity.Data = this._controller.Orders.ShipCity;
            // 
            // lblShippedDate
            // 
            this.lblShippedDate.BoundTo = new Firefly.Box.UI.ControlBinding(this.grp);
            this.lblShippedDate.ColorScheme = this.clrDefaultHelpWindow;
            this.lblShippedDate.FontScheme = this.fntDefaultDialogText;
            this.lblShippedDate.Location = new System.Drawing.Point(59, 114);
            this.lblShippedDate.Name = "lblShippedDate";
            this.lblShippedDate.Rtf = "Shipped Date:";
            this.lblShippedDate.Size = new System.Drawing.Size(65, 13);
            this.lblShippedDate.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.lblShippedDate.Text = "Shipped Date:";
            // 
            // txtOrders_ShippedDate
            // 
            this.txtOrders_ShippedDate.BoundTo = new Firefly.Box.UI.ControlBinding(this.grp);
            this.txtOrders_ShippedDate.Location = new System.Drawing.Point(133, 114);
            this.txtOrders_ShippedDate.Name = "txtOrders_ShippedDate";
            this.txtOrders_ShippedDate.Size = new System.Drawing.Size(76, 16);
            this.txtOrders_ShippedDate.Tag = "ShippedDate";
            this.txtOrders_ShippedDate.Data = this._controller.Orders.ShippedDate;
            // 
            // lblShipCountry
            // 
            this.lblShipCountry.BoundTo = new Firefly.Box.UI.ControlBinding(this.grp);
            this.lblShipCountry.ColorScheme = this.clrDefaultHelpWindow;
            this.lblShipCountry.FontScheme = this.fntDefaultDialogText;
            this.lblShipCountry.Location = new System.Drawing.Point(251, 114);
            this.lblShipCountry.Name = "lblShipCountry";
            this.lblShipCountry.Rtf = "Ship Country:";
            this.lblShipCountry.Size = new System.Drawing.Size(60, 13);
            this.lblShipCountry.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.lblShipCountry.Text = "Ship Country:";
            // 
            // txtOrders_ShipCountry
            // 
            this.txtOrders_ShipCountry.BoundTo = new Firefly.Box.UI.ControlBinding(this.grp);
            this.txtOrders_ShipCountry.Location = new System.Drawing.Point(325, 114);
            this.txtOrders_ShipCountry.Name = "txtOrders_ShipCountry";
            this.txtOrders_ShipCountry.Size = new System.Drawing.Size(109, 16);
            this.txtOrders_ShipCountry.Tag = "ShipCountry";
            this.txtOrders_ShipCountry.Data = this._controller.Orders.ShipCountry;
            // 
            // grp_
            // 
            this.grp_.ColorScheme = this.clrDefaultHelpWindow;
            this.grp_.ForceTransparentBackgroundOnStandardStyle = true;
            this.grp_.Location = new System.Drawing.Point(303, 320);
            this.grp_.Name = "grp_";
            this.grp_.Size = new System.Drawing.Size(173, 39);
            this.grp_.Style = Firefly.Box.UI.ControlStyle.Standard;
            this.grp_.Text = "";
            // 
            // txtV_Total
            // 
            this.txtV_Total.Alignment = System.Drawing.ContentAlignment.MiddleRight;
            this.txtV_Total.BoundTo = new Firefly.Box.UI.ControlBinding(this.grp_);
            this.txtV_Total.Location = new System.Drawing.Point(368, 332);
            this.txtV_Total.Name = "txtV_Total";
            this.txtV_Total.ReadOnly = true;
            this.txtV_Total.Size = new System.Drawing.Size(82, 16);
            this.txtV_Total.Tag = "v.Total";
            this.txtV_Total.Data = this._controller.v_Total;
            // 
            // lblTotal
            // 
            this.lblTotal.BoundTo = new Firefly.Box.UI.ControlBinding(this.grp_);
            this.lblTotal.ColorScheme = this.clrDefaultHelpWindow;
            this.lblTotal.FontScheme = this.fntDefaultTableEditField;
            this.lblTotal.Location = new System.Drawing.Point(316, 333);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Rtf = "Total";
            this.lblTotal.Size = new System.Drawing.Size(43, 16);
            this.lblTotal.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.lblTotal.Text = "Total";
            // 
            // cboOrders_CustomerID
            // 
            this.cboOrders_CustomerID.IgnoreEmptyBoundValues = true;
            this.cboOrders_CustomerID.Location = new System.Drawing.Point(248, 8);
            this.cboOrders_CustomerID.Name = "cboOrders_CustomerID";
            this.cboOrders_CustomerID.Size = new System.Drawing.Size(198, 20);
            this.cboOrders_CustomerID.Tag = "CustomerID";
            this.cboOrders_CustomerID.BindListSource += new Firefly.Box.UI.Advanced.BindingEventHandler<System.EventArgs>(this.cboOrders_CustomerID_BindListSource);
            this.cboOrders_CustomerID.Data = this._controller.Orders.CustomerID;
            // 
            // btn
            // 
            this.btn.Format = "...";
            this.btn.Location = new System.Drawing.Point(449, 7);
            this.btn.Name = "btn";
            this.btn.RaiseClickBeforeFocusChange = true;
            this.btn.Size = new System.Drawing.Size(21, 17);
            this.btn.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.btn_Click);
            // 
            // btnPrint
            // 
            this.btnPrint.Format = "Print";
            this.btnPrint.Location = new System.Drawing.Point(338, 380);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.RaiseClickBeforeFocusChange = true;
            this.btnPrint.Size = new System.Drawing.Size(68, 23);
            this.btnPrint.Click += new Firefly.Box.UI.Advanced.ButtonClickEventHandler(this.btnPrint_Click);
            // 
            // btn_
            // 
            this.btn_.Location = new System.Drawing.Point(410, 380);
            this.btn_.Name = "btn_";
            this.btn_.RaiseClickBeforeFocusChange = true;
            this.btn_.Size = new System.Drawing.Size(69, 23);
            // 
            // ShowOrdersView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(5F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(511, 411);
            this.ColorScheme = this.clrDefaultHelpWindow;
            this.Controls.Add(this.btn_);
            this.Controls.Add(this.btnPrint);
            this.Controls.Add(this.btn);
            this.Controls.Add(this.cboOrders_CustomerID);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.txtV_Total);
            this.Controls.Add(this.grp_);
            this.Controls.Add(this.txtOrders_ShipCountry);
            this.Controls.Add(this.lblShipCountry);
            this.Controls.Add(this.txtOrders_ShippedDate);
            this.Controls.Add(this.lblShippedDate);
            this.Controls.Add(this.txtOrders_ShipCity);
            this.Controls.Add(this.lblShipCity);
            this.Controls.Add(this.txtOrders_RequiredDate);
            this.Controls.Add(this.lblRequiredDate);
            this.Controls.Add(this.txtOrders_ShipAddress);
            this.Controls.Add(this.lblShipAddress);
            this.Controls.Add(this.txtOrders_OrderDate);
            this.Controls.Add(this.lblOrderDate);
            this.Controls.Add(this.grp);
            this.Controls.Add(this.lblCustomer);
            this.Controls.Add(this.txtOrders_OrderID);
            this.Controls.Add(this.lblOrder);
            this.HorizontalExpressionFactor = 4D;
            this.HorizontalScale = 5D;
            this.Name = "ShowOrdersView";
            this.StartPosition = Firefly.Box.UI.WindowStartPosition.CenterMDI;
            this.Text = "Show Orders";
            this.VerticalExpressionFactor = 8D;
            this.VerticalScale = 13D;
            this.ResumeLayout(false);

        }
    }
}
