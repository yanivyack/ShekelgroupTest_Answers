using System;
using System.Media;
using System.Windows.Forms;
using ENV.Advanced;
using Firefly.Box;
using Firefly.Box.Advanced;
using System.Text;

namespace ENV
{
    public class Message
    {
        public string Text { get; set; }
        public string Title { get; set; }
        public System.Windows.Forms.MessageBoxIcon Icon { get; set; }
        public System.Windows.Forms.MessageBoxButtons Buttons { get; set; }
        public System.Windows.Forms.MessageBoxDefaultButton DefaultButton { get; set; }
        public bool AppendToLog { get; set; }
        bool _throwException;
        public Message(string text, bool throwFlowAbortExceptionOnShow)
        {
            Text = text;
            _throwException = throwFlowAbortExceptionOnShow;
            if (_throwException)
            {
                Title = LocalizationInfo.Current.Error;
                Icon = System.Windows.Forms.MessageBoxIcon.Error;
                AppendToLog = false;
            }
            else
            {
                Title = LocalizationInfo.Current.Warning;
                Icon = System.Windows.Forms.MessageBoxIcon.Warning;
                AppendToLog = false;
            }
            Buttons = System.Windows.Forms.MessageBoxButtons.OK;
            DefaultButton = System.Windows.Forms.MessageBoxDefaultButton.Button1;

        }
        public static bool ReproduceExactMagicWarningErrorProgramBehaviour { get; set; }
        static void WriteTrace(string text, bool warning, bool appendToLog)
        {
            if (Firefly.Box.Text.IsNullOrEmpty(text))
                return;
            var type = "";
            if (ReproduceExactMagicWarningErrorProgramBehaviour)
            {

                type = "Error";
            }
            else
            {
                if (warning)
                    type = "Warning";
                else
                    type = "Error";
            }
            ErrorLog.WriteTrace(type, () =>
            {

                if (!ReproduceExactMagicWarningErrorProgramBehaviour || appendToLog)
                    text += ", program: " + ENV.UserMethods.Instance.Prog();
                return text;
            });
        }
        public static bool DisableBeep = false;
        public Number Show()
        {
            if (AppendToLog)
                DoAppendToLog(Title, Text);
            WriteTrace(Text, !_throwException, AppendToLog);


            if (!Common._suppressDialogForTesting && !Firefly.Box.Text.IsNullOrEmpty(Text))
            {
                if (_throwException && !DisableBeep)
                    SystemSounds.Beep.Play();
                 var r = Common.ShowMessageBox(Title, Icon, Text, Buttons, DefaultButton);
                AbortTheFlow();
                switch (r)
                {
                    case System.Windows.Forms.DialogResult.OK:
                        return 1;
                    case System.Windows.Forms.DialogResult.Cancel:
                        return 2;
                    case System.Windows.Forms.DialogResult.Abort:

                        return 3;
                    case System.Windows.Forms.DialogResult.Retry:
                        return 4;
                    case System.Windows.Forms.DialogResult.Ignore:
                        return 5;
                    case System.Windows.Forms.DialogResult.Yes:
                        return 6;
                    case System.Windows.Forms.DialogResult.No:
                        return 7;
                    default:
                        return 0;
                }

            }
            else
                AbortTheFlow();

            return 0;
        }

        void AbortTheFlow()
        {
            if (_throwException)
                HandlerCollectionWrapper.HandlerWrapper.ThrowFlowAbortException(new FlowAbortException(Text, Title));
        }

