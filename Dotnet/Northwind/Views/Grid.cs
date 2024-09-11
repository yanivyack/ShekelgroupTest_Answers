using ENV;
using ENV.Data;
using Firefly.Box;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Northwind.Views
{
    public class Grid : UIControllerBase
    {
        public readonly Models.Products products = new Models.Products();

        public Grid()
        {
            From = products;
            Where.Add(products.ProductName.StartsWith("C").Or(products.ProductName.StartsWith("c")));
        }

        public void Run()
        {
            Execute();
        }

        protected override void OnLoad()
        {
            View = () => new Views.GridView(this);
        }
    }
}