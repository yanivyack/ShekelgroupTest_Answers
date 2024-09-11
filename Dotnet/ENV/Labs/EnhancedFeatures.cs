using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ENV.Data;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using Form = System.Windows.Forms.Form;

namespace ENV.Labs
{
    public class EnhancedFeatures
    {

        public static void Enable()
        {
            Data.DateColumn.NewInstance += x => x.Expand += () => DateColumn_Expand(x);
            ENV.UI.Grid.AlwaysEnableGridEnhancements = true;

        }






        public static System.Drawing.Point PointToMdiClient(System.Drawing.Point p)
        {
            var result = p;
            Common.RunOnRootMDI(
                mdi =>
                {
                    foreach (Control c in mdi.Controls)
                    {
                        if (c is MdiClient)
                            result = c.PointToClient(p);
                    }
                });
            return result;
        }

        static void DateColumn_Expand(Data.DateColumn column)
        {
            var formParent = Firefly.Box.Context.Current.ActiveTasks[Firefly.Box.Context.Current.ActiveTasks.Count - 1] as UIController;
            if (formParent != null)
            {
                var control = formParent.View.LastFocusedControl;
                var f = new ENV.UI.Form
                {
                    ChildWindow = true,
                    TitleBar = false,
                    Border = Firefly.Box.UI.ControlBorderStyle.None,
                    Dock = DockStyle.Fill
                };
                //f.Location = location;
                var dtp = new MonthCalendar() { Dock = DockStyle.Fill };
                try
                {
                    dtp.SelectionStart = column.Value.ToDateTime();
                    dtp.SelectionEnd = column.Value.ToDateTime();
                    dtp.MaxSelectionCount = 1;
                }
                catch
                {
                }
                f.ClientSize = new System.Drawing.Size(255, 162);
                f.Controls.Add(dtp);
                var uic = new UIController { View = f, AllowSelect = true };
                dtp.DateSelected += delegate { uic.Raise(Command.Select); };


                uic.Handlers.Add(Firefly.Box.Command.Select).Invokes += zz =>
                {
                    if (formParent.Activity != Activities.Browse && !control.ReadOnly)
                        column.Value = Date.FromDateTime(dtp.SelectionStart);
                };


                ShowAsCombo(uic);
            }
        }

        static void ShowAsCombo(UIController uic)
        {
            var formParent =
                Firefly.Box.Context.Current.ActiveTasks[Firefly.Box.Context.Current.ActiveTasks.Count - 1] as
                UIController;
            if (formParent != null)
            {

                var control = formParent.View.LastFocusedControl;
                var location = new System.Drawing.Point(control.Left, control.Bottom);
                location = control.Parent.PointToScreen(location);
                var tsDropDown = new ToolStripDropDown();
                var c = new System.Windows.Forms.Control { Size = uic.View.ClientSize };
                var tsControlHost = new ToolStripControlHost(c);
                tsDropDown.Items.Add(tsControlHost);

                tsDropDown.Show(location);
                var uicEnded = false;
                var exit = new CustomCommand();
                uic.Handlers.Add(exit).Invokes +=
                    eventArgs =>
                    {
                        if (!uicEnded)
                            uic.Exit();
                        uicEnded = true;
                    };
                tsDropDown.Closing +=
                    (sender, args) =>
                    {
                        if (!uicEnded)
                        {
                            uic.Raise(exit);
                            args.Cancel = true;
                        }
                    };
                uic.View.SetParentControl(c);

                uic.Run();
                uicEnded = true;
                tsDropDown.Close();
            }
        }
    }
}