        internal static void DoAppendToLog(string title, string text)
        {
            ErrorLog.WriteToLogFile(new Exception(string.Format("[{0}] - {1}", title, text)), delegate { });
        }
        #region Error and Warning
        public static Number ShowError(Text text, Text title, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, Bool appendToLog)
        {
            return new Message(text, true)
            {
                Title = title,
                Buttons = buttons,
                Icon = icon,
                DefaultButton = defaultButton,
                AppendToLog = appendToLog
            }.Show();
        }
        public static Number ShowError(Text text, MessageBoxIcon icon, Bool appendToLog)
        {
            return new Message(text, true)
            {
                Icon = icon,
                AppendToLog = appendToLog
            }.Show();
        }
        public static Number ShowError(Text text, MessageBoxIcon icon)
        {
            return new Message(text, true)
            {
                Icon = icon,
            }.Show();
        }
        public static Number ShowError(Text text, Text title, MessageBoxIcon icon)
        {
            return new Message(text, true)
            {
                Title = title,
                Icon = icon,
            }.Show();
        }
        public static Number ShowError(Text text, Text title)
        {
            return new Message(text, true)
            {
                Title = title,
            }.Show();
        }
        public static Number ShowError(Text text, Text title, Bool appendToLog)
        {
            return new Message(text, true)
            {
                Title = title,
                AppendToLog = appendToLog
            }.Show();
        }
        public static Number ShowError(Text text, Bool appendToLog)
        {
            return new Message(text, true)
            {
                AppendToLog = appendToLog
            }.Show();
        }


        public static Number ShowError(Text text)
        {
            if (JapaneseMethods.Enabled && text.ToString().StartsWith("$"))
                return ShowError(GetMessageText(text), GetMessageTitle(text) ?? LocalizationInfo.Current.Error, true);
            else
                return new Message(text, true).Show();
        }
        public static Number ShowWarning(Text text, MessageBoxButtons buttons, MessageBoxIcon icon, Bool appendToLog)
        {
            return new Message(text, false)
            {

                Buttons = buttons,
                Icon = icon,

                AppendToLog = appendToLog
            }.Show();
        }
        public static Number ShowWarning(Text text, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton, Bool appendToLog)
        {
            return new Message(text, false)
            {

                Buttons = buttons,

                DefaultButton = defaultButton,
                AppendToLog = appendToLog
            }.Show();
        }
        public static Number ShowWarning(Text text, Text title, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton, Bool appendToLog)
        {
            return new Message(text, false)
            {
                Title = title,
                Buttons = buttons,
                DefaultButton = defaultButton,
                AppendToLog = appendToLog
            }.Show();
        }
        public static Number ShowWarning(Text text, Text title, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, Bool appendToLog)
        {
            return new Message(text, false)
            {
                Title = title,
                Buttons = buttons,
                Icon = icon,
                DefaultButton = defaultButton,
                AppendToLog = appendToLog
            }.Show();
        }
        public static Number ShowWarning(Text text, Text title, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            return new Message(text, false)
            {
                Title = title,
                Icon = icon,
                DefaultButton = defaultButton,
            }.Show();
        }

        public static Number ShowWarning(Text text, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, Bool appendToLog)
        {
            return new Message(text, false)
            {
                Buttons = buttons,
                Icon = icon,
                DefaultButton = defaultButton,
                AppendToLog = appendToLog
            }.Show();
        }

        public static Number ShowWarning(Text text, Text title, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton)
        {
            return new Message(text, false)
            {
                Title = title,
                Buttons = buttons,
                DefaultButton = defaultButton,
            }.Show();
        }
        public static Number ShowWarning(Text text, Text title, MessageBoxDefaultButton defaultButton)
        {
            return new Message(text, false)
            {
                Title = title,
                DefaultButton = defaultButton,
            }.Show();
        }
        public static Number ShowWarning(Text text, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            return new Message(text, false)
            {
                Buttons = buttons,
                Icon = icon,
                DefaultButton = defaultButton,
            }.Show();
        }

        public static Number ShowWarning(Text text, Text title, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            return new Message(text, false)
            {
                Title = title,
                Buttons = buttons,
                Icon = icon,
                DefaultButton = defaultButton,
            }.Show();
        }

