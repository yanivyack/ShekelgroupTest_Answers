using Firefly.Box.UI;
using System.Drawing;
using Firefly.Box.UI.Advanced;
using Firefly.Box;
using Northwind.Shared.Theme;
namespace Northwind.Orders.Printing
{
    partial class Print_OrdersC1
    {
        internal Shared.Theme.Printing.ReportSection Header;
        Shared.Theme.Colors.DefaultEditField clrDefaultEditField;
        Shared.Theme.Printing.Label lblPage;
        Shared.Theme.Printing.TextBox txtExp_2;
        Shared.Theme.Printing.Label lblOrdersPrinting;
        Shared.Theme.Printing.Label lblDate;
        Shared.Theme.Printing.TextBox txtExp_3;
        Shared.Theme.Colors.DefaultPrintFormColor clrDefaultPrintFormColor;
        Shared.Theme.Fonts.WizardLargeTitle fntWizardLargeTitle;
        internal Shared.Theme.Printing.ReportSection Customer;
        Shared.Theme.Colors.DefaultPrintFormColor clrDefaultPrintFormColor_;
        Shared.Theme.Printing.Label lblCustomer;
        Shared.Theme.Printing.TextBox txtCustomers_ContactName;
        Shared.Theme.Fonts.WizardSmallTitle fntWizardSmallTitle;
        internal Shared.Theme.Printing.ReportSection Body;
        Shared.Theme.Colors.DefaultPrintFormColor clrDefaultPrintFormColor_1;
        Shared.Theme.Printing.Grid grd;
        Shared.Theme.Printing.GridColumn gclOrderID;
        Shared.Theme.Printing.GridColumn gclOrderDate;
        Shared.Theme.Printing.GridColumn gclShipName;
        Shared.Theme.Printing.GridColumn gclShipCity;
        Shared.Theme.Printing.TextBox txtOrders_OrderID;
        Shared.Theme.Printing.TextBox txtOrders_OrderDate;
        Shared.Theme.Printing.TextBox txtOrders_ShipName;
        Shared.Theme.Printing.TextBox txtOrders_ShipCity;
        void InitializeComponent()
        {
            this.Header = new Northwind.Shared.Theme.Printing.ReportSection();
            this.clrDefaultEditField = new Northwind.Shared.Theme.Colors.DefaultEditField();
            this.lblPage = new Northwind.Shared.Theme.Printing.Label();
            this.clrDefaultPrintFormColor = new Northwind.Shared.Theme.Colors.DefaultPrintFormColor();
            this.txtExp_2 = new Northwind.Shared.Theme.Printing.TextBox();
            this.lblOrdersPrinting = new Northwind.Shared.Theme.Printing.Label();
            this.fntWizardLargeTitle = new Northwind.Shared.Theme.Fonts.WizardLargeTitle();
            this.lblDate = new Northwind.Shared.Theme.Printing.Label();
            this.txtExp_3 = new Northwind.Shared.Theme.Printing.TextBox();
            this.Customer = new Northwind.Shared.Theme.Printing.ReportSection();
            this.clrDefaultPrintFormColor_ = new Northwind.Shared.Theme.Colors.DefaultPrintFormColor();
            this.lblCustomer = new Northwind.Shared.Theme.Printing.Label();
            this.fntWizardSmallTitle = new Northwind.Shared.Theme.Fonts.WizardSmallTitle();
            this.txtCustomers_ContactName = new Northwind.Shared.Theme.Printing.TextBox();
            this.Body = new Northwind.Shared.Theme.Printing.ReportSection();
            this.clrDefaultPrintFormColor_1 = new Northwind.Shared.Theme.Colors.DefaultPrintFormColor();
            this.grd = new Northwind.Shared.Theme.Printing.Grid();
            this.gclOrderID = new Northwind.Shared.Theme.Printing.GridColumn();
            this.txtOrders_OrderID = new Northwind.Shared.Theme.Printing.TextBox();
            this.gclOrderDate = new Northwind.Shared.Theme.Printing.GridColumn();
            this.txtOrders_OrderDate = new Northwind.Shared.Theme.Printing.TextBox();
            this.gclShipName = new Northwind.Shared.Theme.Printing.GridColumn();
            this.txtOrders_ShipName = new Northwind.Shared.Theme.Printing.TextBox();
            this.gclShipCity = new Northwind.Shared.Theme.Printing.GridColumn();
            this.txtOrders_ShipCity = new Northwind.Shared.Theme.Printing.TextBox();
            this.Header.SuspendLayout();
            this.Customer.SuspendLayout();
            this.Body.SuspendLayout();
            this.grd.SuspendLayout();
            this.gclOrderID.SuspendLayout();
            this.gclOrderDate.SuspendLayout();
            this.gclShipName.SuspendLayout();
            this.gclShipCity.SuspendLayout();
            this.SuspendLayout();
            // 
            // Header
            // 
            this.Header.AutoScaleDimensions = new System.Drawing.SizeF(4.8F, 12F);
            this.Header.ColorScheme = this.clrDefaultEditField;
            this.Header.Controls.Add(this.txtExp_3);
            this.Header.Controls.Add(this.lblDate);
            this.Header.Controls.Add(this.lblOrdersPrinting);
            this.Header.Controls.Add(this.txtExp_2);
            this.Header.Controls.Add(this.lblPage);
            this.Header.Height = 48;
            this.Header.Name = "Header";
            // 
            // lblPage
            // 
            this.lblPage.ColorScheme = this.clrDefaultPrintFormColor;
            this.lblPage.Location = new System.Drawing.Point(365, 9);
            this.lblPage.Name = "lblPage";
            this.lblPage.Rtf = "Page:";
            this.lblPage.Size = new System.Drawing.Size(35, 12);
            this.lblPage.Text = "Page:";
            // 
            // txtExp_2
            // 
            this.txtExp_2.ColorScheme = this.clrDefaultPrintFormColor;
            this.txtExp_2.Format = "4";
            this.txtExp_2.Location = new System.Drawing.Point(401, 9);
            this.txtExp_2.Name = "txtExp_2";
            this.txtExp_2.Size = new System.Drawing.Size(26, 12);
            this.txtExp_2.Data = Firefly.Box.UI.Advanced.ControlData.FromNumber(_controller.Exp_2);
            // 
            // lblOrdersPrinting
            // 
            this.lblOrdersPrinting.ColorScheme = this.clrDefaultPrintFormColor;
            this.lblOrdersPrinting.FontScheme = this.fntWizardLargeTitle;
            this.lblOrdersPrinting.Location = new System.Drawing.Point(164, 15);
            this.lblOrdersPrinting.Name = "lblOrdersPrinting";
            this.lblOrdersPrinting.Rtf = "Orders Printing";
            this.lblOrdersPrinting.Size = new System.Drawing.Size(138, 26);
            this.lblOrdersPrinting.Text = "Orders Printing";
            // 
            // lblDate
            // 
            this.lblDate.ColorScheme = this.clrDefaultPrintFormColor;
            this.lblDate.Location = new System.Drawing.Point(365, 29);
            this.lblDate.Name = "lblDate";
            this.lblDate.Rtf = "Date:";
            this.lblDate.Size = new System.Drawing.Size(35, 12);
            this.lblDate.Text = "Date:";
            // 
            // txtExp_3
            // 
            this.txtExp_3.ColorScheme = this.clrDefaultPrintFormColor;
            this.txtExp_3.Format = "##/##/####";
            this.txtExp_3.Location = new System.Drawing.Point(402, 29);
            this.txtExp_3.Name = "txtExp_3";
            this.txtExp_3.Size = new System.Drawing.Size(78, 12);
            this.txtExp_3.Data = Firefly.Box.UI.Advanced.ControlData.FromDate(_controller.Exp_3);
            // 
            // Customer
            // 
            this.Customer.ColorScheme = this.clrDefaultPrintFormColor_;
            this.Customer.Controls.Add(this.txtCustomers_ContactName);
            this.Customer.Controls.Add(this.lblCustomer);
            this.Customer.Height = 36;
            this.Customer.Name = "Customer";
            this.Customer.PageHeader = true;
            // 
            // lblCustomer
            // 
            this.lblCustomer.ColorScheme = this.clrDefaultPrintFormColor_;
            this.lblCustomer.FontScheme = this.fntWizardSmallTitle;
            this.lblCustomer.Location = new System.Drawing.Point(5, 12);
            this.lblCustomer.Name = "lblCustomer";
            this.lblCustomer.Rtf = "Customer:";
            this.lblCustomer.Size = new System.Drawing.Size(72, 15);
            this.lblCustomer.Text = "Customer:";
            // 
            // txtCustomers_ContactName
            // 
            this.txtCustomers_ContactName.FontScheme = this.fntWizardSmallTitle;
            this.txtCustomers_ContactName.Location = new System.Drawing.Point(86, 12);
            this.txtCustomers_ContactName.Name = "txtCustomers_ContactName";
            this.txtCustomers_ContactName.Size = new System.Drawing.Size(259, 15);
            this.txtCustomers_ContactName.Data = this._controller.Customers.ContactName;
            // 
            // Body
            // 
            this.Body.ColorScheme = this.clrDefaultPrintFormColor_1;
            this.Body.Controls.Add(this.grd);
            this.Body.Height = 60;
            this.Body.Name = "Body";
            // 
            // grd
            // 
            this.grd.ColorScheme = this.clrDefaultPrintFormColor_1;
            this.grd.Controls.Add(this.gclOrderID);
            this.grd.Controls.Add(this.gclOrderDate);
            this.grd.Controls.Add(this.gclShipName);
            this.grd.Controls.Add(this.gclShipCity);
            this.grd.HeaderHeight = 18;
            this.grd.Location = new System.Drawing.Point(35, 12);
            this.grd.Name = "grd";
            this.grd.RowHeight = 16.5;
            this.grd.Size = new System.Drawing.Size(418, 36);
            // 
            // gclOrderID
            // 
            this.gclOrderID.ColorScheme = this.clrDefaultPrintFormColor_1;
            this.gclOrderID.Controls.Add(this.txtOrders_OrderID);
            this.gclOrderID.Name = "gclOrderID";
            this.gclOrderID.Text = "OrderID";
            this.gclOrderID.UseTextEndEllipsis = true;
            this.gclOrderID.Width = 73;
            // 
            // txtOrders_OrderID
            // 
            this.txtOrders_OrderID.Alignment = System.Drawing.ContentAlignment.MiddleRight;
            this.txtOrders_OrderID.Location = new System.Drawing.Point(4, 2);
            this.txtOrders_OrderID.Name = "txtOrders_OrderID";
            this.txtOrders_OrderID.Size = new System.Drawing.Size(64, 12);
            this.txtOrders_OrderID.Data = this._controller.Orders.OrderID;
            // 
            // gclOrderDate
            // 
            this.gclOrderDate.ColorScheme = this.clrDefaultPrintFormColor_1;
            this.gclOrderDate.Controls.Add(this.txtOrders_OrderDate);
            this.gclOrderDate.Name = "gclOrderDate";
            this.gclOrderDate.Text = "OrderDate";
            this.gclOrderDate.UseTextEndEllipsis = true;
            this.gclOrderDate.Width = 78;
            // 
            // txtOrders_OrderDate
            // 
            this.txtOrders_OrderDate.Location = new System.Drawing.Point(3, 2);
            this.txtOrders_OrderDate.Name = "txtOrders_OrderDate";
            this.txtOrders_OrderDate.Size = new System.Drawing.Size(70, 12);
            this.txtOrders_OrderDate.Data = this._controller.Orders.OrderDate;
            // 
            // gclShipName
            // 
            this.gclShipName.ColorScheme = this.clrDefaultPrintFormColor_1;
            this.gclShipName.Controls.Add(this.txtOrders_ShipName);
            this.gclShipName.Name = "gclShipName";
            this.gclShipName.Text = "ShipName";
            this.gclShipName.UseTextEndEllipsis = true;
            this.gclShipName.Width = 158;
            // 
            // txtOrders_ShipName
            // 
            this.txtOrders_ShipName.Location = new System.Drawing.Point(3, 2);
            this.txtOrders_ShipName.Name = "txtOrders_ShipName";
            this.txtOrders_ShipName.Size = new System.Drawing.Size(144, 12);
            this.txtOrders_ShipName.Data = this._controller.Orders.ShipName;
            // 
            // gclShipCity
            // 
            this.gclShipCity.ColorScheme = this.clrDefaultPrintFormColor_1;
            this.gclShipCity.Controls.Add(this.txtOrders_ShipCity);
            this.gclShipCity.Name = "gclShipCity";
            this.gclShipCity.Text = "ShipCity";
            this.gclShipCity.UseTextEndEllipsis = true;
            this.gclShipCity.Width = 109;
            // 
            // txtOrders_ShipCity
            // 
            this.txtOrders_ShipCity.Location = new System.Drawing.Point(3, 2);
            this.txtOrders_ShipCity.Name = "txtOrders_ShipCity";
            this.txtOrders_ShipCity.Size = new System.Drawing.Size(101, 12);
            this.txtOrders_ShipCity.Data = this._controller.Orders.ShipCity;
            // 
            // Print_OrdersC1
            // 
            this.Controls.Add(this.Header);
            this.Controls.Add(this.Customer);
            this.Controls.Add(this.Body);
            this.HorizontalExpressionFactor = 4D;
            this.HorizontalScale = 4.8D;
            this.Name = "Print_OrdersC1";
            this.UseScaleConversion = false;
            this.VerticalExpressionFactor = 8D;
            this.VerticalScale = 12D;
            this.Width = 488;
            this.Header.ResumeLayout(false);
            this.Customer.ResumeLayout(false);
            this.Body.ResumeLayout(false);
            this.grd.ResumeLayout(false);
            this.gclOrderID.ResumeLayout(false);
            this.gclOrderDate.ResumeLayout(false);
            this.gclShipName.ResumeLayout(false);
            this.gclShipCity.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
