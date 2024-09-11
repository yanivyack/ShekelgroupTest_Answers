using Firefly.Box.UI;
namespace Northwind.Views
{
    public class ContextMenuMap : ENV.UI.ContextMenuMap 
    {
        public ContextMenuMap(System.ComponentModel.IContainer container)
        {
            this.Add(2, () => new DefaultContextMenu(container));
            this.Add(1, () => new DefaultPulldownMenu(container));
        }
    }
}
