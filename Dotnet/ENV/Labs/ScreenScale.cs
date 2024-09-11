using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Firefly.Box;
using ENV.Data;
using ENV;
using iTextSharp.text.pdf;

namespace ENV.Labs
{
    public class ScreenScale : UIControllerBase
    {
        static private Rectangle CurrentResolution = Screen.PrimaryScreen.Bounds;

        public TextColumn SourceRes = new TextColumn {DefaultValue = "800 x 600"};
        public TextColumn TargetRes = new TextColumn{ DefaultValue = "1024 x 768"};
        public NumberColumn XFactor = new NumberColumn("","4%");
        public NumberColumn YFactor = new NumberColumn("","4%");

        public ScreenScale()
        {
            Columns.Add(SourceRes, TargetRes, XFactor, YFactor);
            XFactor.BindValue(() => (ExtractWidth(TargetRes) / ExtractWidth(SourceRes)) * 100);
            YFactor.BindValue(() => (ExtractHeight(TargetRes) / ExtractHeight(SourceRes)) * 100);
        }

        static double ExtractHeight(string resolution)
        {
            return double.Parse(resolution.Substring(resolution.IndexOf('x') + 1));
        }

        static double ExtractWidth(string resolution)
        {
            return double.Parse(resolution.Substring(0, resolution.IndexOf('x') - 1));
        }

        public void Run()
        {

            Execute();
        }

        protected override void OnLoad()
        {
            View = () => new UI.ScreenScaleView(this);
        }

        public void Scale()
        {
            ENV.UI.Form.ScalingFactor = new SizeF(XFactor/100, YFactor/100);
            Exit();
        }
        
        public static void Show()
        {
            ENV.UI.Form.ToggleScalingDemo();
            


         //   new ScreenScale().Run();
        }

        internal int GetOriginalHeight()
        {
            return (int) (ExtractHeight(SourceRes)/6);
        }

        internal int GetOriginalWidth()
        {
            return (int) (ExtractWidth(SourceRes)/8);
        }

        internal int GetNewHeight()
        {
            return (int)(ExtractHeight(TargetRes) / 6);
        }

        internal int GetNewWidth()
        {
            return (int)(ExtractWidth(TargetRes) / 8);
        }
    }
}