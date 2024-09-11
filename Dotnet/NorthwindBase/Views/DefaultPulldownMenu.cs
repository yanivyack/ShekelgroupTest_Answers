namespace Northwind.Views
{
    public class DefaultPulldownMenu : ENV.UI.Menus.ContextMenuStripBase 
    {
        public DefaultPulldownMenu(System.ComponentModel.IContainer container) : base(container)
        {
            InitBasedOn("Northwind", "Northwind.Views.ApplicationMdiMenu");
        }
    }
}
