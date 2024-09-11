using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ENV.Advanced;
using ENV.Data;
using ENV.Labs;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using Firefly.Box.UI;
using Firefly.Box.UI.Advanced;

namespace ENV.UI
{
    public class TextBox : Firefly.Box.UI.TextBox, ICanShowCustomHelp
    {
        ENV.Utilities.ControlHelper _helper;

        [DefaultValue(false)]
        public bool AutoExpand
        {
            get { return _helper.AutoExpand; }
            set { _helper.AutoExpand = value; }
        }
        [Obsolete("Replaced with ForceLeftAlignOnEditing")]
        public bool ForceLeftAlignedEditingOfNumericData { set { ForceLeftAlignOnEditing = value; } }

        [DefaultValue(false)]
        public bool MustInput { get; set; }

        public event BindingEventHandler<BooleanBindingEventArgs> BindMustInput
        {
            add { AddBindingEvent("MustInput", () => MustInput, x => MustInput = x, value); }
            remove { RemoveBindingEvent("MustInput", value); }
        }
        static bool AlwaysShowExpandButton = false;
        public static void ToggleExpandButton()
        {
            AlwaysShowExpandButton = !AlwaysShowExpandButton;
        }
        protected T Create<T>()
        {
            return AbstractFactory.Create<T>();
        }

        /// <summary>CheckBox</summary>
        public TextBox()
        {
            AllowLeaveUsingArrowsKeysInMultiline = UserSettings.AllowLeaveTextboxUsingArrowKeysInMultiline;
            if (ENV.JapaneseMethods.Enabled)
                AllowLeaveUsingEnterKeysInMultiline = false;

            _helper = new ENV.Utilities.ControlHelper(this);
            bool textChangedSinceEnter = false;
            bool inControl = false;
            base.Enter += () =>
            {
                _helper.ControlEnter(Enter);
                textChangedSinceEnter = false;
                inControl = true;
            };

            base.Leave += () =>
            {
                _helper.ControlLeave(Leave);
                inControl = false;
            };
            base.Change += () =>
            {
                _helper.ControlChange(Change);
                textChangedSinceEnter = true;
            };
            base.InputValidation += () =>
            {
                if (MustInput && inControl && !textChangedSinceEnter &&
                    !ENV.UserMethods.Instance.Stat(0, "Q") && !ReadOnly)
                    Common.ErrorInStatusBar(LocalizationInfo.Current.ControlMustBeUpdated);
                _helper.ControlInputValidation(InputValidation);
            };

            this.InvalidChar += c => Common.SetTemporaryStatusMessage(string.Format(LocalizationInfo.Current.InvalidChar, c));
            this.InvalidInput += c =>
            {
                ColumnBase col = null;
                if (this.Data != null && this.Data.Column != null)
                    col = this.Data.Column;
                {
                    if (DiscardPendingCommandsOnInvalidInput)
                        Context.Current.DiscardPendingCommands();
                    if (!string.IsNullOrEmpty(col.InputRange) &&
                        col.InputRange.Trim().Length > 0)
                        Common.SetTemporaryStatusMessage(string.Format(LocalizationInfo.Current.InputDoesntMatchRange, c,
                            col.InputRange));
                    else
                    {
                        var format = this.Format;
                        if (string.IsNullOrEmpty(format))
                            format = col.Format;
                        if (col is Firefly.Box.Data.DateColumn)
                        {
                            Common.SetTemporaryStatusMessage(LocalizationInfo.Current.InvalidDate);
                            return;
                        }
                        var nc = col as Firefly.Box.Data.NumberColumn;
                        if (nc != null)
                        {

                            var nfi = new NumberFormatInfo(format);
                            if (nfi.DecimalDigits > 0)
                                format = string.Format("{0}.{1} " + LocalizationInfo.Current.Digits, nfi.WholeDigits,
                                    nfi.DecimalDigits);
                            else
                                format = nfi.WholeDigits + " " + LocalizationInfo.Current.Digits;
                        }
                        Common.SetTemporaryStatusMessage(string.Format(LocalizationInfo.Current.InvalidValue, c,
                            format));
                    }
                }
            };
            LengthExceeded += () =>
            {
                System.Media.SystemSounds.Beep.Play();
            };

        }

        [Browsable(false)]
        public bool DiscardPendingCommandsOnInvalidInput { get; set; }
      

        public void ClearExpandEvent()
        {
            _helper.ClearExpandEvent();
        }
        public override string Format
        {
            get
            {
                return base.Format;
            }
            set
            {
                if (value == null && Data != null && (Data.Column is NumberColumn || Data.Value is Number))
                    value = "100";
                base.Format = value;
            }
        }
        public override event System.Action Enter;
        public override event System.Action Leave;
        public override event System.Action Change;
        public override event System.Action InputValidation;

