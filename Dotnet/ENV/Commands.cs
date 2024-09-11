using System;
using System.Windows.Forms;
using ENV.Advanced;
using Firefly.Box;
using Firefly.Box.Advanced;

namespace ENV
{
    public class Commands
    {
        public static readonly Command CustomCommand_1 = new BackwardCompatibleUserCommand { Name = "User Action 1" };
        public static readonly Command CustomCommand_2 = new BackwardCompatibleUserCommand { Name = "User Action 2" };
        public static readonly Command CustomCommand_3 = new BackwardCompatibleUserCommand { Name = "User Action 3" };
        public static readonly Command CustomCommand_4 = new BackwardCompatibleUserCommand { Name = "User Action 4" };
        public static readonly Command CustomCommand_5 = new BackwardCompatibleUserCommand { Name = "User Action 5" };
        public static readonly Command CustomCommand_6 = new BackwardCompatibleUserCommand { Name = "User Action 6" };
        public static readonly Command CustomCommand_7 = new BackwardCompatibleUserCommand { Name = "User Action 7" };
        public static readonly Command CustomCommand_8 = new BackwardCompatibleUserCommand { Name = "User Action 8" };
        public static readonly Command CustomCommand_9 = new BackwardCompatibleUserCommand { Name = "User Action 9" };
        public static readonly Command CustomCommand_10 = new BackwardCompatibleUserCommand { Name = "User Action 10" };
        public static readonly Command CustomCommand_11 = new BackwardCompatibleUserCommand { Name = "User Action 11" };
        public static readonly Command CustomCommand_12 = new BackwardCompatibleUserCommand { Name = "User Action 12" };
        public static readonly Command CustomCommand_13 = new BackwardCompatibleUserCommand { Name = "User Action 13" };
        public static readonly Command CustomCommand_14 = new BackwardCompatibleUserCommand { Name = "User Action 14" };
        public static readonly Command CustomCommand_15 = new BackwardCompatibleUserCommand { Name = "User Action 15" };
        public static readonly Command CustomCommand_16 = new BackwardCompatibleUserCommand { Name = "User Action 16" };
        public static readonly Command CustomCommand_17 = new BackwardCompatibleUserCommand { Name = "User Action 17" };
        public static readonly Command CustomCommand_18 = new BackwardCompatibleUserCommand { Name = "User Action 18" };
        public static readonly Command CustomCommand_19 = new BackwardCompatibleUserCommand { Name = "User Action 19" };
        public static readonly Command CustomCommand_20 = new BackwardCompatibleUserCommand { Name = "User Action 20" };

        public static readonly Command SelectApplicationFromList = new CustomCommand { Name = "SelectApplicationFromList" };
        public static readonly Command ExternalEvent = new CustomCommand { Name = "ExternalEvent" };
        public static readonly Command PageFooter = new CustomCommand { Name = "PageFooter" };
        public static readonly Command PageHeader = new CustomCommand { Name = "PageHeader" };
        public static readonly Command SingleInstanceAsyncTaskReactivated = new CustomCommand { Name = "SingleInstanceAsyncTaskReactivated" };
        public static readonly Command CloseAllTasks = new CustomCommand { Name = "CloseAllTasks" };
        public static readonly Command ContextGotFocus = new CustomCommand { Name = "ContextGotFocus" };
        public static readonly Command ContextLostFocus = new CustomCommand { Name = "ContextLostFocus" };
        public static readonly Command GridColumnFilterClick = new CustomCommand { Name = "GridColumnFilterClick" };

        public static readonly Command CloseAllWindows = new CustomCommand { Name = "CloseAllWindows" };
        public static readonly Command NextWindow = new CustomCommand { Name = "NextWindow" };
        public static readonly Command PreviousWindow = new CustomCommand { Name = "PreviousWindow" };
        public static readonly Command MoreWindows = new CustomCommand { Name = "MoreWindows" };

        #region Commands that has no special meaning for the users, and were used as custom commands
        public static readonly Command Fonts = new CustomCommand { Name = "Fonts" };
        public static readonly Command Colors = new CustomCommand { Name = "Colors" };
        #endregion


