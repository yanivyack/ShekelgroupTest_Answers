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
    public class FontFile
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
        public static Dictionary<int, FontScheme> _map = null;
        public static Dictionary<int, FontScheme> _uiMap = null;
        public static void LoadSettingFromFile(int index, FontScheme fontScheme)
        {
            ReadColorFileIfNotAlreadyReaden();
            FontScheme result;
            if (_map.TryGetValue(index, out result))
            {
                fontScheme.Font = result.Font;
                fontScheme.TextAngle = result.TextAngle;
            }
            if (_uiMap != null)
                if (_uiMap.TryGetValue(index, out result))
                {
                    var x = fontScheme as ENV.UI.LoadableFontScheme;
                    if (x != null)
                    {
                        x.SavePrintingFont();
                    }
                    fontScheme.Font = result.Font;
                    fontScheme.TextAngle = result.TextAngle;
                }
        }
        internal class Font
        {
            public bool RequiresComponent()
            {
                return false;
            }
            string _name;
            string _fontName;


            float _fontSize;
            int _direction;
            int _gdiCharSet;

            string _fontInternalName;

            public Font(string lineFromExport)
            {
                string[] parts = lineFromExport.Split(',');
                _name = parts[0].TrimEnd();
                _fontName = parts[1];
                _fontSize = float.Parse(parts[2]);
                _gdiCharSet = int.Parse(parts[3]);
                _direction = int.Parse(parts[4]) / 10;

                _fontInternalName = _fontName + " " + _fontSize.ToString();
                for (int i = 5; i < parts.Length; i++)
                {
                    switch (parts[i])
                    {
                        case "Bold":
                            AddToStyle(System.Drawing.FontStyle.Bold);
                            _fontInternalName += " bold";
                            break;
                        case "Italic":
                            AddToStyle(System.Drawing.FontStyle.Italic);
                            _fontInternalName += " italic";
                            break;
                        case "Strike":
                            AddToStyle(System.Drawing.FontStyle.Strikeout);
                            _fontInternalName += " strike";
                            break;
                        case "Underline":
                            AddToStyle(System.Drawing.FontStyle.Underline);
                            _fontInternalName += " underline";
                            break;
                    }
                }

                if (_fontStyle == 0)
                    AddToStyle(System.Drawing.FontStyle.Regular);
            }


            System.Drawing.FontStyle _fontStyle = 0;
            private void AddToStyle(System.Drawing.FontStyle fontStyle)
            {
                if (_fontStyle == 0)
                {

                    _fontStyle = fontStyle;
                }
                else
                {
                    _fontStyle = _fontStyle | fontStyle;
                }
            }

            public FontScheme GetScheme()
            {
                return new FontScheme()
                {
                    Font =
                               new System.Drawing.Font(_fontName, _fontSize, _fontStyle, GraphicsUnit.Point, (byte)_gdiCharSet),
                    TextAngle = _direction
                };
            }


        }

        static void ReadColorFileIfNotAlreadyReaden()
        {
            if (_map == null)
            {
                lock (_lock)
                {
                    if (_map == null)
                    {
                        _map = new Dictionary<int, FontScheme>();
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
                                        var cs = new Font(l).GetScheme();
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

        internal static Bool Set(Number index, Text fontName, Number size, Number scriptCode, Number orientation, Bool bold, Bool italic, Bool strike, Bool underline)
        {
            if (_uiMap == null)
                _uiMap = new Dictionary<int, FontScheme>();
            FontScheme fc;
            if (!_uiMap.TryGetValue(index, out fc))
            {
                _uiMap.Add(index, fc = new FontScheme());
            }

            fc.TextAngle = orientation;
            System.Drawing.FontStyle fontStyle = 0;
            Action<FontStyle> set = fs =>
            {
                if (fontStyle == 0)
                    fontStyle = fs;
                else
                    fontStyle = fontStyle | fs;
            };
            if (bold)
                set(FontStyle.Bold);
            if (italic)
                set(FontStyle.Italic);
            if (strike)
                set(FontStyle.Strikeout);
            if (underline)
                set(FontStyle.Underline);
            if (fontStyle == 0)
                fontStyle = FontStyle.Regular;
            fc.Font = new System.Drawing.Font(fontName.Trim(), (float)size, fontStyle, GraphicsUnit.Point, (byte)scriptCode);

            return true;


        }
    }
}
namespace ENV.UI
{
    public class LoadableFontScheme : Firefly.Box.UI.FontScheme
    {

        FontScheme _printingVersion;

        internal static FontScheme GetPrintingFont(FontScheme value)
        {
            var x = value as LoadableFontScheme;
            if (x != null)
                return x.GetPrintingVersion();
            return value;
        }

        internal FontScheme GetPrintingVersion()
        {
            if (_printingVersion != null)
            {
                return _printingVersion;
            }
            return this;
        }

        internal void SavePrintingFont()
        {
            _printingVersion = new FontScheme()
            {
                Font = Font,
                TextAngle = TextAngle
            };
        }
    }
}