        public new event System.Action Expand
        {
            add { _helper.Expand += value; }

            remove { _helper.Expand -= value; }
        }
        /// <summary>
        /// Only used for the UserMethods.ControlSelectProgram method - backward compatability only
        /// </summary>
        public Type ExpandClassType { set { _helper.ExpandClassType = value; } get { return _helper.ExpandClassType; } }
        public AfterExpandGoToNextControlOptions AfterExpandGoToNextControl { get { return _helper.AfterExpandGoToNextControl; } set { _helper.AfterExpandGoToNextControl = value; } }

        public string StatusTip
        {
            get { return _helper.StatusTip; }
            set { _helper.StatusTip = value; }
        }

        public CustomHelp CustomHelp { get; set; }

        protected override void OnLoad()
        {
            _originalStyle = Style;
            if (AlwaysShowExpandButton)
                ShowExpandButton = true;
            if (ENV.Labs.FaceLiftDemo.Enabled)
            {

                if (Parent is Form)
                {
                    if (Style != Firefly.Box.UI.ControlStyle.Flat)
                    {
                        Style = Firefly.Box.UI.ControlStyle.Standard;
                    }

                }
                _onGrid = Parent is Firefly.Box.UI.GridColumn;
                if (_origColorScheme != null)
                    ColorScheme = _origColorScheme;
            }
            base.OnLoad();

            if (FocusedBackColor != Color.Empty || FocusedReadOnlyBackColor != Color.Empty)
                FocusedForeColor = SystemColors.ControlText;
        }
        bool _onGrid = false;
        ControlStyle _originalStyle;
        internal ControlStyle GetOriginalStyle()
        {
            return _originalStyle;
        }
        Firefly.Box.UI.ColorScheme _origColorScheme;

