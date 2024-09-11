using Firefly.Box.UI;
using System.Drawing;
using Firefly.Box.UI.Advanced;
using Firefly.Box;
using Northwind.Shared.Theme;
namespace Northwind.Customers.Views
{
    partial class ShowCustomersView
    {
        System.ComponentModel.IContainer components;
        Shared.Theme.Colors.DefaultHelpWindow clrDefaultHelpWindow;
        Northwind.Views.Controls.V9CompatibleDefaultTable grd;
        Shared.Theme.Controls.CompatibleGridColumn gclCustomerID;
        Shared.Theme.Controls.CompatibleGridColumn gclCompanyName;
        Shared.Theme.Controls.CompatibleGridColumn gclAddress;
        Shared.Theme.Controls.CompatibleGridColumn gclCity;
        Shared.Theme.Controls.CompatibleGridColumn gclPhone;
        Shared.Theme.Controls.CompatibleTextBox txtCustomers_CustomerID;
        Shared.Theme.Controls.CompatibleTextBox txtCustomers_CompanyName;
        Shared.Theme.Controls.CompatibleTextBox txtCustomers_Address;
        Shared.Theme.Controls.CompatibleTextBox txtCustomers_City;
        Shared.Theme.Controls.CompatibleTextBox txtCustomers_Phone;
        Northwind.Views.Controls.OK btn;
        Northwind.Views.Controls.Close btn_;
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
            this.gclCustomerID = new Northwind.Shared.Theme.Controls.CompatibleGridColumn();
            this.txtCustomers_CustomerID = new Northwind.Shared.Theme.Controls.CompatibleTextBox();
            this.gclCompanyName = new Northwind.Shared.Theme.Controls.CompatibleGridColumn();
            this.txtCustomers_CompanyName = new Northwind.Shared.Theme.Controls.CompatibleTextBox();
            this.gclAddress = new Northwind.Shared.Theme.Controls.CompatibleGridColumn();
            this.txtCustomers_Address = new Northwind.Shared.Theme.Controls.CompatibleTextBox();
            this.gclCity = new Northwind.Shared.Theme.Controls.CompatibleGridColumn();
            this.txtCustomers_City = new Northwind.Shared.Theme.Controls.CompatibleTextBox();
            this.gclPhone = new Northwind.Shared.Theme.Controls.CompatibleGridColumn();
            this.txtCustomers_Phone = new Northwind.Shared.Theme.Controls.CompatibleTextBox();
            this.btn = new Northwind.Views.Controls.OK();
            this.btn_ = new Northwind.Views.Controls.Close();
            TabOrderMode = Firefly.Box.UI.TabOrderMode.ManualIgnoringContainerHeirarchy;
            this.grd.SuspendLayout();
            this.gclCustomerID.SuspendLayout();
            this.gclCompanyName.SuspendLayout();
            this.gclAddress.SuspendLayout();
            this.gclCity.SuspendLayout();
            this.gclPhone.SuspendLayout();
            this.SuspendLayout();
            // 
            // grd
            // 
            this.grd.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 100);
            this.grd.AllowMultiSelect = false;
            this.grd.Controls.Add(this.gclCustomerID);
            this.grd.Controls.Add(this.gclCompanyName);
            this.grd.Controls.Add(this.gclAddress);
            this.grd.Controls.Add(this.gclCity);
            this.grd.Controls.Add(this.gclPhone);
            this.grd.Location = new System.Drawing.Point(10, 13);
            this.grd.Name = "grd";
            this.grd.RowHeight = 21;
            this.grd.Size = new System.Drawing.Size(793, 296);
            // 
            // gclCustomerID
            // 
            this.gclCustomerID.AllowSort = true;
            this.gclCustomerID.Controls.Add(this.txtCustomers_CustomerID);
            this.gclCustomerID.Name = "gclCustomerID";
            this.gclCustomerID.Text = "CustomerID";
            this.gclCustomerID.Width = 60;
            // 
            // txtCustomers_CustomerID
            // 
            this.txtCustomers_CustomerID.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 0);
            this.txtCustomers_CustomerID.Location = new System.Drawing.Point(3, 1);
            this.txtCustomers_CustomerID.Name = "txtCustomers_CustomerID";
            this.txtCustomers_CustomerID.Size = new System.Drawing.Size(39, 16);
            this.txtCustomers_CustomerID.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtCustomers_CustomerID.TabIndex = 1;
            this.txtCustomers_CustomerID.Tag = "CustomerID";
            this.txtCustomers_CustomerID.Data = this._controller.Customers.CustomerID;
            // 
            // gclCompanyName
            // 
            this.gclCompanyName.AllowSort = true;
            this.gclCompanyName.Controls.Add(this.txtCustomers_CompanyName);
            this.gclCompanyName.Name = "gclCompanyName";
            this.gclCompanyName.Text = "Company Name";
            this.gclCompanyName.Width = 170;
            // 
            // txtCustomers_CompanyName
            // 
            this.txtCustomers_CompanyName.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 0);
            this.txtCustomers_CompanyName.Location = new System.Drawing.Point(2, 1);
            this.txtCustomers_CompanyName.Name = "txtCustomers_CompanyName";
            this.txtCustomers_CompanyName.Size = new System.Drawing.Size(160, 16);
            this.txtCustomers_CompanyName.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtCustomers_CompanyName.TabIndex = 2;
            this.txtCustomers_CompanyName.Tag = "CompanyName";
            this.txtCustomers_CompanyName.Data = this._controller.Customers.CompanyName;
            // 
            // gclAddress
            // 
            this.gclAddress.Controls.Add(this.txtCustomers_Address);
            this.gclAddress.Name = "gclAddress";
            this.gclAddress.Text = "Address";
            this.gclAddress.Width = 306;
            // 
            // txtCustomers_Address
            // 
            this.txtCustomers_Address.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 0);
            this.txtCustomers_Address.Location = new System.Drawing.Point(2, 1);
            this.txtCustomers_Address.Name = "txtCustomers_Address";
            this.txtCustomers_Address.Size = new System.Drawing.Size(300, 16);
            this.txtCustomers_Address.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtCustomers_Address.TabIndex = 3;
            this.txtCustomers_Address.Tag = "Address";
            this.txtCustomers_Address.Data = this._controller.Customers.Address;
            // 
            // gclCity
            // 
            this.gclCity.AllowSort = true;
            this.gclCity.Controls.Add(this.txtCustomers_City);
            this.gclCity.Name = "gclCity";
            this.gclCity.Text = "City";
            this.gclCity.Width = 100;
            // 
            // txtCustomers_City
            // 
            this.txtCustomers_City.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 0);
            this.txtCustomers_City.Location = new System.Drawing.Point(2, 1);
            this.txtCustomers_City.Name = "txtCustomers_City";
            this.txtCustomers_City.Size = new System.Drawing.Size(95, 16);
            this.txtCustomers_City.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtCustomers_City.TabIndex = 4;
            this.txtCustomers_City.Tag = "City";
            this.txtCustomers_City.Data = this._controller.Customers.City;
            // 
            // gclPhone
            // 
            this.gclPhone.Controls.Add(this.txtCustomers_Phone);
            this.gclPhone.Name = "gclPhone";
            this.gclPhone.Text = "Phone";
            this.gclPhone.Width = 134;
            // 
            // txtCustomers_Phone
            // 
            this.txtCustomers_Phone.AdvancedAnchor = new Firefly.Box.UI.AdvancedAnchor(0, 100, 0, 0);
            this.txtCustomers_Phone.Location = new System.Drawing.Point(2, 1);
            this.txtCustomers_Phone.Name = "txtCustomers_Phone";
            this.txtCustomers_Phone.Size = new System.Drawing.Size(125, 16);
            this.txtCustomers_Phone.Style = Firefly.Box.UI.ControlStyle.Flat;
            this.txtCustomers_Phone.TabIndex = 5;
            this.txtCustomers_Phone.Tag = "Phone";
            this.txtCustomers_Phone.Data = this._controller.Customers.Phone;
            // 
            // btn
            // 
            this.btn.Location = new System.Drawing.Point(660, 325);
            this.btn.Name = "btn";
            this.btn.RaiseClickBeforeFocusChange = true;
            this.btn.Size = new System.Drawing.Size(60, 23);
            this.btn.TabIndex = 7;
            // 
            // btn_
            // 
            this.btn_.Location = new System.Drawing.Point(735, 325);
            this.btn_.Name = "btn_";
            this.btn_.RaiseClickBeforeFocusChange = true;
            this.btn_.Size = new System.Drawing.Size(60, 23);
            this.btn_.TabIndex = 6;
            // 
            // ShowCustomersView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(5F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(810, 361);
            this.ColorScheme = this.clrDefaultHelpWindow;
            this.Controls.Add(this.btn_);
            this.Controls.Add(this.btn);
            this.Controls.Add(this.grd);
            this.HorizontalExpressionFactor = 4D;
            this.HorizontalScale = 5D;
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "ShowCustomersView";
            this.StartPosition = Firefly.Box.UI.WindowStartPosition.CenterMDI;
            this.Text = "Show Customers";
            this.VerticalExpressionFactor = 8D;
            this.VerticalScale = 13D;
            this.grd.ResumeLayout(false);
            this.gclCustomerID.ResumeLayout(false);
            this.gclCompanyName.ResumeLayout(false);
            this.gclAddress.ResumeLayout(false);
            this.gclCity.ResumeLayout(false);
            this.gclPhone.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
