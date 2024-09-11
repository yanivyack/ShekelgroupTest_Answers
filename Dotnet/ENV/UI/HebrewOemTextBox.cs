using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ENV.Data.Storage;
using Firefly.Box;

namespace ENV.UI
{
    public class HebrewOemTextBox : TextBox
    {
        static Dictionary<char, char> _translateBack = new Dictionary<char, char>();

        private static int[] _newChars =
        {
            8364, 129, 8218, 402, 8222, 8230, 8224, 8225, 710, 8240, 352, 8249, 338, 141,
            381, 143, 144, 8216, 8217, 8220, 8221, 8226, 8211, 8212, 732, 8482, 353
        };

        private bool _reversedEditingEnabledForTesting;
        private int _maxCharsInLineForTesting = -1;
        private bool _selectionStartTestingEnabled;
        private int _selectionStartForTesting = 0;
        const int _alef = 'א';

        static HebrewOemTextBox()
        {
            for (int i = 0; i < _newChars.Length; i++)
            {
                _translateBack.Add((char)_newChars[i], (char)(_alef + i));
            }
        }

        public HebrewOemTextBox()
        {
            Enter += () =>
            {
                if (Multiline)
                {
                    Action onEnter = () =>
                    {
                        if (IsDisposed)
                            return;
                        SelectionLength = 0;
                        SetSelectionStart(ConvertToSelectionStart(0));
                    };
                    if (_reversedEditingEnabledForTesting)
                        Context.Current.BeginInvoke(onEnter);
                    else if (IsHandleCreated)
                        BeginInvoke(onEnter);
                }
            };
        }

        internal void SetSelectionStart(int value)
        {
            SelectionStart = _selectionStartForTesting = value;
        }

        internal int GetSelectionStart()
        {
            if (_selectionStartTestingEnabled)
                return _selectionStartForTesting;
            return SelectionStart;
        }

        public bool HebrewDosCompatibleEditing { get; set; }

        protected override bool KeepTrackingSpaces { get { return OemEncode(); } }

        bool OemEncode()
        {
            return HebrewDosCompatibleEditing && FormatHasH();
        }

        bool FormatHasH()
        {
            if (!string.IsNullOrEmpty(Format))
                return Format.Contains("H");
            var c = Data.Column;
            if (c != null && !string.IsNullOrEmpty(c.Format))
                return c.Format.Contains("H");
            return false;
        }

        protected override string TranslateData(string text)
        {
            var translatedData = base.TranslateData(text);
            if (OemEncode())
            {
                IgnoreInputMask = true;
                translatedData = TranslateToDisplay(translatedData, FormatHasH() && !Multiline);
                if (FormatHasH() && Multiline)
                    translatedData = ToFilppedMultiline(translatedData);
            }
            else
            {
                translatedData = HebrewOemTextStorage.Decode(translatedData);
                if (EncodeLatin())
                    translatedData =
                        Encoding.GetEncoding(1252).GetString(Encoding.GetEncoding(1255).GetBytes(translatedData));
            }
            return translatedData;
        }

        internal static string TranslateToDisplay(string data, bool flip)
        {
            var charArray = data.ToCharArray();
            var result = new char[charArray.Length];
            for (int i = 0; i < result.Length; i++)
            {
                int c = charArray[i];
                if (c >= _alef && c < _alef + _newChars.Length)
                {
                    c = _newChars[c - _alef];
                }
                result[flip ? charArray.Length - 1 - i : i] = (char) c;
            }
            return new string(result);
        }

        internal string ToFilppedMultiline(string text)
        {
            if (text.Length <= GetMaxCharsInLine())
                return Reverse(text);

            var lines = new List<string>();
            string line = "";

            var words = text.Split(' ');
            for (var i = 0; i < words.Length; i++)
            {
                string word = words[i];

                if (i < words.Length - 1)
                    word += " ";

                bool retryword;
                do
                {
                    retryword = false;
                    if (line.Length > 0)
                    {
                        if (line.Length + word.Length <= GetMaxCharsInLine())
                            line += word;
                        else
                        {
                            lines.Add(Reverse(line));
                            line = "";
                            retryword = true;
                        }
                    }
                    else
                    {
                        if (word.Length <= GetMaxCharsInLine())
                            line = word;
                        else
                        {
                            lines.Add(Reverse(word.Substring(0, GetMaxCharsInLine())));
                            word = word.Substring(GetMaxCharsInLine());
                            retryword = true;
                        }
                    }
                } while (retryword);
            }

            if (line.Length > 0)
                lines.Add(Reverse(line));

            return string.Join("\r\n", lines.ToArray());
        }

        static string Reverse(string text)
        {
            char[] charArray = text.ToCharArray();
            Array.Reverse(charArray);
            return new String(charArray);
        }