        public static readonly UIControllerCustomCommand CustomOrderBy = new UIControllerCustomCommand("CustomOrderBy") { Enabled = false };
        public static readonly UIControllerCustomCommand SelectOrderBy = new UIControllerCustomCommand("SelectOrderBy") { Enabled = false };
        public static readonly UIControllerCustomCommand ChangeOrderBy = new UIControllerCustomCommand("ChangeOrderBy") ;
        public static readonly UIControllerCustomCommand FilterRows = new UIControllerCustomCommand("FilterRows") { Enabled = false };
        public static readonly UIControllerCustomCommand FindRows = new UIControllerCustomCommand("FindRows") { Enabled = false };
        public static readonly UIControllerCustomCommand FindNextRow = new UIControllerCustomCommand("FindNextRow") { Enabled = false };
        public static readonly UIControllerCustomCommand ExportData = new UIControllerCustomCommand("ExportData") { Enabled = false };

        public static readonly CustomCommand PrinterSettingsDialog = new CustomCommand { Name = "PrinterSettingsDialog" };
        public static readonly CustomCommand About = new CustomCommand { Name = "About" };
        public static readonly CustomCommand ShellToOS = new CustomCommand { Name = "ShellToOS" };
        public static readonly CustomCommand PerformButtonClick = new UIControllerCustomCommand("PerformButtonClick");
        public static readonly CustomCommand Ok = new CustomCommand { Name = "Ok" };
        public static CustomCommand BreakAll = new CustomCommand { Name = "BreakIntoCode" };

        public static readonly CustomCommand ToggleInsertOverwriteMode = new CustomCommand { Name = "ToggleInsertOverwriteMode" };

        public static readonly TemplateModeCommand TemplateExit = new TemplateModeCommand(Command.Exit, "TemplateExit")
        {
            Precondition = CustomCommandPrecondition.LeaveControl,
        },
      ClearCurrentValueInTemplate = new TemplateModeCommand("ClearCurrentValueInTemplate")
      {
          Shortcut = Keys.F4,
          Precondition = CustomCommandPrecondition.LeaveControl,
      },
      ClearTemplate = new TemplateModeCommand("ClearTemplate")
      {
          Shortcut = Keys.F3,
          Precondition = CustomCommandPrecondition.LeaveControl,
      },
      TemplateFromValues = new TemplateModeCommand("TemplateFromValues")
      {
          Shortcut = Keys.F7,
          Precondition = CustomCommandPrecondition.LeaveControl,
      },
      TemplateToValues = new TemplateModeCommand("TemplateToValues")
      {
          Shortcut = Keys.F8,
          Precondition = CustomCommandPrecondition.LeaveControl,
      },
      TemplateOk = new TemplateModeCommand("TemplateOk")
      {
          Shortcut = Keys.Enter,
          Precondition = CustomCommandPrecondition.LeaveControl,
      },
      TemplateExpression = new TemplateModeCommand("TemplateExpression")
      {
          Shortcut = Keys.Control | Keys.E,
          Precondition = CustomCommandPrecondition.LeaveControl,
      };

