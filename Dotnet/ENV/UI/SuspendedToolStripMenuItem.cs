using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ENV.UI
{
    public class SuspendedToolStripMenuItem : System.Windows.Forms.ToolStripMenuItem
    {
        public SuspendedToolStripMenuItem()
        {
            Overflow = ToolStripItemOverflow.AsNeeded;
        }
        internal event Action ContextOpening;
        internal void OnContextOpening()
        {
            if (ContextOpening!=null)
                ContextOpening();
        }

        protected override ToolStripDropDown CreateDefaultDropDown()
        {
            return new myDropDown(this);
        }

        protected override Padding DefaultPadding
        {
            get
            {
                return new Padding(2, 0, 1, 0);
            }
        }

        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message m, Keys keyData)
        {
            if (HasDropDownItems && Enabled && ShortcutKeys == keyData)
            {
                OnClick(new EventArgs());
                return true;
            }
            return base.ProcessCmdKey(ref m, keyData);
        }

        internal class myDropDown : ToolStripDropDownMenu
        {
            Control parent;
            bool _eventsRegistered;

            public myDropDown(ToolStripMenuItem owner)
            {
                OwnerItem = owner;
                if (OwnerItem.Owner != null)
                {
                    SuspendLayout();
                    OwnerItem.Owner.Paint += Owner_Paint;
                    _eventsRegistered = true;
                }
            }

            void ResumeLayoutAndUnregisterEvents(Control owner)
            {
                if (!_eventsRegistered) return;
                ResumeLayout(true);
                PerformLayout();
                if (owner != null)
                    owner.Paint -= Owner_Paint;
            }

            void Owner_Paint(object sender, PaintEventArgs e)
            {
                ResumeLayoutAndUnregisterEvents(sender as Control);
            }

            bool _inOnInvalidated;
            protected override void OnInvalidated(InvalidateEventArgs e)
            {
                var x = false;
                if (!_inOnInvalidated && Items.Count > 0)
                {
                    _inOnInvalidated = true;
                    try
                    {
                        foreach (ToolStripItem item in Items)
                        {
                            if (!item.Available) continue;
                            if (item.Bounds.Top > DisplayRectangle.Y + item.Bounds.Height)
                                Select(true, false);
                            else
                                break;
                        }
                        for (int i = Items.Count - 1; i >= 0; i--)
                        {
                            var item = Items[i];
                            if (!item.Available) continue;
                            if (item.Bounds.Bottom < DisplayRectangle.Bottom - item.Bounds.Height)
                                Select(true, true);
                            else
                                break;

                        }
                    }
                    finally
                    {
                        _inOnInvalidated = false;
                    }
                }
                base.OnInvalidated(e);
            }

            WeakReference _sourceControl;
            protected override void OnOpened(EventArgs e)
            {
                base.OnOpened(e);
                var i = OwnerItem;
                while (i.OwnerItem != null)
                    i = i.OwnerItem;
                if (i.Owner is ContextMenuStrip && ((ContextMenuStrip)i.Owner).SourceControl != null)
                    _sourceControl = new WeakReference(((ContextMenuStrip)i.Owner).SourceControl);
            }

            public Control SourceControl { get { return _sourceControl != null ? (Control)_sourceControl.Target : null; } }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            if (Enabled && !DropDown.Visible && Owner != null && Owner.IsHandleCreated)
            {
                Owner.BeginInvoke(new Action(delegate
                {
                    if (IsDisposed) return;
                    if (DropDown.IsDisposed || DropDown.Visible) return;
                    ShowDropDown();
                }));
            }
        }
    }
    public class WindowListToolStripMenuItem : SuspendedToolStripMenuItem
    {
        public WindowListToolStripMenuItem()
        {
            Text = "WindowList";
            Available = false;
        }
        List<ToolStripItem> _items = new List<ToolStripItem>();
        protected override void OnOwnerChanged(EventArgs e)
        {
            var x = OwnerItem as ToolStripMenuItem;
            if (x != null)
            {
                x.DropDownOpening += (sender, args) =>
                {
                    foreach (var item in _items)
                    {
                        x.DropDownItems.Remove(item);
                    }
                    int myPos = x.DropDownItems.IndexOf(this);
                    int i = 0;
                    _items.Clear();
                    lock (ENV.UI.Form._windowList)
                    {
                        foreach (var zzz in ENV.UI.Form._windowList)
                        {
                            var item = zzz;
                            var s = new SuspendedToolStripMenuItem() { Text = ++i + " " + item.Text };
                            s.Click += delegate
                            {
                                item.TryFocus(delegate { });
                            };
                            x.DropDownItems.Insert(myPos + i, s);
                            _items.Add(s);
                        }
                    }
                };
            }
        }


    }
}
