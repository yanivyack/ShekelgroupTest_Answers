using ENV;
using ENV.Data;
using Firefly.Box;
using Firefly.Box.UI.Advanced;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Northwind.Views.Views
{
    partial class GridView : Shared.Theme.Controls.Form
    {
        Grid _controller;
        public GridView(Grid controller)
        {
            _controller = controller;
            InitializeComponent();
        }
    }
}