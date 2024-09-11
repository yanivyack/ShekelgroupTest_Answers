namespace Northwind
{
    class ApplicationPrograms : ENV.ProgramCollection 
    {
        public ApplicationPrograms()
        {
            Add(3, "Show Customers", "", typeof(Customers.IShowCustomers));
            Add(4, "Show Products", "", typeof(Products.IShowProducts));
            Add(5, "Show Orders", "", typeof(Orders.IShowOrders));
            Add(6, "Print - Order", "", "Northwind.Orders.Print_Order");
            Add(7, "Print - Orders", "", "Northwind.Orders.Print_Orders");
        }
    }
}
