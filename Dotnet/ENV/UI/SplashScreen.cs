using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ENV.UI
{
    public partial class SplashScreen : System.Windows.Forms.Form
    {
        public SplashScreen()
        {
            InitializeComponent();
            label1.Text = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
        }
        static SplashScreen _splash;
        static long _start;
        public static void Start()
        {
            return;
            _start = System.Environment.TickCount;
            var t = new System.Threading.Thread(() =>
            {
                System.Windows.Forms.Application.Run(_splash = new SplashScreen());
            })
            {
                IsBackground = true,
                ApartmentState = System.Threading.ApartmentState.STA
            };
            t.Start();
        }
        public static void Stop()
        {
            if (_splash != null)
            {
                Action x = () => _splash.Close();
                if (_splash.InvokeRequired)
                    _splash.Invoke(x);
                else
                    x();
                Common.RunOnRootMDI(form => form.Activate());
            }
      //      MessageBox.Show(((System.Environment.TickCount - _start)).ToString("0,000"));

        }
    }
}
