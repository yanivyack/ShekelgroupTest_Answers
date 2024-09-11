using System;
using System.Drawing;
using ENV.Data;
using ENV.UI;
using Firefly.Box.UI;
using Firefly.Box.UI.Advanced;

namespace ENV.Labs
{
    public static class FaceLiftDemo
    {
        public static Color BackGroundColor =/* Color.LightSteelBlue;// */System.Drawing.Color.FromArgb(227, 233, 255);
        public static Color AlternatingBackColor = System.Drawing.Color.FromArgb(227, 233, 255);
        public static Color BorderColor = Color.Orange;
        public static bool Enabled { get; set; }

        public static void MatchBackColor(System.Windows.Forms.Form control)
        {
            if (!Enabled)
                return;
            if (IsStandardBackcolor(control.BackColor))
                control.BackColor = FaceLiftDemo.BackGroundColor;
        }
        public static void MatchBackColor(Firefly.Box.UI.Advanced.ControlBase control)
        {
            if (!Enabled)
                return;
            if (IsStandardBackcolor(control.BackColor))
                control.BackColor = FaceLiftDemo.BackGroundColor;
        }

        public static bool IsStandardBackcolor(Color backColor)
        {
            return backColor == System.Drawing.SystemColors.ButtonFace ||
                backColor == System.Drawing.SystemColors.MenuBar ||
                backColor == System.Drawing.SystemColors.Window ||
                   backColor.Equals(System.Drawing.Color.FromArgb(255, 192, 192, 192)) ||
                   backColor.Equals(System.Drawing.Color.FromArgb(255, 218, 222, 220)) ||
                   backColor.Equals(System.Drawing.Color.FromArgb(255, 255, 255, 255)) ||
                   backColor.Equals(System.Drawing.Color.FromArgb(255, 212, 208, 200)) ||
                   backColor.Equals(System.Drawing.Color.FromArgb(255, 205, 205, 205));
        }

        public static Action<ENV.UI.Grid> GridLoaded = (ENV.UI.Grid grid) =>
          {
              if (FaceLiftDemo.Enabled && grid.HeaderHeight > 0)
              {
                  grid.UseVisualStyles = true;
                  grid.AlternatingBackColor = ENV.Labs.FaceLiftDemo.AlternatingBackColor;
                  grid.RowColorStyle = Firefly.Box.UI.GridRowColorStyle.AlternatingRowBackColor;
                  grid.DoubleColumnSeparatorInFlatStyle = false;
                  grid.UnderConstructionNewGridLook = true;
                  grid.EnableGridEnhancementsCodeSample = true;
                  grid.BackColor = SystemColors.Window;
                  if (grid.HeaderHeight > 2)
                  {
                      foreach (var control in grid.Controls)
                      {
                          var gc = control as ENV.UI.GridColumn;
                          if (gc != null)
                          {
                              Rectangle foundRect = Rectangle.Empty;
                              Action found = () => { };
                              var r = new Rectangle(gc.Left + grid.Left, grid.Top, gc.Width, grid.HeaderHeight);
                              foreach (var c in grid.Parent.Controls)
                              {
                                  gc.ForeColor = Color.Black;
                                  {
                                      var l = c as Firefly.Box.UI.Label;
                                      if (l != null && l.Visible && l.Bounds.IntersectsWith(r))
                                      {
                                          var inter = l.Bounds;
                                          inter.Intersect(r);
                                          if (inter.Width * inter.Height > foundRect.Width * foundRect.Height)
                                          {
                                              foundRect = inter;
                                              found = () =>
                                              {
                                          gc.Text = l.Text;
                                          gc.Font = l.Font;
                                          l.Visible = false;
                                              };
                                          }

                                      }
                                  }
                                  {
                                      var l = c as Firefly.Box.UI.TextBox;
                                      if (l != null && l.Bounds.IntersectsWith(r))
                                      {
                                          var inter = l.Bounds;
                                          inter.Intersect(r);
                                          if (inter.Width * inter.Height > foundRect.Width * foundRect.Height)
                                          {
                                              foundRect = inter;
                                              found = () =>
                                              {
                                          gc.BindText += (s, e) => e.Value = l.Data.Value.ToString();
                                          gc.Font = l.Font;
                                          l.Visible = false;
                                              };
                                          }
                                      }
                                  }
                              }
                              found();
                          }
                      }
                  }

              }
          };

        public static ColorScheme MatchColorScheme(ColorScheme value)
        {
            if (FaceLiftDemo.Enabled)
                if (IsStandardBackcolor(value.BackColor))
                    return new Firefly.Box.UI.ColorScheme(value.ForeColor, FaceLiftDemo.BackGroundColor);
            return value;
        }

        public static FontScheme MatchFontScheme(FontScheme value)
        {
            if (FaceLiftDemo.Enabled && value != null)
            {
                if (value.Font.Name == "System")
                    value = new FontScheme()
                    {
                        Font =
                                    new Font("Arial", value.Font.Size, value.Font.Style, value.Font.Unit, value.Font.GdiCharSet),
                        TextAngle = value.TextAngle
                    };

                else if (value.Font.Name == "Aharoni")
                    value = new FontScheme()
                    {
                        Font =
                                    new Font("Arial", value.Font.Size - 2, value.Font.Style, value.Font.Unit, value.Font.GdiCharSet),
                        TextAngle = value.TextAngle
                    };

                var specificFontScalingFactor = 0;
                if (int.TryParse(UserMethods.Instance.IniGet("[FontScaling]" + value.Font.OriginalFontName), out specificFontScalingFactor))
                    return new FontScheme()
                    {
                        Font = new Font(value.Font.OriginalFontName, (int)(value.Font.Size * specificFontScalingFactor / 100), value.Font.Style, value.Font.Unit, value.Font.GdiCharSet),
                        TextAngle = value.TextAngle
                    };
            }
            return ENV.UI.Form.MatchFont(value);
        }

        public static void Scale()
        {
            var horizontal = new NumberColumn("Horizontal Factor", "3%") { Value = ENV.UI.Form.ScalingFactor.Width * 100 };
            var vertical = new NumberColumn("Vertical Factor", "3%") { Value = ENV.UI.Form.ScalingFactor.Height * 100 };

            var fb = new FormBuilder("Form Factors");
            fb.AddColumn(horizontal);
            fb.AddColumn(vertical);
            fb.AddAction("OK", () =>
            {
                fb.Close();
                ENV.UI.Form.ScalingFactor = new SizeF(((float)horizontal) / 100, ((float)vertical) / 100);
            });
            fb.Run();

        }
    }
}