        public override Firefly.Box.UI.ColorScheme ColorScheme
        {
            get
            {
                return base.ColorScheme;
            }
            set
            {

                _origColorScheme = value;
                if (FaceLiftDemo.Enabled && FaceLiftDemo.IsStandardBackcolor(value.BackColor))
                {


                    if (Style == Firefly.Box.UI.ControlStyle.Flat && _onGrid)
                        value = new Firefly.Box.UI.ColorScheme(value.ForeColor, Color.Transparent) { TransparentBackground = true };
                    else
                    {
                        value = new Firefly.Box.UI.ColorScheme(value.ForeColor, System.Drawing.SystemColors.Window);
                    }
                }


                base.ColorScheme = value;
            }

        }
        public override Firefly.Box.UI.FontScheme FontScheme
        {
            get
            {
                return base.FontScheme;
            }
            set
            {
                base.FontScheme = FaceLiftDemo.MatchFontScheme(value);
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {

            if (ENV.JapaneseMethods.Enabled)
                if (!Char.IsControl(e.KeyChar))
                {
                    var tc = Data.Column as TextColumn;
                    if (tc != null)
                    {
                        var f = tc.FormatInfo;
                        if (!string.IsNullOrEmpty(Format))
                            f = new TextFormatInfo(Format);
                        if (f.MaxDataLength == f.MaxLength)
                        {
                            if (SelectionLength == 0)
                            {
                                if (JapaneseMethods.GetTextByteLength(Text + e.KeyChar) > f.MaxDataLength)
                                    e.Handled = true;
                            }
                        }
                    }
                    else
                    {
                        var nc = Data.Column as NumberColumn;
                        if (nc != null)
                        {
                            if (char.IsDigit(e.KeyChar) && SelectionLength == 0 && SelectionStart == Text.Length)
                            {
                                int digits = 0;
                                bool foundDecimalPoint = false;
                                foreach (var c in Text)
                                {
                                    if (char.IsDigit(c))
                                        digits++;
                                    else if (Number.DecimalSeparator.Value == c)
                                    {
                                        foundDecimalPoint = true;
                                        digits = 0;
                                    }
                                }
                                var fi = nc.FormatInfo;
                                if (!string.IsNullOrEmpty(Format))
                                    fi = new NumberFormatInfo(Format);
                                if (foundDecimalPoint)
                                    e.Handled = digits >= fi.DecimalDigits;
                                else
                                    e.Handled = digits >= fi.WholeDigits;
                            }
                        }

                    }

                }
        }

        protected override string TranslateUserInput(string text)
        {


            var nc = Data.Column as NumberColumn;
            if (nc != null&&nc.Value!=null)
            {
                var s = text.TrimEnd();
                if (s.Length > 1 && "+-*/".IndexOf(s[s.Length - 1]) >= 0)
                {
                    var fmt = string.IsNullOrEmpty(Format) ? nc.Format : Format;
                    if (fmt != null && (fmt.Contains("\\+") || fmt.Contains("\\-")||fmt.Contains("/")))
                        return text;
                    var s1 = Number.Parse(text, fmt).ToString(fmt).TrimEnd();
                    if (s1.Length > 1 && "+-*/".IndexOf(s1[s1.Length - 1]) >= 0)
                    {
                        s = s.Remove(s.Length - 1);
                        if (!(s.Length > 1 && "+-*/".IndexOf(s[s.Length - 1]) >= 0))
                            return text;
                    }

                    Number x = nc.Value;
                    Number y = Number.Parse(text.Substring(0, s.Length - 1), fmt);
                    if (y != 0)
                    {
                        Number result = x;
                        switch (s[s.Length - 1])
                        {
                            case '+':
                                result = x + y;
                                break;
                            case '-':
                                result = x - y;
                                break;
                            case '*':
                                result = x * y;
                                break;
                            case '/':
                                result = x / y;
                                break;
                        }
                        return result.ToString(fmt);
                    }

                }
            }
            if (RemoveNonAnsiCharsForAnsiColumns)
            {
                var tx = Data.Column as TextColumn;
                if (tx != null && tx.StorageType != TextStorageType.Unicode)
                {
                    var enc = LocalizationInfo.Current.OuterEncoding;
                    text = enc.GetString(enc.GetBytes(text));
                }
            }
            return text;
        }
        public bool RemoveNonAnsiCharsForAnsiColumns { get; set; }
        protected override string TranslateData(string text)
        {
            if (Data != null && Data.Column == null)
            {
                if (ENV.JapaneseMethods.Enabled &&this.Data.Value is Text )
                    text = JapaneseMethods.MatchForJapaneseLength(text, Format);
            }
            return base.TranslateData(text);
        }
        internal static string FixSymbolText(string s, FontScheme f)
        {
            if (f != null && f.Font != null && f.Font.GdiCharSet == 2)
            {
                var sb = new StringBuilder();
                foreach (var item in ENV.LocalizationInfo.Current.InnerEncoding.GetBytes(s))
                {
                    sb.Append((char)item);
                }
                return sb.ToString();
            }
            return s;
        }

        protected override string TranslatePaste(string text)
        {
            if (ENV.JapaneseMethods.Enabled)
            {
                var tc = Data.Column as TextColumn;
                if (tc != null)
                {
                    var f = tc.FormatInfo;
                    if (!string.IsNullOrEmpty(Format))
                        f = new TextFormatInfo(Format);
                    if (f.MaxDataLength == f.MaxLength)
                        return JapaneseMethods.TranslatePaste(Text, SelectedText, text, f.MaxDataLength);
                }
            }
            return base.TranslatePaste(text);
        }

        internal static Color FocusedBackColor { get; set; }
        internal static Color FocusedReadOnlyBackColor { get; set; }

        protected override Color GetFocusedBackColor(bool readOnly)
        {
            if (FindForm() is Form)
            {
                if (!readOnly && FocusedBackColor != Color.Empty)
                    return FocusedBackColor;
                if (readOnly && FocusedReadOnlyBackColor != Color.Empty)
                    return FocusedReadOnlyBackColor;
            }
            return base.GetFocusedBackColor(readOnly);
        }

        public static void SetFocusedBackColor(string focusedBackColorRGB, string readOnlyFocusedBackColorRGB)
        {
            if (!string.IsNullOrEmpty(focusedBackColorRGB))
                ENV.UI.TextBox.FocusedBackColor = Color.FromArgb(
                    Convert.ToInt32(focusedBackColorRGB.Substring(6, 2), 16),
                    Convert.ToInt32(focusedBackColorRGB.Substring(4, 2), 16),
                    Convert.ToInt32(focusedBackColorRGB.Substring(2, 2), 16));
            if (!string.IsNullOrEmpty(readOnlyFocusedBackColorRGB))
                ENV.UI.TextBox.FocusedReadOnlyBackColor = Color.FromArgb(
                    Convert.ToInt32(readOnlyFocusedBackColorRGB.Substring(6, 2), 16),
                    Convert.ToInt32(readOnlyFocusedBackColorRGB.Substring(4, 2), 16),
                    Convert.ToInt32(readOnlyFocusedBackColorRGB.Substring(2, 2), 16));
            Context.Current.InvokeUICommand(
                () =>
                {
                    Action<System.Windows.Forms.Control> refreshTextBoxes = c => { };
                    refreshTextBoxes =
                        c =>
                        {
                            foreach (Control c1 in c.Controls)
                            {
                                if (c1 is System.Windows.Forms.TextBox && c1.Parent is TextBox)
                                    c1.BackColor = c1.BackColor;
                                refreshTextBoxes(c1);
                            }
                        };
                    foreach (System.Windows.Forms.Form f in System.Windows.Forms.Application.OpenForms)
                        refreshTextBoxes(f);
                });
        }
        protected override char CharToLower(char c)
        {
            return UserMethods.CharTypedByUserWithUFormatToLower(c);
        }
        protected override char CharToUpper(char c)
        {
            return LocalizationInfo.Current.CharTypedByUserWithUFormatToUpper(c);
        }

        protected override Size ToleratedContainerOverflow
        {
            get { return base.ToleratedContainerOverflow + _helper.ToleratedContainerOverflow; }
        }

        public override string FilesSeparator
        {
            get
            {
                return UserMethods.FilesSeparator;
            }
        }
    }
    public enum AfterExpandGoToNextControlOptions
    {
        Default,
        True,
        False
    }
}