using ENV.UI.Menus;
using Firefly.Box;
namespace Northwind.Views
{
    public class ApplicationMdiMenu : MenuStripBase 
    {
        public ApplicationMdiMenu()
        {
            var toolbarGroup4 = GetToolBarGroup(4);
            var toolbarGroup1 = GetToolBarGroup(1);
            var toolbarGroup2 = GetToolBarGroup(2);
            var toolbarGroup3 = GetToolBarGroup(3);
            Add(
                new MenuEntry("&File")
                #region items
                {
                    new RaiseCommand("&Shell to OS", ENV.Commands.ShellToOS)
                    , new Separator()
                    , new RaiseCommand("&Printer Setup", ENV.Commands.PrinterSettingsDialog){ Image = Properties.Resources.Printer }
                    , new Separator()
                    , new ManagedCommand("E&xit System", Command.ExitApplication)
                }
                #endregion
                , new MenuEntry("&Edit")
                #region items
                {
                    new ManagedCommand("&Cancel", Command.UndoChangesInRow)
                    , new ManagedCommand("Und&o Editing", Command.UndoEditing){ Image = Properties.Resources.Undo }
                    , new Separator()
                    , new ManagedCommand("&Zoom", Command.Expand)
                    , new ManagedCommand("&Wide", Command.ExpandTextBox)
                    , new Separator()
                    , new ManagedCommand("C&reate Line", Command.InsertRow)
                    , new ManagedCommand("&Delete Line", Command.DeleteRow)
                    , new Separator()
                    , new ManagedCommand("Cu&t", Command.Cut){ Image = Properties.Resources.Cut }
                    , new ManagedCommand("C&opy", Command.Copy){ Image = Properties.Resources.Copy }
                    , new ManagedCommand("P&aste", Command.Paste){ Image = Properties.Resources.Paste }
                    , new ManagedCommand("S&elect All", Command.SelectAll)
                    , new ManagedCommand("D&itto", Command.SetFocusedControlValueSameAsInPreviousRow)
                    , new ManagedCommand("Set to &NULL", Command.SetFocusedControlValueToNull)
                    , new Separator()
                    , new MenuEntry("&VCR")
                    #region items
                    {
                        new ManagedCommand("Begin Table", Command.GoToFirstRow){ Image = Properties.Resources.First, ToolBarGroup = toolbarGroup4 }
                        , new ManagedCommand("Previous Screen", Command.GoToPreviousPage){ Image = Properties.Resources.PageUp, ToolBarGroup = toolbarGroup4 }
                        , new ManagedCommand("Previous Row", Command.GoToPreviousRow){ Image = Properties.Resources.Previous, ToolBarGroup = toolbarGroup4 }
                        , new ManagedCommand("Next Row", Command.GoToNextRow){ Image = Properties.Resources.Next, ToolBarGroup = toolbarGroup4 }
                        , new ManagedCommand("Next Screen", Command.GoToNextPage){ Image = Properties.Resources.PageDown, ToolBarGroup = toolbarGroup4 }
                        , new ManagedCommand("End Table", Command.GoToLastRow){ Image = Properties.Resources.Last, ToolBarGroup = toolbarGroup4 }
                    }
                    #endregion
                }
                #endregion
                , new MenuEntry("&Options")
                #region items
                {
                    new ManagedCommand("&Modify Records", Command.SwitchToUpdateActivity){ Image = Properties.Resources.UpdateMode, ToolBarGroup = toolbarGroup1 }
                    , new ManagedCommand("&Create Records", Command.SwitchToInsertActivity){ Image = Properties.Resources.InsertMode, ToolBarGroup = toolbarGroup1 }
                    , new ManagedCommand("&Query Records", Command.SwitchToBrowseActivity){ Image = Properties.Resources.BrowseMode, ToolBarGroup = toolbarGroup1 }
                    , new Separator()
                    , new ManagedCommand("&Locate a Record", ENV.Commands.FindRows){ Image = Properties.Resources.Find, ToolBarGroup = toolbarGroup2 }
                    , new ManagedCommand("Locate &Next", ENV.Commands.FindNextRow){ Image = Properties.Resources.FindNext, ToolBarGroup = toolbarGroup2 }
                    , new ManagedCommand("&Range of Records", ENV.Commands.FilterRows){ Image = Properties.Resources.Filter, ToolBarGroup = toolbarGroup2 }
                    , new ManagedCommand("&View by Key", ENV.Commands.SelectOrderBy){ Image = Properties.Resources.SelectSort, ToolBarGroup = toolbarGroup2 }
                    , new ManagedCommand("&Sort Records", ENV.Commands.CustomOrderBy){ Image = Properties.Resources.CustomSort, ToolBarGroup = toolbarGroup2 }
                    , new ManagedCommand("&Print Data", ENV.Commands.ExportData)
                    , new DeveloperToolsMenu(Application.Instance, typeof (Shared.DataSources))
                    , new ManagedCommand("Clear Value", ENV.Commands.ClearCurrentValueInTemplate)
                    , new ManagedCommand("Clear Template", ENV.Commands.ClearTemplate)
                    , new ManagedCommand("From Values", ENV.Commands.TemplateFromValues)
                    , new ManagedCommand("To Value", ENV.Commands.TemplateToValues)
                    , new ManagedCommand("Define Expression", ENV.Commands.TemplateExpression)
                }
                #endregion
                , new MenuEntry("Programs")
                #region items
                {
                    new MenuEntry("Customers", () => Create<Customers.IShowCustomers>().Run()){ CloseActiveControllers = true }
                    , new Separator()
                    , new MenuEntry("Products", () => Create<Products.IShowProducts>().Run()){ CloseActiveControllers = true }
                    , new Separator()
                    , new MenuEntry("Orders", () => Create<Orders.IShowOrders>().Run()){ CloseActiveControllers = true }
                }
                #endregion
                , new MenuEntry("&Help")
                #region items
                {
                    new ManagedCommand("&Help", Command.Help){ Image = Properties.Resources.Help, ToolBarGroup = toolbarGroup3 }
                    , new Separator()
                    , new RaiseCommand("&About ", ENV.Commands.About)
                }
                #endregion
            );
        }
    }
}