        public static Number ShowWarning(Text text, Text title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return new Message(text, false)
            {
                Title = title,
                Buttons = buttons,
                Icon = icon,
            }.Show();
        }
        public static Number ShowWarning(Text text, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return new Message(text, false)
            {
                Buttons = buttons,
                Icon = icon,
            }.Show();
        }
        public static Number ShowWarning(Text text, MessageBoxIcon icon, Bool appendToLog)
        {
            return new Message(text, false)
            {
                Icon = icon,

                AppendToLog = appendToLog
            }.Show();
        }
        public static Number ShowWarning(Text text, MessageBoxButtons buttons)
        {
            return new Message(text, false)
            {

                Buttons = buttons,
            }.Show();
        }
        public static Number ShowWarning(Text text, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton)
        {
            return new Message(text, false)
            {
                Buttons = buttons,
                DefaultButton = defaultButton,
            }.Show();
        }
        public static Number ShowWarning(Text text, MessageBoxDefaultButton defaultButton)
        {
            return new Message(text, false)
            {

                DefaultButton = defaultButton,
            }.Show();
        }
        public static Number ShowWarning(Text text, Text title, MessageBoxIcon icon)
        {
            return new Message(text, false)
            {
                Title = title,
                Icon = icon,
            }.Show();
        }
        public static Number ShowWarning(Text text, Text title, MessageBoxButtons buttons)
        {
            return new Message(text, false)
            {
                Title = title,
                Buttons = buttons,
            }.Show();
        }

        public static Number ShowWarning(Text text, Text title)
        {
            return new Message(text, false)
            {
                Title = title,
            }.Show();
        }


        public static Number ShowWarning(Text text)
        {
            if (JapaneseMethods.Enabled && text.ToString().StartsWith("$"))
                return ShowWarning(GetMessageText(text), GetMessageTitle(text) ?? LocalizationInfo.Current.Warning);
            else
                return new Message(text, false).Show();
        }
        public static Number ShowWarning(Text text, MessageBoxIcon icon)
        {
            return new Message(text, false)
            {
                Icon = icon,
            }.Show();
        }
        public static Number ShowWarning(Text text, Bool appendToLog)
        {
            return new Message(text, false)
            {
                AppendToLog = appendToLog
            }.Show();
        }
        public static Number ShowWarning(Text text, MessageBoxButtons buttons, bool appendToLog)
        {
            return new Message(text, false)
            {
                Buttons = buttons,
                AppendToLog = appendToLog,
            }.Show();
        }
        public static Number ShowWarning(Text text, Text title, bool appendToLog)
        {
            return new Message(text, true)
            {
                Title = title,
                AppendToLog = appendToLog
            }.Show();
        }
        public static Number ShowWarning(Text text, Text title, MessageBoxIcon icon, bool appendToLog)
        {
            return new Message(text, false)
            {
                Title = title,
                Icon = icon,
                AppendToLog = appendToLog,
            }.Show();
        }
        static string GetMessageText(Text t)
        {
            t = t.Substring(1);
            var y = t.IndexOf("|");
            if (y > -1)
                return t.Remove(y);
            else
                return t;
        }
        static string GetMessageTitle(Text t)
        {
            t = t.Substring(1);
            var y = t.IndexOf("|");
            if (y > -1)
                return t.Substring(y + 1);
            else
                return null;
        }
        public static void ShowErrorInStatusBar(Text text)
        {
            WriteTrace(text, false, true);
            if (JapaneseMethods.Enabled && text.ToString().StartsWith("$"))
                ShowError(text);
            else
                Common.ErrorInStatusBar(text);
        }

        public static void ShowWarningInStatusBar(Text text)
        {
            WriteTrace(text, true, false);
            if (JapaneseMethods.Enabled && text.ToString().StartsWith("$"))
                ShowWarning(text);
            else
                Common.WarningInStatusBar(text);
        }
        public static void ShowWarningInStatusBar(Text text, bool appendToLog)
        {
            if (appendToLog)
                Message.DoAppendToLog("Warning", text);
            WriteTrace(text, true, appendToLog);
            Common.WarningInStatusBar(text);
        }
        public static void ShowErrorInStatusBar(Text text, bool appendToLog)
        {
            if (appendToLog)
                Message.DoAppendToLog("Error", text);
            WriteTrace(text, true, appendToLog);
            Common.ErrorInStatusBar(text);
        }
        #endregion

    }
}
