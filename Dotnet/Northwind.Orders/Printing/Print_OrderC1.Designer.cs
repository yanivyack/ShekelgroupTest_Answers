using Firefly.Box.UI;
using System.Drawing;
using Firefly.Box.UI.Advanced;
using Firefly.Box;
using Northwind.Shared.Theme;
namespace Northwind.Orders.Printing
{
    partial class Print_OrderC1
    {
        internal Shared.Theme.Printing.ReportSection Header;
        Shared.Theme.Colors.DefaultEditField clrDefaultEditField;
        Shared.Theme.Printing.Label lblPage;
        Shared.Theme.Printing.TextBox txtExp_1;
        Shared.Theme.Printing.Label lblOrderPrinting;
        Shared.Theme.Printing.Label lblDate;
        Shared.Theme.Printing.TextBox txtExp_2;
        Shared.Theme.Printing.GroupBox grpDetails;
        Shared.Theme.Printing.TextBox txtOrder_Details_OrderID;
        Shared.Theme.Printing.TextBox txtOrders_ShipName;
        Shared.Theme.Printing.Label lblOrder;
        Shared.Theme.Printing.Label lblCustomer;
        Shared.Theme.Printing.Line lin;
        Shared.Theme.Printing.Label lblOrderDate;
        Shared.Theme.Printing.TextBox txtOrders_OrderDate;
        Shared.Theme.Printing.Label lblShipAddress;
        Shared.Theme.Printing.TextBox txtOrders_ShipAddress;
        Shared.Theme.Printing.Label lblRequiredDate;
        Shared.Theme.Printing.TextBox txtOrders_RequiredDate;
        Shared.Theme.Printing.Label lblShipCity;
        Shared.Theme.Printing.TextBox txtOrders_ShipCity;
        Shared.Theme.Printing.Label lblShippedDate;
        Shared.Theme.Printing.TextBox txtOrders_ShippedDate;
        Shared.Theme.Printing.Label lblShipCountry;
        Shared.Theme.Printing.TextBox txtOrders_ShipCountry;
        Shared.Theme.Printing.Label lblItemDescriptionQuantityPrice;
        Shared.Theme.Printing.Shape shp;
        Shared.Theme.Colors.DefaultPrintFormColor clrDefaultPrintFormColor;
        Shared.Theme.Fonts.WizardLargeTitle fntWizardLargeTitle;
        Shared.Theme.Fonts.DefaultDialogText fntDefaultDialogText;
        internal Shared.Theme.Printing.ReportSection Detail;
        Shared.Theme.Colors.DefaultPrintFormColor clrDefaultPrintFormColor_;
        Shared.Theme.Printing.TextBox txtOrder_Details_ProductID;
        Shared.Theme.Printing.TextBox txtProducts_ProductName;
        Shared.Theme.Printing.TextBox txtOrder_Details_Quantity;
        Shared.Theme.Printing.TextBox txtOrder_Details_UnitPrice;
        Shared.Theme.Printing.TextBox txtOrder_Details_Discount;
        Shared.Theme.Printing.TextBox txtExp_5;
        internal Shared.Theme.Printing.ReportSection Footer;
        Shared.Theme.Colors.DefaultPrintFormColor clrDefaultPrintFormColor_1;
        Shared.Theme.Printing.Shape shp_;
        Shared.Theme.Printing.Label lblOrderTotal;
        Shared.Theme.Printing.TextBox txtVOrderTotal;
        Shared.Theme.Fonts.WizardLargeTitle fntWizardLargeTitle_;
        void InitializeComponent()
        {
            this.Header = new Northwind.Shared.Theme.Printing.ReportSection();
            this.clrDefaultEditField = new Northwind.Shared.Theme.Colors.DefaultEditField();
            this.lblPage = new Northwind.Shared.Theme.Printing.Label();
            this.clrDefaultPrintFormColor = new Northwind.Shared.Theme.Colors.DefaultPrintFormColor();
            this.txtExp_1 = new Northwind.Shared.Theme.Printing.TextBox();
            this.lblOrderPrinting = new Northwind.Shared.Theme.Printing.Label();
            this.fntWizardLargeTitle = new Northwind.Shared.Theme.Fonts.WizardLargeTitle();
            this.lblDate = new Northwind.Shared.Theme.Printing.Label();
            this.txtExp_2 = new Northwind.Shared.Theme.Printing.TextBox();
            this.grpDetails = new Northwind.Shared.Theme.Printing.GroupBox();
            this.txtOrder_Details_OrderID = new Northwind.Shared.Theme.Printing.TextBox();
            this.txtOrders_ShipName = new Northwind.Shared.Theme.Printing.TextBox();
            this.lblOrder = new Northwind.Shared.Theme.Printing.Label();
            this.fntDefaultDialogText = new Northwind.Shared.Theme.Fonts.DefaultDialogText();
            this.lblCustomer = new Northwind.Shared.Theme.Printing.Label();
            this.lin = new Northwind.Shared.Theme.Printing.Line();
            this.lblOrderDate = new Northwind.Shared.Theme.Printing.Label();
            this.txtOrders_OrderDate = new Northwind.Shared.Theme.Printing.TextBox();
            this.lblShipAddress = new Northwind.Shared.Theme.Printing.Label();
            this.txtOrders_ShipAddress = new Northwind.Shared.Theme.Printing.TextBox();
            this.lblRequiredDate = new Northwind.Shared.Theme.Printing.Label();
            this.txtOrders_RequiredDate = new Northwind.Shared.Theme.Printing.TextBox();
            this.lblShipCity = new Northwind.Shared.Theme.Printing.Label();
            this.txtOrders_ShipCity = new Northwind.Shared.Theme.Printing.TextBox();
            this.lblShippedDate = new Northwind.Shared.Theme.Printing.Label();
            this.txtOrders_ShippedDate = new Northwind.Shared.Theme.Printing.TextBox();
            this.lblShipCountry = new Northwind.Shared.Theme.Printing.Label();
            this.txtOrders_ShipCountry = new Northwind.Shared.Theme.Printing.TextBox();
            this.lblItemDescriptionQuantityPrice = new Northwind.Shared.Theme.Printing.Label();
            this.shp = new Northwind.Shared.Theme.Printing.Shape();
            this.Detail = new Northwind.Shared.Theme.Printing.ReportSection();
            this.clrDefaultPrintFormColor_ = new Northwind.Shared.Theme.Colors.DefaultPrintFormColor();
            this.txtOrder_Details_ProductID = new Northwind.Shared.Theme.Printing.TextBox();
            this.txtProducts_ProductName = new Northwind.Shared.Theme.Printing.TextBox();
            this.txtOrder_Details_Quantity = new Northwind.Shared.Theme.Printing.TextBox();
            this.txtOrder_Details_UnitPrice = new Northwind.Shared.Theme.Printing.TextBox();
            this.txtOrder_Details_Discount = new Northwind.Shared.Theme.Printing.TextBox();
            this.txtExp_5 = new Northwind.Shared.Theme.Printing.TextBox();
            this.Footer = new Northwind.Shared.Theme.Printing.ReportSection();
            this.clrDefaultPrintFormColor_1 = new Northwind.Shared.Theme.Colors.DefaultPrintFormColor();
            this.shp_ = new Northwind.Shared.Theme.Printing.Shape();
            this.lblOrderTotal = new Northwind.Shared.Theme.Printing.Label();
            this.fntWizardLargeTitle_ = new Northwind.Shared.Theme.Fonts.WizardLargeTitle();
            this.txtVOrderTotal = new Northwind.Shared.Theme.Printing.TextBox();
            this.Header.SuspendLayout();
            this.Detail.SuspendLayout();
            this.Footer.SuspendLayout();
            this.SuspendLayout();
            // 
            // Header
            // 
            this.Header.AutoScaleDimensions = new System.Drawing.SizeF(4.8F, 12F);
            this.Header.ColorScheme = this.clrDefaultEditField;
            this.Header.Controls.Add(this.shp);
            this.Header.Controls.Add(this.lblItemDescriptionQuantityPrice);
            this.Header.Controls.Add(this.txtOrders_ShipCountry);
            this.Header.Controls.Add(this.lblShipCountry);
            this.Header.Controls.Add(this.txtOrders_ShippedDate);
            this.Header.Controls.Add(this.lblShippedDate);
            this.Header.Controls.Add(this.txtOrders_ShipCity);
            this.Header.Controls.Add(this.lblShipCity);
            this.Header.Controls.Add(this.txtOrders_RequiredDate);
            this.Header.Controls.Add(this.lblRequiredDate);
            this.Header.Controls.Add(this.txtOrders_ShipAddress);
            this.Header.Controls.Add(this.lblShipAddress);
            this.Header.Controls.Add(this.txtOrders_OrderDate);
            this.Header.Controls.Add(this.lblOrderDate);
            this.Header.Controls.Add(this.lin);
            this.Header.Controls.Add(this.lblCustomer);
            this.Header.Controls.Add(this.lblOrder);
            this.Header.Controls.Add(this.txtOrders_ShipName);
            this.Header.Controls.Add(this.txtOrder_Details_OrderID);
            this.Header.Controls.Add(this.grpDetails);
            this.Header.Controls.Add(this.txtExp_2);
            this.Header.Controls.Add(this.lblDate);
            this.Header.Controls.Add(this.lblOrderPrinting);
            this.Header.Controls.Add(this.txtExp_1);
            this.Header.Controls.Add(this.lblPage);
            this.Header.Height = 240;
            this.Header.Name = "Header";
            this.Header.PageHeader = true;
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
            // txtExp_1
            // 
            this.txtExp_1.ColorScheme = this.clrDefaultPrintFormColor;
            this.txtExp_1.Format = "4";
            this.txtExp_1.Location = new System.Drawing.Point(401, 9);
            this.txtExp_1.Name = "txtExp_1";
            this.txtExp_1.Size = new System.Drawing.Size(26, 12);
            this.txtExp_1.Data = Firefly.Box.UI.Advanced.ControlData.FromNumber(_controller.Exp_1);
            // 
            // lblOrderPrinting
            // 
            this.lblOrderPrinting.ColorScheme = this.clrDefaultPrintFormColor;
            this.lblOrderPrinting.FontScheme = this.fntWizardLargeTitle;
            this.lblOrderPrinting.Location = new System.Drawing.Point(164, 15);
            this.lblOrderPrinting.Name = "lblOrderPrinting";
            this.lblOrderPrinting.Rtf = "Order Printing";
            this.lblOrderPrinting.Size = new System.Drawing.Size(138, 26);
            this.lblOrderPrinting.Text = "Order Printing";
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
            // txtExp_2
            // 
            this.txtExp_2.ColorScheme = this.clrDefaultPrintFormColor;
            this.txtExp_2.Format = "##/##/####";
            this.txtExp_2.Location = new System.Drawing.Point(402, 29);
            this.txtExp_2.Name = "txtExp_2";
            this.txtExp_2.Size = new System.Drawing.Size(78, 12);
            this.txtExp_2.Data = Firefly.Box.UI.Advanced.ControlData.FromDate(_controller.Exp_2);
            // 
            // grpDetails
            // 
            this.grpDetails.ColorScheme = this.clrDefaultEditField;
            this.grpDetails.LineWidth = 3;
            this.grpDetails.Location = new System.Drawing.Point(43, 66);
            this.grpDetails.Name = "grpDetails";
            this.grpDetails.Size = new System.Drawing.Size(407, 126);
            this.grpDetails.Text = "Details";
            // 
            // txtOrder_Details_OrderID
            // 
            this.txtOrder_Details_OrderID.Border = Firefly.Box.UI.ControlBorderStyle.Thin;
            this.txtOrder_Details_OrderID.BoundTo = new Firefly.Box.UI.ControlBinding(this.grpDetails);
            this.txtOrder_Details_OrderID.Location = new System.Drawing.Point(92, 87);
            this.txtOrder_Details_OrderID.Name = "txtOrder_Details_OrderID";
            this.txtOrder_Details_OrderID.Size = new System.Drawing.Size(67, 18);
            this.txtOrder_Details_OrderID.Data = this._controller.Order_Details.OrderID;
            // 
            // txtOrders_ShipName
            // 
            this.txtOrders_ShipName.Border = Firefly.Box.UI.ControlBorderStyle.Thin;
            this.txtOrders_ShipName.BoundTo = new Firefly.Box.UI.ControlBinding(this.grpDetails);
            this.txtOrders_ShipName.Location = new System.Drawing.Point(251, 87);
            this.txtOrders_ShipName.Name = "txtOrders_ShipName";
            this.txtOrders_ShipName.Size = new System.Drawing.Size(191, 18);
            this.txtOrders_ShipName.Data = this._controller.Orders.ShipName;
            // 
            // lblOrder
            // 
            this.lblOrder.BoundTo = new Firefly.Box.UI.ControlBinding(this.grpDetails);
            this.lblOrder.ColorScheme = this.clrDefaultPrintFormColor;
            this.lblOrder.FontScheme = this.fntDefaultDialogText;
            this.lblOrder.Location = new System.Drawing.Point(54, 90);
            this.lblOrder.Name = "lblOrder";
            this.lblOrder.Rtf = "Order:";
            this.lblOrder.Size = new System.Drawing.Size(38, 18);
            this.lblOrder.Text = "Order:";
            // 
            // lblCustomer
            // 
            this.lblCustomer.BoundTo = new Firefly.Box.UI.ControlBinding(this.grpDetails);
            this.lblCustomer.ColorScheme = this.clrDefaultPrintFormColor;
            this.lblCustomer.FontScheme = this.fntDefaultDialogText;
            this.lblCustomer.Location = new System.Drawing.Point(193, 90);
            this.lblCustomer.Name = "lblCustomer";
            this.lblCustomer.Rtf = "Customer:";
            this.lblCustomer.Size = new System.Drawing.Size(55, 18);
            this.lblCustomer.Text = "Customer:";
            // 
            // lin
            // 
            this.lin.BoundTo = new Firefly.Box.UI.ControlBinding(this.grpDetails);
            this.lin.ColorScheme = this.clrDefaultPrintFormColor;
            this.lin.End = new System.Drawing.Point(449, 111);
            this.lin.Name = "lin";
            this.lin.Start = new System.Drawing.Point(49, 111);
            // 
            // lblOrderDate
            // 
            this.lblOrderDate.BoundTo = new Firefly.Box.UI.ControlBinding(this.grpDetails);
            this.lblOrderDate.ColorScheme = this.clrDefaultPrintFormColor;
            this.lblOrderDate.FontScheme = this.fntDefaultDialogText;
            this.lblOrderDate.Location = new System.Drawing.Point(56, 120);
            this.lblOrderDate.Name = "lblOrderDate";
            this.lblOrderDate.Rtf = "Order Date:";
            this.lblOrderDate.Size = new System.Drawing.Size(50, 12);
            this.lblOrderDate.Text = "Order Date:";
            // 
            // txtOrders_OrderDate
            // 
            this.txtOrders_OrderDate.Border = Firefly.Box.UI.ControlBorderStyle.Thin;
            this.txtOrders_OrderDate.BoundTo = new Firefly.Box.UI.ControlBinding(this.grpDetails);
            this.txtOrders_OrderDate.Format = "##/##/####Z";
            this.txtOrders_OrderDate.Location = new System.Drawing.Point(127, 120);
            this.txtOrders_OrderDate.Name = "txtOrders_OrderDate";
            this.txtOrders_OrderDate.Size = new System.Drawing.Size(73, 15);
            this.txtOrders_OrderDate.Data = this._controller.Orders.OrderDate;
            // 
            // lblShipAddress
            // 
            this.lblShipAddress.BoundTo = new Firefly.Box.UI.ControlBinding(this.grpDetails);
            this.lblShipAddress.ColorScheme = this.clrDefaultPrintFormColor;
            this.lblShipAddress.FontScheme = this.fntDefaultDialogText;
            this.lblShipAddress.Location = new System.Drawing.Point(241, 120);
            this.lblShipAddress.Name = "lblShipAddress";
            this.lblShipAddress.Rtf = "Ship Address:";
            this.lblShipAddress.Size = new System.Drawing.Size(60, 12);
            this.lblShipAddress.Text = "Ship Address:";
            // 
            // txtOrders_ShipAddress
            // 
            this.txtOrders_ShipAddress.Border = Firefly.Box.UI.ControlBorderStyle.Thin;
            this.txtOrders_ShipAddress.BoundTo = new Firefly.Box.UI.ControlBinding(this.grpDetails);
            this.txtOrders_ShipAddress.Location = new System.Drawing.Point(312, 120);
            this.txtOrders_ShipAddress.Name = "txtOrders_ShipAddress";
            this.txtOrders_ShipAddress.Size = new System.Drawing.Size(104, 15);
            this.txtOrders_ShipAddress.Data = this._controller.Orders.ShipAddress;
            // 
            // lblRequiredDate
            // 
            this.lblRequiredDate.BoundTo = new Firefly.Box.UI.ControlBinding(this.grpDetails);
            this.lblRequiredDate.ColorScheme = this.clrDefaultPrintFormColor;
            this.lblRequiredDate.FontScheme = this.fntDefaultDialogText;
            this.lblRequiredDate.Location = new System.Drawing.Point(56, 141);
            this.lblRequiredDate.Name = "lblRequiredDate";
            this.lblRequiredDate.Rtf = "Required Date:";
            this.lblRequiredDate.Size = new System.Drawing.Size(66, 12);
            this.lblRequiredDate.Text = "Required Date:";
            // 
            // txtOrders_RequiredDate
            // 
            this.txtOrders_RequiredDate.Border = Firefly.Box.UI.ControlBorderStyle.Thin;
            this.txtOrders_RequiredDate.BoundTo = new Firefly.Box.UI.ControlBinding(this.grpDetails);
            this.txtOrders_RequiredDate.Format = "##/##/####Z";
            this.txtOrders_RequiredDate.Location = new System.Drawing.Point(127, 141);
            this.txtOrders_RequiredDate.Name = "txtOrders_RequiredDate";
            this.txtOrders_RequiredDate.Size = new System.Drawing.Size(73, 15);
            this.txtOrders_RequiredDate.Data = this._controller.Orders.RequiredDate;
            // 
            // lblShipCity
            // 
            this.lblShipCity.BoundTo = new Firefly.Box.UI.ControlBinding(this.grpDetails);
            this.lblShipCity.ColorScheme = this.clrDefaultPrintFormColor;
            this.lblShipCity.FontScheme = this.fntDefaultDialogText;
            this.lblShipCity.Location = new System.Drawing.Point(241, 141);
            this.lblShipCity.Name = "lblShipCity";
            this.lblShipCity.Rtf = "Ship City:";
            this.lblShipCity.Size = new System.Drawing.Size(40, 12);
            this.lblShipCity.Text = "Ship City:";
            // 
            // txtOrders_ShipCity
            // 
            this.txtOrders_ShipCity.Border = Firefly.Box.UI.ControlBorderStyle.Thin;
            this.txtOrders_ShipCity.BoundTo = new Firefly.Box.UI.ControlBinding(this.grpDetails);
            this.txtOrders_ShipCity.Location = new System.Drawing.Point(312, 141);
            this.txtOrders_ShipCity.Name = "txtOrders_ShipCity";
            this.txtOrders_ShipCity.Size = new System.Drawing.Size(104, 15);
            this.txtOrders_ShipCity.Data = this._controller.Orders.ShipCity;
            // 
            // lblShippedDate
            // 
            this.lblShippedDate.BoundTo = new Firefly.Box.UI.ControlBinding(this.grpDetails);
            this.lblShippedDate.ColorScheme = this.clrDefaultPrintFormColor;
            this.lblShippedDate.FontScheme = this.fntDefaultDialogText;
            this.lblShippedDate.Location = new System.Drawing.Point(56, 162);
            this.lblShippedDate.Name = "lblShippedDate";
            this.lblShippedDate.Rtf = "Shipped Date:";
            this.lblShippedDate.Size = new System.Drawing.Size(62, 12);
            this.lblShippedDate.Text = "Shipped Date:";
            // 
            // txtOrders_ShippedDate
            // 
            this.txtOrders_ShippedDate.Border = Firefly.Box.UI.ControlBorderStyle.Thin;
            this.txtOrders_ShippedDate.BoundTo = new Firefly.Box.UI.ControlBinding(this.grpDetails);
            this.txtOrders_ShippedDate.Format = "##/##/####Z";
            this.txtOrders_ShippedDate.Location = new System.Drawing.Point(127, 162);
            this.txtOrders_ShippedDate.Name = "txtOrders_ShippedDate";
            this.txtOrders_ShippedDate.Size = new System.Drawing.Size(73, 15);
            this.txtOrders_ShippedDate.Data = this._controller.Orders.ShippedDate;
            // 
            // lblShipCountry
            // 
            this.lblShipCountry.BoundTo = new Firefly.Box.UI.ControlBinding(this.grpDetails);
            this.lblShipCountry.ColorScheme = this.clrDefaultPrintFormColor;
            this.lblShipCountry.FontScheme = this.fntDefaultDialogText;
            this.lblShipCountry.Location = new System.Drawing.Point(241, 162);
            this.lblShipCountry.Name = "lblShipCountry";
            this.lblShipCountry.Rtf = "Ship Country:";
            this.lblShipCountry.Size = new System.Drawing.Size(58, 12);
            this.lblShipCountry.Text = "Ship Country:";
            // 
            // txtOrders_ShipCountry
            // 
            this.txtOrders_ShipCountry.Border = Firefly.Box.UI.ControlBorderStyle.Thin;
            this.txtOrders_ShipCountry.BoundTo = new Firefly.Box.UI.ControlBinding(this.grpDetails);
            this.txtOrders_ShipCountry.Location = new System.Drawing.Point(312, 162);
            this.txtOrders_ShipCountry.Name = "txtOrders_ShipCountry";
            this.txtOrders_ShipCountry.Size = new System.Drawing.Size(104, 15);
            this.txtOrders_ShipCountry.Data = this._controller.Orders.ShipCountry;
            // 
            // lblItemDescriptionQuantityPrice
            // 
            this.lblItemDescriptionQuantityPrice.ColorScheme = this.clrDefaultPrintFormColor;
            this.lblItemDescriptionQuantityPrice.Location = new System.Drawing.Point(8, 218);
            this.lblItemDescriptionQuantityPrice.Name = "lblItemDescriptionQuantityPrice";
            this.lblItemDescriptionQuantityPrice.Rtf = "Item                             Description                                       Quantity    Price      Discount      Total";
            this.lblItemDescriptionQuantityPrice.Size = new System.Drawing.Size(464, 12);
            this.lblItemDescriptionQuantityPrice.Text = "Item                             Description                                       Quantity    Price      Discount      Total";
            // 
            // shp
            // 
            this.shp.ColorScheme = this.clrDefaultPrintFormColor;
            this.shp.LineHorizontal = true;
            this.shp.LineWidth = 2;
            this.shp.Location = new System.Drawing.Point(7, 231);
            this.shp.Name = "shp";
            this.shp.Size = new System.Drawing.Size(460, 8);
            this.shp.Text = "";
            // 
            // Detail
            // 
            this.Detail.ColorScheme = this.clrDefaultPrintFormColor_;
            this.Detail.Controls.Add(this.txtExp_5);
            this.Detail.Controls.Add(this.txtOrder_Details_Discount);
            this.Detail.Controls.Add(this.txtOrder_Details_UnitPrice);
            this.Detail.Controls.Add(this.txtOrder_Details_Quantity);
            this.Detail.Controls.Add(this.txtProducts_ProductName);
            this.Detail.Controls.Add(this.txtOrder_Details_ProductID);
            this.Detail.Height = 18;
            this.Detail.Name = "Detail";
            // 
            // txtOrder_Details_ProductID
            // 
            this.txtOrder_Details_ProductID.Location = new System.Drawing.Point(7, 3);
            this.txtOrder_Details_ProductID.Name = "txtOrder_Details_ProductID";
            this.txtOrder_Details_ProductID.Size = new System.Drawing.Size(64, 12);
            this.txtOrder_Details_ProductID.Data = this._controller.Order_Details.ProductID;
            // 
            // txtProducts_ProductName
            // 
            this.txtProducts_ProductName.Location = new System.Drawing.Point(113, 3);
            this.txtProducts_ProductName.Name = "txtProducts_ProductName";
            this.txtProducts_ProductName.Size = new System.Drawing.Size(157, 12);
            this.txtProducts_ProductName.Data = this._controller.Products.ProductName;
            // 
            // txtOrder_Details_Quantity
            // 
            this.txtOrder_Details_Quantity.Alignment = System.Drawing.ContentAlignment.MiddleRight;
            this.txtOrder_Details_Quantity.Location = new System.Drawing.Point(277, 3);
            this.txtOrder_Details_Quantity.Name = "txtOrder_Details_Quantity";
            this.txtOrder_Details_Quantity.Size = new System.Drawing.Size(35, 12);
            this.txtOrder_Details_Quantity.Data = this._controller.Order_Details.Quantity;
            // 
            // txtOrder_Details_UnitPrice
            // 
            this.txtOrder_Details_UnitPrice.Alignment = System.Drawing.ContentAlignment.MiddleRight;
            this.txtOrder_Details_UnitPrice.Format = "5.2C+$;";
            this.txtOrder_Details_UnitPrice.Location = new System.Drawing.Point(323, 3);
            this.txtOrder_Details_UnitPrice.Name = "txtOrder_Details_UnitPrice";
            this.txtOrder_Details_UnitPrice.Size = new System.Drawing.Size(42, 12);
            this.txtOrder_Details_UnitPrice.Data = this._controller.Order_Details.UnitPrice;
            // 
            // txtOrder_Details_Discount
            // 
            this.txtOrder_Details_Discount.Alignment = System.Drawing.ContentAlignment.MiddleRight;
            this.txtOrder_Details_Discount.Format = "5.2C+$;";
            this.txtOrder_Details_Discount.Location = new System.Drawing.Point(367, 3);
            this.txtOrder_Details_Discount.Name = "txtOrder_Details_Discount";
            this.txtOrder_Details_Discount.Size = new System.Drawing.Size(46, 12);
            this.txtOrder_Details_Discount.Data = this._controller.Order_Details.Discount;
            // 
            // txtExp_5
            // 
            this.txtExp_5.ColorScheme = this.clrDefaultPrintFormColor_;
            this.txtExp_5.Format = "5.2C+$;";
            this.txtExp_5.Location = new System.Drawing.Point(420, 3);
            this.txtExp_5.Name = "txtExp_5";
            this.txtExp_5.Size = new System.Drawing.Size(43, 12);
            this.txtExp_5.Data = Firefly.Box.UI.Advanced.ControlData.FromNumber(_controller.Exp_5);
            // 
            // Footer
            // 
            this.Footer.ColorScheme = this.clrDefaultPrintFormColor_1;
            this.Footer.Controls.Add(this.txtVOrderTotal);
            this.Footer.Controls.Add(this.lblOrderTotal);
            this.Footer.Controls.Add(this.shp_);
            this.Footer.Height = 87;
            this.Footer.Name = "Footer";
            // 
            // shp_
            // 
            this.shp_.ColorScheme = this.clrDefaultPrintFormColor_1;
            this.shp_.LineWidth = 3;
            this.shp_.Location = new System.Drawing.Point(187, 18);
            this.shp_.Name = "shp_";
            this.shp_.Size = new System.Drawing.Size(263, 48);
            this.shp_.Square = true;
            // 
            // lblOrderTotal
            // 
            this.lblOrderTotal.BoundTo = new Firefly.Box.UI.ControlBinding(this.shp_);
            this.lblOrderTotal.ColorScheme = this.clrDefaultPrintFormColor_1;
            this.lblOrderTotal.FontScheme = this.fntWizardLargeTitle_;
            this.lblOrderTotal.Location = new System.Drawing.Point(242, 35);
            this.lblOrderTotal.Name = "lblOrderTotal";
            this.lblOrderTotal.Rtf = "Order Total >>";
            this.lblOrderTotal.Size = new System.Drawing.Size(115, 20);
            this.lblOrderTotal.Text = "Order Total >>";
            // 
            // txtVOrderTotal
            // 
            this.txtVOrderTotal.Alignment = System.Drawing.ContentAlignment.MiddleRight;
            this.txtVOrderTotal.BoundTo = new Firefly.Box.UI.ControlBinding(this.shp_);
            this.txtVOrderTotal.FontScheme = this.fntWizardLargeTitle_;
            this.txtVOrderTotal.Location = new System.Drawing.Point(353, 35);
            this.txtVOrderTotal.Name = "txtVOrderTotal";
            this.txtVOrderTotal.Size = new System.Drawing.Size(86, 20);
            this.txtVOrderTotal.Data = this._controller.vOrderTotal;
            // 
            // Print_OrderC1
            // 
            this.Controls.Add(this.Header);
            this.Controls.Add(this.Detail);
            this.Controls.Add(this.Footer);
            this.HorizontalExpressionFactor = 4D;
            this.HorizontalScale = 4.8D;
            this.Name = "Print_OrderC1";
            this.UseScaleConversion = false;
            this.VerticalExpressionFactor = 8D;
            this.VerticalScale = 12D;
            this.Width = 488;
            this.Header.ResumeLayout(false);
            this.Detail.ResumeLayout(false);
            this.Footer.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