        int GetMaxCharsInLine()
        {
            if (_maxCharsInLineForTesting != -1)
                return _maxCharsInLineForTesting;

            var form = FindForm() as Form;
            var avgCharWidth = form != null ? form.GetAverageCharWidth(Font) : (Font != null ? Font.Height / 2 : 5);
            return Width / avgCharWidth - 1;
        }

        bool EncodeLatin()
        {
            return Font.GdiCharSet != 177 && Font.GdiCharSet != 255;
        }

        protected override string TranslateUserInput(string text)
        {
            var s = base.TranslateUserInput(text);
            if (OemEncode())
                s = TranslateFromDisplay(s);
            else if (EncodeLatin())
                s = Encoding.GetEncoding(1255).GetString(Encoding.GetEncoding(1252).GetBytes(s));
            return s;
        }

        string TranslateFromDisplay(string text)
        {
            var flip = FormatHasH();
            if (flip && Multiline)
            {
                text = FromFlippedMultiline(text);
                flip = false;
            }
            var y = text.ToCharArray();
            var x = new char[y.Length];
            for (int i = 0; i < x.Length; i++)
            {
                char c = y[i];
                char z;
                if (_translateBack.TryGetValue(c, out z))
                    c = z;

                x[flip ? y.Length - 1 - i : i] = c;
            }
            return new string(x);
        }

        internal static string FromFlippedMultiline(string text)
        {
            var lines = text.Split(new[] {"\r\n"}, StringSplitOptions.None);
            return string.Join("", lines.Select(Reverse).ToArray());
        }

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [Serializable, System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)
        ]
        struct MSG
        {
            public IntPtr hwnd;
            public int message;
            public IntPtr wParam;
            public IntPtr lParam;
            public int time;
            public int pt_x;
            public int pt_y;
        }

        [DllImport("user32.dll")]
        static extern bool PeekMessage([In, Out] ref MSG msg, IntPtr hwnd, int msgMin, int msgMax, int remove);

        protected override bool SkipControllerHandlersForKey(Keys keys)
        {
            if (!ReversedEditing()) return base.SkipControllerHandlersForKey(keys);

            if (keys == Keys.Left) return true;
            if (keys == Keys.Right) return true;
            if (keys == (Keys.Shift | Keys.Left)) return true;
            if (keys == (Keys.Shift | Keys.Right)) return true;
            if (keys == (Keys.Control | Keys.Left)) return true;
            if (keys == (Keys.Control | Keys.Right)) return true;
            if (keys == (Keys.Shift | Keys.End)) return true;
            if (keys == (Keys.Shift | Keys.Home)) return true;
            if (keys == Keys.End) return true;
            if (keys == Keys.Home) return true;
            if (keys == Keys.Delete) return true;
            if (keys == Keys.Back) return true;

            return base.SkipControllerHandlersForKey(keys);
        }

        bool ReversedEditing()
        {
            return (OemEncode() && InputLanguage.CurrentInputLanguage.Culture.TextInfo.IsRightToLeft) ||
                   _reversedEditingEnabledForTesting;
        }

