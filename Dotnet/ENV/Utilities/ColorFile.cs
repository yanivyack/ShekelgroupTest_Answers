using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Firefly.Box;
using Firefly.Box.UI;

namespace ENV.Utilities
{
    public class ColorFile
    {
        static string _fileName;
        public static string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                _map = null;
            }
        }

        static object _lock = new object();
        public static Dictionary<int, ColorScheme> _map = null;
        public static void LoadSettingFromFile(int index, ColorScheme colorScheme)
        {
            ReadColorFileIfNotAlreadyReaden();
            ColorScheme result;
            if (_map.TryGetValue(index, out result))
            {
                colorScheme.ForeColor = result.ForeColor;
                colorScheme.BackColor = result.BackColor;
                colorScheme.TransparentBackground = result.TransparentBackground;
            }
            if (!_colorTypes.ContainsKey(index))
                _colorTypes.Add(index, colorScheme.GetType());
        }

        static void ReadColorFileIfNotAlreadyReaden()
        {
            if (_map == null)
            {
                lock (_lock)
                {
                    if (_map == null)
                    {
                        _map = new Dictionary<int, ColorScheme>();
                        try
                        {
                            string fileName = PathDecoder.DecodePath(_fileName);
                            if (File.Exists(fileName))
                            {
                                using (var sr = new StreamReader(fileName))
                                {
                                    string l = null;
                                    while ((l = sr.ReadLine()) != null)
                                    {
                                        var cs = new ColorScheme();
                                        string[] parts = l.Split(',');

                                        string colorType = "0";
                                        if (parts.Length > 3)
                                            colorType = parts[3];

                                        switch (colorType)
                                        {
                                            case "0":
                                                cs.BackColor = FromString(parts[2]);
                                                cs.ForeColor = FromString(parts[1]);
                                                break;
                                            case "1":
                                                cs.TransparentBackground = true;
                                                cs.BackColor = FromString(parts[2]);
                                                cs.ForeColor = FromString(parts[1]);
                                                break;
                                            case "2":
                                                cs.BackColor = FromString(parts[2]);
                                                cs.ForeColor = FromSystem(parts[1]);
                                                break;
                                            case "3":
                                                cs.TransparentBackground = true;
                                                cs.BackColor = FromString(parts[2]);
                                                cs.ForeColor = FromSystem(parts[1]);
                                                break;
                                            case "4":
                                                cs.BackColor = FromSystem(parts[2]);
                                                cs.ForeColor = FromString(parts[1]);
                                                break;
                                            case "6":
                                                cs.BackColor = FromSystem(parts[2]);
                                                cs.ForeColor = FromSystem(parts[1]);
                                                break;
                                            case "5":
                                                cs.TransparentBackground = true;
                                                cs.BackColor = FromSystem(parts[2]);
                                                cs.ForeColor = FromString(parts[1]);
                                                break;
                                            case "7":
                                                cs.TransparentBackground = true;
                                                cs.BackColor = FromSystem(parts[2]);
                                                cs.ForeColor = FromSystem(parts[1]);
                                                break;

                                            default:

                                                cs.BackColor = FromString(parts[2]);
                                                cs.ForeColor = FromString(parts[1]);
                                                break;
                                        }
                                        _map.Add(_map.Count + 1, cs);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            ErrorLog.WriteToLogFile(e);
                        }
                    }
                }
            }
        }

        public static Color FromString(string colorID)
        {
            colorID = colorID.PadLeft(8, '0');
            if (colorID == "FFFFFC18")
            {
                return Color.Transparent;
            }
            if (colorID.StartsWith("FF"))
            {
                return FromSystem(colorID);
            }
            else
            {
                int
                    b = int.Parse(colorID.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                    g = int.Parse(colorID.Substring(4, 2), System.Globalization.NumberStyles.HexNumber),
                    r = int.Parse(colorID.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);

                return
                    Color.FromArgb(r, g, b);
            }
        }
        static Dictionary<int, Type> _colorTypes = new Dictionary<int, Type>();
        public static void Map(int index, Type type)
        {
            if (!_colorTypes.ContainsKey(index))
                _colorTypes.Add(index, type);
        }

        static Color FromSystem(string hex)
        {
            switch (hex)
            {
                case "FFFFFFFF": return SystemColors.ScrollBar;
                case "FFFFFFFE": return SystemColors.Desktop;
                case "FFFFFFFD": return SystemColors.ActiveCaption;
                case "FFFFFFFC": return SystemColors.InactiveCaption;
                case "FFFFFFFB": return SystemColors.Menu;
                case "FFFFFFFA": return SystemColors.Window;
                case "FFFFFFF9": return SystemColors.WindowFrame;
                case "FFFFFFF8": return SystemColors.MenuText;
                case "FFFFFFF7": return SystemColors.WindowText;
                case "FFFFFFF6": return SystemColors.ActiveCaptionText;
                case "FFFFFFF5": return SystemColors.ActiveBorder;
                case "FFFFFFF4": return SystemColors.InactiveBorder;
                case "FFFFFFF3": return SystemColors.AppWorkspace;
                case "FFFFFFF2": return SystemColors.Highlight;
                case "FFFFFFF1": return SystemColors.HighlightText;
                case "FFFFFFF0": return SystemColors.ButtonFace;
                case "FFFFFFEF": return SystemColors.ButtonShadow;
                case "FFFFFFEE": return SystemColors.GrayText;
                case "FFFFFFEC": return SystemColors.InactiveCaptionText;
                case "FFFFFFEB": return SystemColors.HighlightText;
                case "FFFFFFE7": return SystemColors.Info;
                case "FFFFFFE8": return SystemColors.InfoText;
                case "FFFFFFED": return SystemColors.ControlText;


            }

            return SystemColors.WindowText;
        }
        static Dictionary<int, ColorScheme> _cache = new Dictionary<int, ColorScheme>();
        public static ColorScheme Find(Number index)
        {
            if (index == null || !_colorTypes.ContainsKey(index))
            {
                return new ColorScheme(SystemColors.ControlText, SystemColors.Control);
            }
            ColorScheme result;
            if (_cache.TryGetValue(index, out result))
                return result;
            result = (ColorScheme)System.Activator.CreateInstance(_colorTypes[index]);
            _cache.Add(index, result);
            return result;
        }
        static Color FromStringSet(string colorID)
        {
            if (colorID.Length < 8)
                colorID = colorID.PadLeft(8, 'F');
            int r = int.Parse(colorID.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                    g = int.Parse(colorID.Substring(4, 2), System.Globalization.NumberStyles.HexNumber),
                   b = int.Parse(colorID.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            return Color.FromArgb(r, g, b);
        }

        internal static Bool Set(Number colorIndex, Text forecolor, Text backcolor)
        {
            ReadColorFileIfNotAlreadyReaden();
            if (_cache.ContainsKey(colorIndex))
                _cache.Remove(colorIndex);
            ColorScheme cs;
            if (!_map.TryGetValue(colorIndex, out cs))
            {
                cs = new ColorScheme();
                _map.Add(colorIndex, cs);
            }
            cs.ForeColor = FromStringSet(forecolor);
            cs.BackColor = FromStringSet(backcolor);
            Type colorSchemeType;
            if (_colorTypes.TryGetValue(colorIndex,out colorSchemeType))

                RefreshColorScheme(Form.ActiveForm, colorSchemeType);
            return true;
        }
        static void RefreshColorScheme(System.Windows.Forms.Control control,Type colorSchemeType)
        
        {
            if (control == null)
                return;
            foreach (var item in control.GetType().GetProperties())
            {
                if (item.PropertyType == typeof(ColorScheme) && item.CanWrite)
                {
                    var val = item.GetValue(control, new object[0]);
                    if (val!=null&&val.GetType()==colorSchemeType)
                        item.SetValue(control, System.Activator.CreateInstance(colorSchemeType),new object[0]);
                }
            }
            foreach (System.Windows.Forms. Control item in control.Controls)
            {
                RefreshColorScheme(item, colorSchemeType);
            }
        }
    }
}
