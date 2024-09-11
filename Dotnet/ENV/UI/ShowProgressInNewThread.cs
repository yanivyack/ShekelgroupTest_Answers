using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Firefly.Box.Advanced;

namespace ENV.UI
{
    public class ShowProgressInNewThread : IDisposable
    {

        Thread _t;
        int _progress = 0;
        Action _dispose;
        bool _disposed = false;
        public ShowProgressInNewThread(BusinessProcessBase bp, string title = null) : this(title ?? bp.Title)
        {
            bp.LeaveRow += () => this.Progress(bp.Counter);
        }
        public ShowProgressInNewThread(string title)
        {
            _dispose = () => _disposed = true;
            _t = new System.Threading.Thread(() =>
            {
                if (_disposed)
                    return;
                var f = new System.Windows.Forms.Form() { StartPosition = FormStartPosition.CenterParent };
                f.Text = title;
                var l = new System.Windows.Forms.Label();
                f.Height = 100;
                f.Width = 400;
                l.AutoSize = false;
                l.Dock = DockStyle.Fill;
                l.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                l.Font = new System.Drawing.Font(l.Font.Name, 10);


                var pb = new ProgressBar() { Style = ProgressBarStyle.Marquee };


                pb.Dock = DockStyle.Top;
                f.Controls.Add(l);
                f.Controls.Add(pb);

                var t = new System.Windows.Forms.Timer();
                t.Interval = 50;
                t.Tick += (sender, args) =>
                {
                    l.Text = "Processed " + _progress.ToString("###,###,###")+" "+_lastText;

                };

                t.Start();
                f.FormClosing += (sender, args) => { if (!_disposed)Cancel = true; args.Cancel = true; };
                _dispose = () =>
                {
                    _disposed = true;

                    f.BeginInvoke(new Action(() =>
                    {
                        if (f.Visible)
                            f.Close();
                        t.Dispose();
                        f.Dispose();
                    }));


                };
                if (!_disposed)
                    f.ShowDialog();
                else
                    _dispose();
            });
            _t.Start();


        }
        public bool Cancel = false;
        public void Progress(int num,string text=null)
        {
            _progress = num;
            _lastText = text ?? "";
        }
        string _lastText = "";


        public void Dispose()
        {
            _dispose();
        }
        public static void ReadAllRowsWithProgress(Firefly.Box.UIController task, string title, Action what)
        {
            if (ENV.UserSettings.DoNotDisplayUI)
            {
                task.ReadAllRows(what);
                return;
            }
            int i = 0;
            var start = DateTime.Now;
            ShowProgressInNewThread p = null;
            task.ReadAllRows(() =>
                                 {
                                     i++;
                                     what();
                                     if (p == null && (DateTime.Now - start).TotalSeconds > 3)
                                     {
                                         p = new ShowProgressInNewThread(title);
                                     }
                                     if (p != null)
                                     {
                                         p.Progress(i);
                                         if (p.Cancel)
                                             throw new FlowAbortException();

                                     }
                                 });
            if (p != null)
                p.Dispose();
        }
        public static void ReadAllRowsWithProgress(Firefly.Box.BusinessProcess task, string title, Action what)
        {
            if (ENV.UserSettings.DoNotDisplayUI)
            {
                task.ReadAllRows(what);
                return;
            }
            int i = 0;
            var start = DateTime.Now;
            ShowProgressInNewThread p = null;
            task.ReadAllRows(() =>
            {
                i++;
                what();
                if (p == null && (DateTime.Now - start).TotalSeconds > 3)
                {
                    p = new ShowProgressInNewThread(title);
                }
                if (p != null)
                {
                    p.Progress(i);
                    if (p.Cancel)
                        throw new FlowAbortException();

                }
            });
            if (p != null)
                p.Dispose();
        }
    }
}