        protected override bool ProcessKeyPreview(ref System.Windows.Forms.Message m)
        {
            if (ReversedEditing())
            {
                if (m.Msg == 256)
                {
                    if (Multiline)
                    {
                        if ((Keys) (long) m.WParam == Keys.Home)
                        {
                            SetSelectionStart(ConvertToSelectionStart(0));
                            return true;
                        }
                        var lastPositionFromRight = Text.LastIndexOf("\n") + 1;
                        if ((Keys) (long) m.WParam == Keys.End)
                        {
                            SetSelectionStart(lastPositionFromRight);
                            return true;
                        }
                        if ((Keys) (long) m.WParam == Keys.Delete)
                        {
                            if (GetSelectionStart() != lastPositionFromRight)
                            {
                                var countCharsFromRight = CountCharsFromRight(Text, GetSelectionStart());
                                if (countCharsFromRight < Text.Length)
                                {
                                    Text =
                                        ToFilppedMultiline(
                                            FromFlippedMultiline(Text.Remove(Math.Max(GetSelectionStart() - 1, 0), 1)));
                                    SetSelectionStart(ConvertToSelectionStart(countCharsFromRight));
                                }
                            }
                            return true;
                        }
                        if ((Keys)(long)m.WParam == Keys.Right)
                        {
                            if (GetSelectionStart() != ConvertToSelectionStart(0))
                                SetSelectionStart(ConvertToSelectionStart(CountCharsFromRight(Text, GetSelectionStart()) - 1));
                            return true;
                        }
                        if ((Keys)(long)m.WParam == Keys.Left)
                        {
                            if (GetSelectionStart() != lastPositionFromRight)
                                SetSelectionStart(ConvertToSelectionStart(CountCharsFromRight(Text, GetSelectionStart()) + 1));
                            return true;
                        }
                    }
                    else
                    {
                        if ((Keys) (long) m.WParam == Keys.End)
                            m.WParam = (IntPtr) Keys.Home;
                        else if ((Keys) (long) m.WParam == Keys.Home)
                            m.WParam = (IntPtr) Keys.End;
                        else if ((Keys) (long) m.WParam == Keys.Delete)
                        {
                            m.WParam = (IntPtr) Keys.Back;
                            PostMessage(m.HWnd, 258, new IntPtr('\b'), IntPtr.Zero);
                        }
                        else if ((Keys) (long) m.WParam == Keys.Back)
                        {
                            m.WParam = (IntPtr) Keys.Delete;
                            var m1 = new MSG();
                            do
                                ; while (PeekMessage(ref m1, m.HWnd, 258, 258, 1));
                        }
                    }
                }
            }
            return base.ProcessKeyPreview(ref m);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (ReversedEditing())
            {
                var originalSelectionStart = GetSelectionStart();
                var originalText = Text;
                Action returnToSelection =
                    () =>
                    {
                        if (IsDisposed || ReadOnly) return;
                        SetSelectionStart(originalSelectionStart);
                    };

                if (Multiline)
                {
                    returnToSelection =
                        () =>
                        {
                            if (IsDisposed || ReadOnly)
                                return;

                            if (e.KeyChar == '\b')
                            {
                                var countCharsFromRight = CountCharsFromRight(originalText, originalSelectionStart);
                                if (countCharsFromRight > 0)
                                {
                                    Text =
                                        ToFilppedMultiline(
                                            FromFlippedMultiline(originalText.Remove(originalSelectionStart, 1)));
                                    SetSelectionStart(ConvertToSelectionStart(countCharsFromRight - 1));
                                }
                                else
                                {
                                    Text = originalText;
                                    SetSelectionStart(originalSelectionStart);
                                }
                            }
                            else if (!char.IsControl(e.KeyChar) || IsSpecialHebrewKey(e.KeyChar))
                            {
                                var countCharsFromRight = CountCharsFromRight(originalText, originalSelectionStart);
                                Text = ToFilppedMultiline(FromFlippedMultiline(Text));
                                SetSelectionStart(ConvertToSelectionStart(countCharsFromRight + 1));
                            }
                            SelectionLength = 0;
                        };
                }

                if (_reversedEditingEnabledForTesting)
                    Context.Current.BeginInvoke(returnToSelection);
                else if (IsHandleCreated)
                    BeginInvoke(returnToSelection);
            }
            if (OemEncode())
            {
                var c = (int) e.KeyChar;

                if (c >= _alef && c < _alef + _newChars.Length)
                {
                    e.KeyChar = (char) _newChars[c - _alef];
                }
            }
            else if (EncodeLatin())
            {
                e.KeyChar =
                    Encoding.GetEncoding(1252).GetChars(Encoding.GetEncoding(1255).GetBytes(new[] {e.KeyChar}))[0];
            }
            base.OnKeyPress(e);
        }

        private bool IsSpecialHebrewKey(char keyChar)
        {
            return keyChar == 129 || keyChar == 141 || keyChar == 143 || keyChar == 144;
        }

        private static int CountCharsFromRight(string originalText, int selectionStart)
        {
            int counter = 0;
            foreach (var line in originalText.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries))
            {
                if (counter > 0)
                {
                    selectionStart -= "\r\n".Length;
                }

                if (selectionStart > line.Length)
                {
                    counter += line.Length;
                    selectionStart -= line.Length;
                }
                else
                {
                    counter += line.Length - selectionStart;
                    break;
                }
            }
            return counter;
        }

        private int ConvertToSelectionStart(int countCharsFromRight)
        {
            int selectionStart = 0;
            var lines = Text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (selectionStart > 0)
                {
                    selectionStart += "\r\n".Length;
                }

                if (countCharsFromRight > line.Length)
                {
                    selectionStart += line.Length;
                    countCharsFromRight -= line.Length;
                }
                else
                {
                    selectionStart += line.Length - countCharsFromRight;
                    break;
                }
            }
            return selectionStart;
        }

        protected override void OnLoad()
        {
            if (OemEncode())
            {
                IgnoreInputMask = true;
                ForceLeftToRightEditing = true;
            }
            base.OnLoad();
        }

        public void PressKeyForTesting(char keyChar)
        {
            OnKeyPress(new KeyPressEventArgs(keyChar));
        }

        internal void EnableReversedEditingForTesting()
        {
            _reversedEditingEnabledForTesting = true;
        }

        internal void SetMaxCharsInLineForTesting(int maxCharsInLine)
        {
            _maxCharsInLineForTesting = maxCharsInLine;
        }

        internal void EnableSelectionStartTesting()
        {
            _selectionStartTestingEnabled = true;
        }
    }

    public class HebrewOemComboBox : ComboBox
    {
        protected override string Translate(string term)
        {
            return HebrewOemTextStorage.Decode(term);
        }
    }
}