        public static readonly CustomCommand UnKnownCommand = new CustomCommand { Name = "UnKnownCommand" };
        public static void SetDefaultKeyboardMapping()
        {
            Command.DeleteRow.Shortcut = Keys.F3;
            Command.Exit.Shortcut = Keys.Escape;
            Command.InsertRow.Shortcut = Keys.F4;

            Command.Expand.Shortcut = Keys.F5;
            Command.ExpandTextBox.Shortcut = Keys.F6;
            Command.SetFocusedControlValueToNull.Shortcut = Keys.Control | Keys.U;
            Command.SetFocusedControlValueSameAsInPreviousRow.Shortcut = Keys.Control | Keys.D;
            Command.SwitchToBrowseActivity.Shortcut = Keys.Control | Keys.Q;
            Command.SwitchToUpdateActivity.Shortcut = Keys.Control | Keys.M;
            Command.UndoEditing.Shortcut = Keys.Alt | Keys.Back;
            CustomOrderBy.Shortcut = Keys.Control | Keys.S;
            SelectOrderBy.Shortcut = Keys.Control | Keys.K;
            FilterRows.Shortcut = Keys.Control | Keys.R;
            FindRows.Shortcut = Keys.Control | Keys.L;
            FindNextRow.Shortcut = Keys.Control | Keys.N;
            ExportData.Shortcut = Keys.Control | Keys.G;

            Command.Exit.Name = "Exit";
            Command.UndoChangesInRowAndExit.Name = "Quit";
            Command.CloseForm.Name = "Close";
            Command.UndoChangesInRow.Name = "Cancel";
            Command.SwitchToBrowseActivity.Name = "Query Records";
            Command.Select.Name = "Select";

            Command.GoToNextControl.Name = "Next Field";
            Command.GoToNextRow.Name = "Next Row";
            Command.GoToNextPage.Name = "Next Page";
            Command.PlaceCursorAtPreviousLineInMultiline.Name = "Next Line";
            Command.GoToNextPage.Name = "Next Screen";


            Command.PlaceCursorAtStartOfTextBox.Name = "Begin Form";
            Command.GoToTopGridRow.Name = "Begin Page";
            Command.GoToFirstRow.Name = "Begin Table";
            Command.GoToTopGridRow.Name = "Begin Screen";

            Command.GoToLastRow.Name = "End Table";
            Command.GoToBottomGridRow.Name = "End Page";


            Command.PlaceCursorAtPreviousLineInMultiline.Name = "Previous Line";
            Command.GoToPreviousPage.Name = "Previous Page";
            Command.GoToPreviousRow.Name = "Previous Row";
            Command.GoToPreviousPage.Name = "Previous Screen";

            Command.DeleteRow.Name = "Delete Line";
            Command.InsertRow.Name = "Create Line";
            Command.ExpandTreeNode.Name = "Expand Node";


            Command.Expand.Name = "Zoom";















            ToggleInsertOverwriteMode.Shortcut = Keys.Insert;
        }
        public static void SetVersion9CompatibleKeyMapping()
        {
            ShellToOS.Shortcut = Keys.Control | Keys.Z;
            Command.SwitchToInsertActivity.Shortcut = Keys.Control | Keys.C;
            Command.Cut.Shortcut = Keys.Shift | Keys.Delete;
            Command.Copy.Shortcut = Keys.Control | Keys.Insert;
            Command.Paste.Shortcut = Keys.Shift | Keys.Insert;
            Command.UndoChangesInRow.Shortcut = Keys.F2;
        }

        public static void SetVersion10CompatibleKeyMapping()
        {
            Command.SwitchToInsertActivity.Shortcut = Keys.Control | Keys.E;
            Command.UndoChangesInRow.Shortcut = Keys.Control | Keys.F2;
            Commands.CustomOrderBy.Shortcut = Keys.Control | Keys.T;
            Command.Cut.AdditionalShortcuts=new Keys[] { Keys.Shift | Keys.Delete };
            Command.Copy.AdditionalShortcuts = new Keys[] { Keys.Control | Keys.Insert };
            Command.Paste.AdditionalShortcuts = new Keys[] { Keys.Shift | Keys.Insert };
        }
    }
    public class TemplateModeCommand : UIControllerCustomCommand
    {
        public TemplateModeCommand(Command c, string name)
            : base(c, false)
        {
            Name = name;
        }
        public TemplateModeCommand(string name)
            : base(false)
        {
            Name = name;
        }
    }
    public class UIControllerCustomCommand : CustomCommand
    {
        ContextStatic<System.Action<HandlerInvokeEventArgs>> _doAction = new ContextStatic<Action<HandlerInvokeEventArgs>>(() => delegate { });

        public UIControllerCustomCommand(bool defaultEnabled) : base(defaultEnabled)
        {
        }

        public UIControllerCustomCommand(string name)
        {
            Name = name;
        }
        public UIControllerCustomCommand(Command trigger, bool defaultEnabled)
            : base(trigger, defaultEnabled)
        {
        }

        internal void SetAction(System.Action<HandlerInvokeEventArgs> action, bool enabled)
        {
            _doAction.Value = action;
            Enabled = enabled;
        }


        internal void ClearAction()
        {
            _doAction.Value = delegate { };
            Enabled = false;
        }
        public void AddTo(HandlerCollectionWrapper handlers)
        {
            AddTo(handlers, null);
        }
        public void AddTo(HandlerCollectionWrapper handlers, Func<bool> enabled)
        {
            
            var h = handlers.Add(this);
            if (enabled!=null)
                h.BindEnabled(enabled);
            h.Invokes += e =>
            {
                _doAction.Value(e);

            };
        }

        protected override bool AllowParentViewAccordingToHandlerContext()
        {
            return false;
        }
    }

    public class BackwardCompatibleUserCommand : CustomCommand
    {
        protected override bool IgnoreIfUnhandledWithinBusinessProcess
        {
            get { return false; }
        }

        protected override Command GetDefaultBehavior()
        {
            return Command.GoToNextControl;
        }
    }
}
