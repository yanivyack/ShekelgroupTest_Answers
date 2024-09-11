using System;
using System.Text;
using System.Windows.Forms;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV
{
    public class LocalizationInfo
    {
        public static LocalizationInfo Current = new LocalizationInfo();
        public virtual System.Windows.Forms.RightToLeft RightToLeft { get { return System.Windows.Forms.RightToLeft.No; } }

        public virtual string DuplicateIndex { get { return "Duplicate index - there is a row with the same primary key values"; } }

        public virtual string LockedRow { get { return "Current row is locked, wait to retry"; } }

        public virtual string TransactionRolledBack { get { return "Transaction rolled back"; } }

        public virtual string IllegalActivity { get { return "Activity Not Allowed"; } }

        public virtual string RowNotFoundInEntity { get { return "Row not found in entity"; } }

        public virtual string Supervisor { get { return "Supervisor"; } }

        public virtual string CreateAutomaticallyAsPartOfTheGeneration { get { return "Create automatically as part of the generation"; } }

        public virtual string DoYouWantToDelete { get { return "Do you want to delete?"; } }

        public virtual string Ok { get { return "OK"; } }

        public virtual string Cancel { get { return "Cancel"; } }

        public virtual string Sort { get { return "Sort"; } }
        public virtual string Error { get { return "Error"; } }
        public virtual string Warning { get { return "Warning"; } }

        public virtual string SortByIndex { get { return "Sort By Index"; } }

        public virtual string Filter { get { return "Filter"; } }

        public virtual string Update { get { return "Modify"; } }

        public virtual string Insert { get { return "Create"; } }

        public virtual string Delete { get { return "Delete"; } }

        public virtual string Browse { get { return "Query"; } }

        public virtual string Options { get { return "Options"; } }

        public virtual string Users { get { return "Users"; } }

        public virtual string Groups { get { return "Groups"; } }

        public virtual string Local { get { return "Virtual"; } }

        public virtual string Parameter { get { return "Parameter"; } }

        public virtual string Expand { get { return "Zoom"; } }

        public virtual string UserNotExistsOrPasswordWasWrong
        {
            get
            {
                return
                    "Invalid Credentials";
            }
        }
        public virtual string FailedToChangePassword
        {
            get
            {
                return
                    "Failed to Change Password";
            }
        }
        public virtual string NewPassword { get { return "New Password"; } }
        public virtual string CurrentPassword { get { return "Current Password"; } }
        public virtual string ConfirmNewPassword { get { return "Confirm New Password"; } }
        public virtual string ChangePassword { get { return "Change Password"; } }
        public virtual string WrongCurrentPassword { get { return "Invalid Current Password"; } }
        public virtual string NewPasswordSameAsOldPassword { get { return "The new Password is the same as the Current Password"; } }
        public virtual string ConfirmNewPasswordNotSameAsNewPassword { get { return "The Confirm new Password doesn't match the new Password"; } }
        public virtual string Exit { get { return "Exit"; } }

        public virtual string Name { get { return "Name"; } }

        public virtual string UserName { get { return "User ID"; } }

        public virtual string Password { get { return "Password"; } }

        public virtual string Login { get { return "\\Login"; } }

        public virtual string SystemLogin { get { return "Login"; } }

        public virtual string ThisRightAlreadyExists { get { return "This right already exists"; } }

        public virtual string Details { get { return "Details"; } }

        public virtual string UserGroups { get { return "User groups"; } }

        public virtual string Roles { get { return "Roles"; } }

        public virtual string RoleName { get { return "Role Name"; } }

        public virtual string Description { get { return "Description"; } }

        public virtual string Choose { get { return "Choose"; } }

        public virtual string ChooseRole { get { return "Choose role"; } }

        public virtual string AdditionalInfo { get { return "Additional Info"; } }

        public virtual string IsEqualTo { get { return "Equals to"; } }

        public virtual string GreaterOrEqual { get { return "Greater or Equals to"; } }

        public virtual string Greater { get { return "Greater than"; } }

        public virtual string LessOrEqual { get { return "Less or Equal to"; } }

        public virtual string LessThen { get { return "Less then"; } }

        public virtual string Between { get { return "Between"; } }

        public virtual string Column { get { return "Column"; } }

        public virtual string FilterType { get { return "Filter Type"; } }

        public virtual string Values { get { return "Values"; } }

        public virtual string FilterRows { get { return "Rng"; } }

        public virtual string NoRowsMatchFilter { get { return "No records within defined range"; } }

        public virtual string FindARow { get { return "Loc"; } }

        public virtual string ConfirmUpdateMessage { get { return "Do you want to save changes?"; } }

        public virtual string ConfirmUpdateTitle { get { return "Confirm Update"; } }

        public virtual string DoYouWantToUndo { get { return "Confirm Undo"; } }

        public virtual string Undo { get { return "Undo"; } }

        public virtual string ThisGroupAlreayExists
        {
            get
            {
                return
                    "This Group Already Exists";
            }
        }
        public virtual string UserModuleNotLoaded { get { return "User module not found/loaded - {0}"; } }

        public virtual string InvalidChar
        {
            get
            {
                return
                    "Invalid Char - {0}";
            }
        }

        public virtual string InputDoesntMatchRange
        {
            get
            {
                return
                    "Invalid Input - {0} - the allowed values are: {1}";
            }
        }

        public virtual string InvalidValue
        {
            get
            {
                return
                    "Invalid Input - {0} - the allowed format is: {1}";
            }
        }

        public virtual string ConfirmExecution { get { return "Confirm Execution?"; } }

        public virtual string ExecutionCompleted { get { return "Execution Complete"; } }

        public virtual string ErrorInStartRun { get { return "Error Loading {0}"; } }
        System.Text.Encoding _outerEncoding;
        public virtual System.Text.Encoding OuterEncoding { get { return _outerEncoding ?? DefaultEncoding; } set { _outerEncoding = value; } }
        System.Text.Encoding _innerEncoding;
        public static Encoding DefaultEncoding = System.Text.Encoding.Default;

        public virtual string ControlMustBeUpdated { get { return "Control must be updated"; } }

        public virtual System.Text.Encoding InnerEncoding
        {
            get { return _innerEncoding ?? DefaultEncoding; }
            set { _innerEncoding = value; }
        }
        public virtual MessageBoxOptions MessageBoxOptions { get { return default(MessageBoxOptions); } }
        public virtual string Digits { get { return "digits"; } }
        public virtual string UpdateNotAllowedInBrowseMode { get { return "Update is not allowed while in browse activity"; } }
        public virtual string RowWasChanged { get { return "Row was changed"; } }
        public virtual string RowDoesNotExist { get { return "Row was lost"; } }
        public virtual string ExportType { get { return "Type"; } }
        public virtual string FileName { get { return "File Name"; } }
        public virtual string Template { get { return "Template"; } }
        public virtual string ExportData { get { return "Export Data"; } }
        public virtual string OpenFile { get { return "Open File"; } }
        public virtual string Import { get { return "Import"; } }
        public virtual string ErrorInExportData { get { return "Error while generating data"; } }
        public virtual string DefaultBoolInputRange { get { return "True,False"; } }
        public virtual string SecuredValues { get { return "Secured Values"; } }
        public virtual string NameAlreadyExist { get { return "Value Already Exists"; } }
        public virtual string ReadOnlyEntityUpdate { get { return "Cannot update readonly entity"; } }
        public virtual string FilterExpression { get { return "Expression"; } }
        public virtual string ErrorOpeningTable { get { return "Error opening table:"; } }
        public virtual string InvalidTableStructure { get { return "Table definition mismatch"; } }
        public virtual string UIControllerWithoutView { get { return "Task without screen"; } }
        public virtual string Delimiter { get { return "Delimiter"; } }
        public virtual string AvailableColumns { get { return "Available Columns"; } }
        public virtual string SelectedColumns { get { return "Selected Columns"; } }
        public virtual string InvalidDate { get { return "Invalid Date"; } }
        public virtual string PasswordWasChangedSuccessfully { get { return "Password Was Changed Successfully"; } }
        public virtual string UserAlreadyExists { get { return "User Already Exists"; } }
        public virtual string GroupAlreadyExists { get { return "Group Already Exists"; } }
        public virtual string UserFileIsLocked { get { return "User File is Locked"; } }
        public virtual string UserFileIsLockedEnterReadOnly { get { return "User File is Locked.\r\nWould you like to enter in readonly mode?"; } }
        public virtual string ExpandTextBox { get { return "WIDE"; } }
        public virtual string TextboxInsertMode { get { return "INS"; } }
        public virtual string TextboxOverwriteMode { get { return "OVR"; } }
        public virtual string ConfirmExitApplication { get { return "Would you like to exit the application?"; } }
        public virtual string ExitApplication { get { return "Exit Application"; } }
        public virtual string Yes { get { return "&Yes"; } }
        public virtual string No { get { return "&No"; } }
        public virtual string YesHotKeys { get { return "yY"; } }
        public virtual string NoHotKeys { get { return "nN"; } }
        public virtual string NullInOnlyOnePartOfDateTimePair { get { return "Null in only one part of date-time pair"; } }

        public virtual bool SupportVisualLogicalFunctions { get { return false; } }

        public virtual string GetErrorFor(DatabaseErrorType error)
        {
            switch (error)
            {
                case DatabaseErrorType.DataChangeFailed:
                    return "Update failed";
                case DatabaseErrorType.DuplicateIndex:
                    return "Duplicate index";
                case DatabaseErrorType.LockedRow:
                    return "Locked Row";
                case DatabaseErrorType.RowDoesNotExist:
                    return "Record lost";
                case DatabaseErrorType.RowWasChangedSinceLoaded:
                    return "Record has been updated";
                case DatabaseErrorType.ReadOnlyEntityUpdate:
                    return "ReadOnly Entity Update";
                default:
                    return "Unknown Error";
            }
        }
        public virtual string PleaseEnterIdAndPassword { get { return "Please enter your system user ID and password"; } }

        public virtual string LogonParameters
        {
            get { return "Logon Parameters"; }
        }

        public virtual string StartOnRowWhereErrorRepositionToFirstRow { get { return "Record not found - positioned at first"; } }

        public virtual string StartOnRowWhereError { get { return "Record not found - positioned at next"; } }
        public virtual string Date { get { return "Date"; } }
        public virtual string UserNotAuthorizedForThisApplication
        {
            get
            {
                return "User is not authorized for this application";
            }
        }

        public virtual string UserIsNotAuthorized { get { return "User is not authorized for this operation"; } }
        public virtual string HelpRequestFailed { get { return "Help request failed '{0}'"; } }
        public virtual string PrintPreview { get { return "Print Preview"; } }
        public virtual string Print { get { return "Print"; } }
        public virtual string PrevieAsWord { get { return " as "; } }
        public virtual string Save { get { return "Save"; } }
        public virtual string Send { get { return "Send"; } }
        public virtual string View { get { return "View"; } }
        public virtual string OnePageView { get { return "1 Page View"; } }
        public virtual string TwoPageView { get { return "2 Pages View"; } }
        public virtual string FourPageView { get { return "4 Pages View"; } }
        public virtual string PrintPreviewFit { get { return "Fit"; } }
        public virtual string PrintPreviewZoom { get { return "Zoom"; } }
        public virtual string Page { get { return "Page"; } }
        public virtual string PrintPreviewOf { get { return "of "; } }
        public virtual string Field { get { return "Field"; } }
        public virtual string FieldName { get { return "Field Name"; } }
        public virtual string FromEntity { get { return "From Entity"; } }

        public virtual string ControllerNotFound => "Program not found for public name: ";

        public virtual Text.ITextComparer GetCaseInsensitiveTextComparer()
        {
            return Common.IgnoreCaseTextComparer.Instance;
        }

        internal virtual char CharTypedByUserWithUFormatToUpper(char c)
        {
            if (ENV.UserSettings.VersionXpaCompatible)
                return Char.ToUpper(c);
            return UserMethods.OnlyEnglishUpper(c);
        }
    }

    public class HebrewLocalizationInfo : LocalizationInfo
    {
        public override string UserIsNotAuthorized => "משתמש אינו רשאי לבצע פעולה זו";
        public override string ControllerNotFound => "לא נמצאה תוכנית עם שם ציבורי: ";
        public override string NoRowsMatchFilter => "אין רשומות המתאימות לתחום";
        public override string TransactionRolledBack => "טרנסאקציה בוטלה";
        public override string UserModuleNotLoaded => "כשלון בטעינת DLL חיצוני: ";
        public override System.Windows.Forms.RightToLeft RightToLeft { get { return System.Windows.Forms.RightToLeft.Yes; } }

        public override string DuplicateIndex { get { return "מפתח כפול - קיימת רשומה עם ערכים זהים בשדות המפתח"; } }

        public override string IllegalActivity { get { return "פעולה זו אינה מותרת במסך זה"; } }

        public override string LockedRow { get { return "רשומה נוכחית נעולה, ממתין לנסיון חוזר"; } }

        public override string RowNotFoundInEntity { get { return "רשומה לא נמצאה בטבלה "; } }

        public override string Supervisor { get { return "אחראי"; } }

        public override string CreateAutomaticallyAsPartOfTheGeneration { get { return "נוצר אוטומטית כחלק מההסבה"; } }

        public override string DoYouWantToDelete { get { return "האם ברצונך למחוק?"; } }

        public override string Ok { get { return "אישור"; } }

        public override string Cancel { get { return "ביטול"; } }

        public override string Sort { get { return "מיון"; } }

        public override string Error
        {
            get { return "שגיאה"; }
        }
        public override string Warning
        {
            get { return "אזהרה"; }
        }

        public override string SortByIndex { get { return "מיון לפי מפתח"; } }

        public override string Filter { get { return "סינון"; } }

        public override string Update { get { return "עדכון"; } }

        public override string Insert { get { return "הוספה"; } }

        public override string Delete { get { return "מחיקה"; } }

        public override string Browse { get { return "דפדוף"; } }

        public override string Options { get { return "אפשרויות"; } }

        public override string Users { get { return "משתמשים"; } }

        public override string Groups { get { return "קבוצות"; } }

        public override string Local { get { return "מקומי"; } }

        public override string Parameter { get { return "פרמטר"; } }

        public override string Expand { get { return "חלון"; } }

        public override string Roles { get { return "הרשאות"; } }

        public override string UserNotExistsOrPasswordWasWrong { get { return "משתמש לא קיים או סיסמה שגויה"; } }

        public override string Exit { get { return "יציאה"; } }

        public override string Name { get { return "שם"; } }

        public override string UserName { get { return "שם משתמש"; } }

        public override string Password { get { return "סיסמה"; } }
        public override string FailedToChangePassword { get { return "כשלון בעדכון סיסמה"; } }
        public override string NewPassword { get { return "סיסמה חדשה"; } }
        public override string CurrentPassword { get { return "סיסמה נוכחית"; } }
        public override string ConfirmNewPassword { get { return "אישור סיסמה חדשה"; } }
        public override string ChangePassword { get { return "שנה סיסמה"; } }
        public override string WrongCurrentPassword { get { return "סיסמה נוכחית שגויה"; } }
        public override string NewPasswordSameAsOldPassword { get { return "הסיסמה החדשה זהה לסיסמה הנוכחית"; } }
        public override string ConfirmNewPasswordNotSameAsNewPassword { get { return "הסיסמה החדשה ואישור הסיסמה החדשה אינם זהים"; } }

        public override string Login { get { return "כניסה"; } }

        public override string SystemLogin { get { return "כניסה למערכת"; } }

        public override string ThisRightAlreadyExists { get { return "הרשאה זו כבר מופיעה בטבלה"; } }

        public override string Details { get { return "פרטים נוספים"; } }

        public override string UserGroups { get { return "קבוצות למשתמש"; } }

        public override string RoleName { get { return "הרשאות"; } }
        public override string Description { get { return "תאור"; } }

        public override string Choose { get { return "בחר"; } }

        public override string ChooseRole { get { return "בחר הרשאה"; } }

        public override string AdditionalInfo { get { return "מידע נוסף"; } }

        public override string IsEqualTo { get { return "שווה ל"; } }

        public override string GreaterOrEqual { get { return "גדול או שווה ל"; } }

        public override string Greater { get { return "גדול מ"; } }

        public override string LessOrEqual { get { return "קטן או שווה ל"; } }

        public override string LessThen { get { return "קטן מ"; } }

        public override string Between { get { return "בין הערכים"; } }

        public override string Column { get { return "שדה"; } }

        public override string FilterType { get { return "סוג"; } }

        public override string Values { get { return "ערכים"; } }

        public override string FilterRows { get { return "סנן רשומות"; } }

        public override string FindARow { get { return "מצא רשומה"; } }

        public override string ConfirmUpdateMessage { get { return "האם ברצונך לשמור את הנתונים בבסיס הנתונים?"; } }
        public override string ConfirmUpdateTitle { get { return "אישור עדכון"; } }
        public override string DoYouWantToUndo { get { return "אישור ביטול"; } }
        public override string InvalidChar { get { return "תו לא חוקי - {0}"; } }
        public override string InputDoesntMatchRange { get { return "ערך לא חוקי לשדה - {0} הערכים המותרים הם:{1}"; } }
        public override string InvalidValue { get { return "ערך לא חוקי לשדה - {0} המבנה המותר הוא:{1}"; } }
        public override string ConfirmExecution { get { return "אשר ביצוע?"; } }
        public override string ExecutionCompleted { get { return "ביצוע הושלם"; } }
        System.Text.Encoding _innerEncoding = System.Text.Encoding.GetEncoding(1255);
        public override System.Text.Encoding OuterEncoding { get { return System.Text.Encoding.GetEncoding(1255); } }

        public override string ControlMustBeUpdated { get { return "חובה לעדכן את השדה"; } }

        public override System.Text.Encoding InnerEncoding { get { return _innerEncoding; } set { _innerEncoding = value; } }
        public override string ErrorInStartRun { get { return "שגיאה בטעינת {0}"; } }
        public override MessageBoxOptions MessageBoxOptions { get { return MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign; } }
        public override string Digits { get { return "ספרות"; } }
        public override string UpdateNotAllowedInBrowseMode { get { return "אין לבצע פעולת עדכון במהלך פעילות דפדוף, העדכון בוטל"; } }
        public override string RowWasChanged { get { return "רשומה השתנתה"; } }
        public override string RowDoesNotExist { get { return "רשומה אבדה"; } }
        public override string ExportType { get { return "סוג"; } }
        public override string FileName { get { return "שם קובץ"; } }
        public override string Template { get { return "תבנית"; } }
        public override string ExportData { get { return "ייצוא נתונים לקובץ"; } }
        public override string OpenFile { get { return "פתח קובץ"; } }
        public override string Import { get { return "יבוא"; } }
        public override string ErrorInExportData { get { return "שגיאה במהלך ייצוא נתונים"; } }
        public override string Undo { get { return "בטל"; } }
        public override string ThisGroupAlreayExists { get { return "קבוצה זה קבר משוייכת"; } }

        public override string DefaultBoolInputRange
        {
            get
            {
                if (UserSettings.Version8Compatible)
                    return "True,False";
                return "אמת,שקר";
            }
        }
        public override string SecuredValues { get { return "ערכים מאובטחים"; } }
        public override string NameAlreadyExist { get { return "ערך זה קיים"; } }
        public override string ReadOnlyEntityUpdate { get { return "לא ניתן לעדכן ישות אשר הוגדרה כקריאה בלבד"; } }
        public override string FilterExpression { get { return "נוסחה"; } }
        public override string ErrorOpeningTable { get { return "כשלון בפתיחת טבלה:"; } }
        public override string Delimiter { get { return "מפריד"; } }
        public override string AvailableColumns { get { return "שדות זמינים"; } }
        public override string SelectedColumns { get { return "שדות שנבחרו"; } }
        public override string InvalidDate { get { return "תאריך שגוי"; } }
        public override string PasswordWasChangedSuccessfully { get { return "סיסמה שונתה בהצלחה"; } }
        public override string UserAlreadyExists { get { return "שם משתמש קיים"; } }
        public override string GroupAlreadyExists { get { return "שם קבוצה קיים"; } }
        public override string UserFileIsLocked { get { return "קובץ משתמשים נעול"; } }
        public override string UserFileIsLockedEnterReadOnly { get { return "קובץ משתמשים נועל.\r\nלהכנס במצב צפייה בלבד?"; } }
        public override string ExpandTextBox { get { return "הרחב"; } }
        public override string TextboxInsertMode { get { return "INS"; } }
        public override string TextboxOverwriteMode { get { return "OVR"; } }
        public override string ConfirmExitApplication { get { return "האם ברצונך לצאת מהמערכת?"; } }
        public override string ExitApplication { get { return "יציאה מהמערכת"; } }
        public override string Yes { get { return "&כן"; } }
        public override string No { get { return "&לא"; } }
        public override string YesHotKeys { get { return "כfF"; } }
        public override string NoHotKeys { get { return "לkK"; } }
        public override string InvalidTableStructure { get { return "אי התאמה בהגדרת קובץ"; } }
        public override string UIControllerWithoutView { get { return "תוכנית ללא מסך"; } }
        public override bool SupportVisualLogicalFunctions
        {
            get
            {
                return true;
            }
        }
        public override string PrintPreview { get { return "תצוגה מקדימה"; } }
        public override string Print { get { return "הדפס"; } }
        public override string PrevieAsWord { get { return " כ"; } }
        public override string Save { get { return "שמור"; } }
        public override string Send { get { return "שלח"; } }
        public override string View { get { return "תצוגה"; } }
        public override string OnePageView { get { return "עמוד אחד"; } }
        public override string TwoPageView { get { return "שני עמודים"; } }
        public override string FourPageView { get { return "ארבעה עמודים"; } }
        public override string PrintPreviewFit { get { return "התאם"; } }
        public override string PrintPreviewZoom { get { return "הגדל"; } }
        public override string Page { get { return "עמוד"; } }
        public override string PrintPreviewOf { get { return "מתוך "; } }
        public override string GetErrorFor(DatabaseErrorType error)
        {
            switch (error)
            {
                case DatabaseErrorType.DataChangeFailed:
                    return "עדכון נכשל";
                case DatabaseErrorType.DuplicateIndex:
                    return "מפתח כפול";
                case DatabaseErrorType.LockedRow:
                    return "רשומה נעולה";
                case DatabaseErrorType.RowDoesNotExist:
                    return "רשומה אבדה";
                case DatabaseErrorType.RowWasChangedSinceLoaded:
                    return "רשומה השתנתה";
                case DatabaseErrorType.ReadOnlyEntityUpdate:
                    return ReadOnlyEntityUpdate;
                default:

                    return "שגיאה לא ידועה";
            }
        }

        public override string PleaseEnterIdAndPassword
        {
            get { return "אנא הזן את פרטי המשתמש והסיסמה"; }
        }
        public override string LogonParameters
        {
            get
            {
                return "פרטי הזדהות";
            }
        }

        public override string StartOnRowWhereErrorRepositionToFirstRow { get { return "רשומה לא נמצאה - מוצגת הרשומה הראשונה"; } }

        public override string StartOnRowWhereError { get { return "רשומה לא נמצאה - מוצגת הרשומה הבאה"; } }

        public override string Date
        {
            get { return "תאריך"; }
        }
        public override string UserNotAuthorizedForThisApplication
        {
            get { return "משתמש זה אינו מורשה למערכת"; }
        }

        public override string HelpRequestFailed
        {
            get { return "שגיאה בהצגת עזרה '{0}'"; }
        }

        public override string Field { get { return "שדה"; } }
        public override string FieldName { get { return "תאור שדה"; } }
        public override string FromEntity { get { return "מקובץ"; } }
    }

    public class FrenchLocalizationInfo : LocalizationInfo
    {
        internal override char CharTypedByUserWithUFormatToUpper(char c)
        {
            return char.ToUpper(c);
        }
        public FrenchLocalizationInfo()
        {
            Firefly.Box.Command.BeforeControlClick.Name = "Click sur Control";
            Firefly.Box.Command.BeforeWindowClick.Name = "Click sur fenêtre";
        }

        public override string DuplicateIndex { get { return "Enregistrement en double"; } }

        public override string IllegalActivity { get { return "Opération non autorisée"; } }

        public override string LockedRow { get { return "Enregistrement verrouillé"; } }

        public override string RowNotFoundInEntity { get { return "Enregistrement non trouvé"; } }

        public override string Supervisor { get { return "Superviseur"; } }

        public override string CreateAutomaticallyAsPartOfTheGeneration { get { return "Automatiquement créé dans le cadre de la conversion"; } }

        public override string DoYouWantToDelete { get { return "Confirmez-vous la suppression ?"; } }

        public override string Ok { get { return "OK"; } }

        public override string Cancel { get { return "Annuler"; } }

        public override string Sort { get { return "Trier"; } }

        public override string Error
        {
            get { return "Erreur"; }
        }
        public override string Warning
        {
            get { return "Avertissement"; }
        }

        public override string SortByIndex { get { return "Trier par index"; } }

        public override string Filter { get { return "Filtre"; } }

        public override string Update { get { return "Modifier"; } }

        public override string Insert { get { return "Insérer"; } }

        public override string Delete { get { return "Supprimer"; } }

        public override string Browse { get { return "Parcourir"; } }

        public override string Options { get { return "Options"; } }

        public override string Users { get { return "Utilisateurs"; } }

        public override string Groups { get { return "Groupes"; } }

        public override string Local { get { return "Local"; } }

        public override string Expand { get { return "Agrandir"; } }

        public override string Roles { get { return "Rôles"; } }

        public override string UserNotExistsOrPasswordWasWrong { get { return "Utilisateur inexistant ou mot de passe incorrect"; } }

        public override string Exit { get { return "Quitter"; } }

        public override string Name { get { return "Nom"; } }

        public override string UserName { get { return "Identifiant"; } }

        public override string Password { get { return "Mot de passe"; } }
        public override string FailedToChangePassword { get { return "Echec lors du chamgement de mot de passe"; } }
        public override string NewPassword { get { return "Nouveau mot de passe"; } }
        public override string CurrentPassword { get { return "Mot de passe actuel"; } }
        public override string ConfirmNewPassword { get { return "Confirmez le nouveau mot de passe"; } }
        public override string ChangePassword { get { return "Changer de mot de passe"; } }
        public override string WrongCurrentPassword { get { return "Mot de passe actuel incorrect"; } }
        public override string NewPasswordSameAsOldPassword { get { return "Le nouveau mot de passe est identique au mot de passe actuel"; } }
        public override string ConfirmNewPasswordNotSameAsNewPassword { get { return "Le nouveau mot de passe et la confirmation du nouveau mot de passe ne sont pas identiques"; } }

        public override string Login { get { return "Code d'accès"; } }

        public override string SystemLogin { get { return "Connexion"; } }

        public override string ThisRightAlreadyExists { get { return "Cette role est déjà dans la table"; } }

        public override string Details { get { return "Détails"; } }

        public override string UserGroups { get { return "Groupes d'utilisateurs"; } }

        public override string RoleName { get { return "Rôle"; } }
        public override string Description { get { return "Description"; } }

        public override string Choose { get { return "Sélectionner"; } }

        public override string ChooseRole { get { return "Sélectionnez le rôle"; } }

        public override string AdditionalInfo { get { return "Plus d'informations"; } }

        public override string IsEqualTo { get { return "égal à"; } }

        public override string GreaterOrEqual { get { return "Supérieur ou égal à"; } }

        public override string Greater { get { return "Supérieur à"; } }

        public override string LessOrEqual { get { return "Inférieur ou égal à"; } }

        public override string LessThen { get { return "Inférieur à"; } }

        public override string Between { get { return "Compris Entre"; } }

        public override string Column { get { return "Colonne"; } }

        public override string FilterType { get { return "Type"; } }

        public override string Values { get { return "Valeurs"; } }

        public override string FilterRows { get { return "Filtrer les enregistrements"; } }

        public override string FindARow { get { return "Rechercher un enregistrement"; } }

        public override string ConfirmUpdateMessage { get { return "Voulez-vous enregistrer les données dans la base de données?"; } }
        public override string ConfirmUpdateTitle { get { return "Confirmer la mise à jour"; } }
        public override string DoYouWantToUndo { get { return "Confirmez-vous l'annulation ?"; } }
        public override string InvalidChar { get { return "Caractère non valide - {0}"; } }
        public override string InputDoesntMatchRange { get { return "Champ de valeur invalide - {0} valeurs autorisées sont:{1}"; } }
        public override string InvalidValue { get { return "Champ de valeur invalide - {0} la structure autorisée est de:{1}"; } }
        public override string ConfirmExecution { get { return "Confirmez-vous l'exécution ?"; } }
        public override string ExecutionCompleted { get { return "Livraison Terminé"; } }
        public override string ControlMustBeUpdated { get { return "Obligation de mettre à jour le champ"; } }
        public override string ErrorInStartRun { get { return "Erreur lors du chargement {0}"; } }

        public override string Digits { get { return "Chiffres"; } }
        public override string UpdateNotAllowedInBrowseMode { get { return "Mise à jour non effectuée en mode liste, la mise à jour est annulée"; } }
        public override string RowWasChanged { get { return "Enregistrement modifié"; } }
        public override string RowDoesNotExist { get { return "Enregistrement inexistant"; } }
        public override string ExportType { get { return "Type"; } }
        public override string FileName { get { return "Nom du fichier"; } }
        public override string Template { get { return "Modèle"; } }
        public override string ExportData { get { return "Exporter les données vers un fichier"; } }
        public override string OpenFile { get { return "Ouvrir un fichier"; } }
        public override string Import { get { return "Importer"; } }
        public override string ErrorInExportData { get { return "Erreur lors de l'exportation des données"; } }
        public override string Undo { get { return "Annuler"; } }
        public override string ThisGroupAlreayExists { get { return "Ce groupe existe déjà"; } }
        public override string DefaultBoolInputRange
        {
            get
            {
                if (UserSettings.Version8Compatible)
                    return "True,False";
                return "Vrai, Faux";
            }
        }
        public override string SecuredValues { get { return "Valeurs sécurisées"; } }
        public override string NameAlreadyExist { get { return "Ce nom existe déjà"; } }
        public override string ReadOnlyEntityUpdate { get { return "Vous ne pouvez pas mettre à jour une entité qui est en lecture seule"; } }
        public override string FilterExpression { get { return "Expression"; } }
        public override string ErrorOpeningTable { get { return "Anomalie à l'ouverture de la table"; } }
        public override string Delimiter { get { return "Séparateur"; } }
        public override string AvailableColumns { get { return "Colonnes disponibles"; } }
        public override string SelectedColumns { get { return "Colonnes sélectionnés"; } }
        public override string InvalidDate { get { return "Date incorrecte"; } }
        public override string PasswordWasChangedSuccessfully { get { return "Mot de passe modifié avec succès"; } }
        public override string UserAlreadyExists { get { return "L'utilisateur existe déjà"; } }
        public override string GroupAlreadyExists { get { return "Le groupe existe déjà"; } }
        public override string UserFileIsLocked { get { return "Le fichier des utilisateurs est verrouillé"; } }
        public override string UserFileIsLockedEnterReadOnly { get { return "Le fichier des utilisateurs est verrouillé.\r\nConnectez-vous en lecture seule ?"; } }
        public override string ExpandTextBox { get { return "Agrandir"; } }
        public override string TextboxInsertMode { get { return "INS"; } }
        public override string TextboxOverwriteMode { get { return "OVR"; } }
        public override string ConfirmExitApplication { get { return "Voulez-vous vous déconnecter ?"; } }
        public override string ExitApplication { get { return "Déconnexion"; } }
        public override string Yes { get { return "&Oui"; } }
        public override string No { get { return "&Non"; } }
        public override string YesHotKeys { get { return "ofF"; } }
        public override string NoHotKeys { get { return "nkK"; } }
        public override string InvalidTableStructure { get { return "Incohérence dans le définition du fichier"; } }
        public override string UIControllerWithoutView { get { return "Programme sans écran"; } }

        public override string GetErrorFor(DatabaseErrorType error)
        {
            switch (error)
            {
                case DatabaseErrorType.DataChangeFailed:
                    return "Echec de mise à jour";
                case DatabaseErrorType.DuplicateIndex:
                    return "Clé en double";
                case DatabaseErrorType.LockedRow:
                    return "Enregistrement verrouillé";
                case DatabaseErrorType.RowDoesNotExist:
                    return "Enregistrement perdu";
                case DatabaseErrorType.RowWasChangedSinceLoaded:
                    return "Enregistrement modifié";
                case DatabaseErrorType.ReadOnlyEntityUpdate:
                    return ReadOnlyEntityUpdate;
                default:
                    return "Erreur inconnue";
            }
        }
        public override string PleaseEnterIdAndPassword
        {
            get { return "Merci d'entrer votre User ID et Mot de passe."; }
        }
        public override string LogonParameters
        {
            get
            {
                return "Informations de connexion";
            }
        }

        public override string PrintPreview { get { return "Aperçu avant Impression"; } }
        public override string Print { get { return "Imprimer"; } }
        public override string PrevieAsWord { get { return " sous "; } }
        public override string Save { get { return "Enregistrer"; } }
        public override string Send { get { return "Envoyer"; } }
        public override string View { get { return "Afficher"; } }
        public override string OnePageView { get { return "Afficher 1 Page"; } }
        public override string TwoPageView { get { return "Afficher 2 Page"; } }
        public override string FourPageView { get { return "Afficher 4 Page"; } }
        public override string PrintPreviewFit { get { return "Ajuster"; } }
        public override string PrintPreviewZoom { get { return "Zoom"; } }
        public override string Page { get { return "Page"; } }
        public override string PrintPreviewOf { get { return "de "; } }


    }
    public class SwedishLocalizationInfo : LocalizationInfo
    {
        internal override char CharTypedByUserWithUFormatToUpper(char c)
        {
            return char.ToUpper(c);
        }
        public override System.Windows.Forms.RightToLeft RightToLeft { get { return System.Windows.Forms.RightToLeft.No; } }

        public override string DuplicateIndex { get { return "Duplicerat index - en annan rad har samma primära nyckelvärden"; } }

        public override string LockedRow { get { return "Aktuell rad är låst, vänta och försök sen igen"; } }

        public override string TransactionRolledBack { get { return "Transaktion backad"; } }

        public override string IllegalActivity { get { return "Aktivitet ej tillåten"; } }

        public override string RowNotFoundInEntity { get { return "Raden hittades inte"; } }

        public override string Supervisor { get { return "Systemadministratör"; } }

        public override string CreateAutomaticallyAsPartOfTheGeneration { get { return "Skapa automatiskt som del av generering"; } }

        public override string DoYouWantToDelete { get { return "Vill du ta bort?"; } }

        public override string Ok { get { return "OK"; } }

        public override string Cancel { get { return "\\Avbryt"; } }

        public override string Sort { get { return "Sortera"; } }
        public override string Error { get { return "Fel"; } }
        public override string Warning { get { return "Varning"; } }

        public override string SortByIndex { get { return "Sortera efter index"; } }

        public override string Filter { get { return "Filtrera"; } }

        public override string Update { get { return "Ändra"; } }

        public override string Insert { get { return "Skapa"; } }

        public override string Delete { get { return "Ta bort"; } }

        public override string Browse { get { return "Fråga"; } }

        public override string Options { get { return "Val"; } }

        public override string Users { get { return "Användare"; } }

        public override string Groups { get { return "Grupper"; } }

        public override string Local { get { return "Virtuell"; } }

        public override string Parameter { get { return "Parameter"; } }

        public override string Expand { get { return "Zoom"; } }

        public override string UserNotExistsOrPasswordWasWrong
        {
            get
            {
                return
                    "Ogiltiga autentiseringsuppgifter";
            }
        }
        public override string FailedToChangePassword
        {
            get
            {
                return
                    "Byte av lösenord misslyckades";
            }
        }
        public override string NewPassword { get { return "Nytt lösenord"; } }
        public override string CurrentPassword { get { return "Nuvarande lösenord"; } }
        public override string ConfirmNewPassword { get { return "Bekräfta nytt lösenord"; } }
        public override string ChangePassword { get { return "Byt lösenord"; } }
        public override string WrongCurrentPassword { get { return "Ogiltigt nuvarande lösenord"; } }
        public override string NewPasswordSameAsOldPassword { get { return "Det nya lösenordet är samma som det gamla lösenordet"; } }
        public override string ConfirmNewPasswordNotSameAsNewPassword { get { return "Bekräfta nytt lösenord stämmer inte med Nytt lösenord"; } }
        public override string Exit { get { return "Avsluta"; } }

        public override string Name { get { return "Namn"; } }

        public override string UserName { get { return "Användarnamn"; } }

        public override string Password { get { return "Lösenord"; } }

        public override string Login { get { return "\\Logga in"; } }

        public override string SystemLogin { get { return "Logga in"; } }

        public override string ThisRightAlreadyExists { get { return "Denna rättighet finns redan"; } }

        public override string Details { get { return "Detaljer"; } }

        public override string UserGroups { get { return "Användargrupper"; } }

        public override string Roles { get { return "Roller"; } }

        public override string RoleName { get { return "Rollnamn"; } }

        public override string Description { get { return "Beskrivning"; } }

        public override string Choose { get { return "Välj"; } }

        public override string ChooseRole { get { return "Välj roll"; } }

        public override string AdditionalInfo { get { return "Ytterligare info"; } }

        public override string IsEqualTo { get { return "Lika med"; } }

        public override string GreaterOrEqual { get { return "Större än eller lika med"; } }

        public override string Greater { get { return "Större än"; } }

        public override string LessOrEqual { get { return "Mindre än eller lika med"; } }

        public override string LessThen { get { return "Mindre än"; } }

        public override string Between { get { return "Mellan"; } }

        public override string Column { get { return "Kolumn"; } }

        public override string FilterType { get { return "Filtertyp"; } }

        public override string Values { get { return "Värden"; } }

        public override string FilterRows { get { return "Avgr"; } }

        public override string FindARow { get { return "Sök"; } }

        public override string ConfirmUpdateMessage { get { return "Vill du spara ändringar?"; } }

        public override string ConfirmUpdateTitle { get { return "Bekräfta uppdatera"; } }

        public override string DoYouWantToUndo { get { return "Bekräfta ångra"; } }

        public override string Undo { get { return "Ångra"; } }

        public override string ThisGroupAlreayExists
        {
            get
            {
                return
                    "Denna grupp finns redan";
            }
        }
        public override string UserModuleNotLoaded { get { return "User module not found/loaded - {0}"; } }

        public override string InvalidChar
        {
            get
            {
                return
                    "Ogiltigt tecken - {0}";
            }
        }

        public override string InputDoesntMatchRange
        {
            get
            {
                return
                    "Ogiltig inmatning - {0} - tillåtna värden: {1}";
            }
        }

        public override string InvalidValue
        {
            get
            {
                return
                    "Ogiltig inmatning - {0} - tillåtet format: {1}";
            }
        }

        public override string ConfirmExecution { get { return "Bekräfta exekvering?"; } }

        public override string ExecutionCompleted { get { return "Exekvering klar"; } }

        public override string ErrorInStartRun { get { return "Fel vid inläsning {0}"; } }
        System.Text.Encoding _outerEncoding = System.Text.Encoding.GetEncoding(1252);
        public override System.Text.Encoding OuterEncoding { get { return _outerEncoding; } set { _outerEncoding = value; } }
        System.Text.Encoding _innerEncoding = System.Text.Encoding.GetEncoding(1252);
        public override string ControlMustBeUpdated { get { return "Kontroll måste uppdateras"; } }

        public override System.Text.Encoding InnerEncoding
        {
            get { return _innerEncoding; }
            set { _innerEncoding = value; }
        }
        public override MessageBoxOptions MessageBoxOptions { get { return default(MessageBoxOptions); } }
        public override string Digits { get { return "siffror"; } }
        public override string UpdateNotAllowedInBrowseMode { get { return "Uppdatering ej tillåten under browse-aktivitet"; } }
        public override string RowWasChanged { get { return "Rad ändrades"; } }
        public override string RowDoesNotExist { get { return "Rad försvann"; } }
        public override string ExportType { get { return "Typ"; } }
        public override string FileName { get { return "Filnamn"; } }
        public override string Template { get { return "Mall"; } }
        public override string ExportData { get { return "Exportera data"; } }
        public override string OpenFile { get { return "Öppna fil"; } }
        public override string Import { get { return "Importera"; } }
        public override string ErrorInExportData { get { return "Fel vid datagenerering"; } }
        public override string DefaultBoolInputRange { get { return "Sant, Falskt"; } }
        public override string SecuredValues { get { return "Säkra värden"; } }
        public override string NameAlreadyExist { get { return "Värde finns redan"; } }
        public override string ReadOnlyEntityUpdate { get { return "Kan inte uppdatera skrivskyddad enhet"; } }
        public override string FilterExpression { get { return "Uttryck"; } }
        public override string ErrorOpeningTable { get { return "Fel när tabell öppnades:"; } }
        public override string InvalidTableStructure { get { return "Tabelldefinition matchar inte"; } }
        public override string UIControllerWithoutView { get { return "Rutin utan formulär"; } }
        public override string Delimiter { get { return "Avgränsare"; } }
        public override string AvailableColumns { get { return "Tillgängliga kolumner"; } }
        public override string SelectedColumns { get { return "Valda kolumner"; } }
        public override string InvalidDate { get { return "Ogiltigt datum"; } }
        public override string PasswordWasChangedSuccessfully { get { return "Lösenordet ändrat"; } }
        public override string UserAlreadyExists { get { return "Användaren finns redan"; } }
        public override string GroupAlreadyExists { get { return "Gruppen finns redan"; } }
        public override string UserFileIsLocked { get { return "Användarfil låst"; } }
        public override string UserFileIsLockedEnterReadOnly { get { return "Användarfil låst.\r\nVill du öppna i skrivskyddat läge?"; } }
        public override string ExpandTextBox { get { return "VIDGA"; } }
        public override string TextboxInsertMode { get { return "INFOGA"; } }
        public override string TextboxOverwriteMode { get { return "SKRIV ÖVER"; } }
        public override string ConfirmExitApplication { get { return "Vill du avsluta applikationen?"; } }
        public override string ExitApplication { get { return "Avsluta applikationen"; } }
        public override string Yes { get { return "&Ja"; } }
        public override string No { get { return "&Nej"; } }
        public override string YesHotKeys { get { return "jJ"; } }
        public override string NoHotKeys { get { return "nN"; } }
        public override string NullInOnlyOnePartOfDateTimePair { get { return "Null i del av datum-tid-par"; } }

        public override bool SupportVisualLogicalFunctions { get { return false; } }

        public override string GetErrorFor(DatabaseErrorType error)
        {
            switch (error)
            {
                case DatabaseErrorType.DataChangeFailed:
                    return "Uppdatering misslyckad";
                case DatabaseErrorType.DuplicateIndex:
                    return "Duplicerat index";
                case DatabaseErrorType.LockedRow:
                    return "Låst rad";
                case DatabaseErrorType.RowDoesNotExist:
                    return "[Posten försvunnen]";
                case DatabaseErrorType.RowWasChangedSinceLoaded:
                    return "[Posten har uppdaterats]";
                case DatabaseErrorType.ReadOnlyEntityUpdate:
                    return "Uppdatering av skrivskyddad entitet";
                default:
                    return "Okänt fel";
            }
        }
        public override string PleaseEnterIdAndPassword { get { return "Please enter your system user ID and password"; } }

        public override string LogonParameters
        {
            get { return "Inloggningsparametrar"; }
        }

        public override string StartOnRowWhereErrorRepositionToFirstRow { get { return "Posten hittades ej - positionerad på första"; } }

        public override string StartOnRowWhereError { get { return "Posten hittades ej - positionerad på nästa"; } }
        public override string Date { get { return "Datum"; } }
        public override string UserNotAuthorizedForThisApplication
        {
            get
            {
                return "Användaren saknar rättigheter för denna applikation";
            }
        }
    }
    public class GermanLocalizationInfo : LocalizationInfo
    {
        internal override char CharTypedByUserWithUFormatToUpper(char c)
        {
            return char.ToUpper(c);
        }
        public override System.Windows.Forms.RightToLeft RightToLeft { get { return System.Windows.Forms.RightToLeft.No; } }

        public override string DuplicateIndex { get { return "Datensatz existiert bereits (Doppelter Index)"; } }

        public override string LockedRow { get { return "Warten auf gesperrte Zeile, Datenquelle"; } }

        public override string TransactionRolledBack { get { return "Transaktion zurückgesetzt"; } }

        public override string IllegalActivity { get { return "Tätigkeit nicht erlaubt"; } }

        public override string RowNotFoundInEntity { get { return "Nicht gefundene Reihe im Feld"; } }

        public override string Supervisor { get { return "Supervisor"; } }

        public override string CreateAutomaticallyAsPartOfTheGeneration { get { return "Automatisch geschafft während die Konvertierung"; } }

        public override string DoYouWantToDelete { get { return "Bitte Löschen bestätigen"; } }

        public override string Ok { get { return "OK"; } }

        public override string Cancel { get { return "Abbrechen"; } }

        public override string Sort { get { return "Sortieren"; } }
        public override string Error { get { return "Fehler"; } }
        public override string Warning { get { return "Warnung"; } }

        public override string SortByIndex { get { return "Nach Index sortieren"; } }

        public override string Filter { get { return "Filter"; } }

        public override string Update { get { return "Ändern"; } }

        public override string Insert { get { return "Anlegen"; } }

        public override string Delete { get { return "Löschen"; } }

        public override string Browse { get { return "Abfragen"; } }

        public override string Options { get { return "Options"; } }

        public override string Users { get { return "Benutzer"; } }

        public override string Groups { get { return "Gruppen"; } }

        public override string Local { get { return "Virtuel"; } }

        public override string Parameter { get { return "Parameter"; } }

        public override string Expand { get { return "Zoomen"; } }

        public override string UserNotExistsOrPasswordWasWrong
        {
            get
            {
                return
                    "Ungültige Berechtigungsnachweise";
            }
        }
        public override string FailedToChangePassword
        {
            get
            {
                return
                    "Passwortänderung xxxx";
            }
        }
        public override string NewPassword { get { return "Neues Passwort"; } }
        public override string CurrentPassword { get { return "Aktuelles Passwort"; } }
        public override string ConfirmNewPassword { get { return "Neues Passwort bestätigen"; } }
        public override string ChangePassword { get { return "Passwort ändern"; } }
        public override string WrongCurrentPassword { get { return "Ungültiges aktuelles Passwort"; } }
        public override string NewPasswordSameAsOldPassword { get { return "Neue Passwort ist derselbe als das Gegenwärtige Passwort"; } }
        public override string ConfirmNewPasswordNotSameAsNewPassword { get { return "Bestätigten neue Passwort vergleicht das neue Passwort nicht"; } }
        public override string Exit { get { return "Verlassen"; } }

        public override string Name { get { return "Name"; } }

        public override string UserName { get { return "Benutzername"; } }

        public override string Password { get { return "Passwort"; } }

        public override string Login { get { return "\\Login"; } }

        public override string SystemLogin { get { return "Login"; } }

        public override string ThisRightAlreadyExists { get { return "Zugriffsrecht bereit existiert"; } }

        public override string Details { get { return "Einzelheit"; } }

        public override string UserGroups { get { return "Benutzergruppen"; } }

        public override string Roles { get { return "Rollennamen"; } }

        public override string RoleName { get { return "Zugriffsrechtname"; } }

        public override string Description { get { return "Beschreibung"; } }

        public override string Choose { get { return "Auswählen"; } }

        public override string ChooseRole { get { return "Wählen Sie Rollenname"; } }

        public override string AdditionalInfo { get { return "Zusätliches Info"; } }

        public override string IsEqualTo { get { return "Gleich als"; } }

        public override string GreaterOrEqual { get { return "Grösser oder Gleich dem"; } }

        public override string Greater { get { return "Grösser als"; } }

        public override string LessOrEqual { get { return "Weniger oder Gleich dem"; } }

        public override string LessThen { get { return "Weniger als"; } }

        public override string Between { get { return "Zwischen"; } }

        public override string Column { get { return "Spalte"; } }

        public override string FilterType { get { return "Filtertyp"; } }

        public override string Values { get { return "Werte"; } }

        public override string FilterRows { get { return "Bereich"; } }

        public override string FindARow { get { return "Legen"; } }

        public override string ConfirmUpdateMessage { get { return "Möchten Sie Aenderungen speichern?"; } }

        public override string ConfirmUpdateTitle { get { return "Aktualisierung bestätigen"; } }

        public override string DoYouWantToUndo { get { return "Undo bestätigen"; } }

        public override string Undo { get { return "Undo"; } }

        public override string ThisGroupAlreayExists
        {
            get
            {
                return
                    "Gruppe bereit existiert";
            }
        }
        public override string UserModuleNotLoaded { get { return "Benutzer-Modul nicht gefunden/geladen - {0}"; } }

        public override string InvalidChar
        {
            get
            {
                return
                    "Ungültiger Buschstaben - {0}";
            }
        }

        public override string InputDoesntMatchRange
        {
            get
            {
                return
                    "Ungültiger Eingang - {0} - das erlaubte Wert ist: {1}";
            }
        }

        public override string InvalidValue
        {
            get
            {
                return
                    "Ungültiger Eingang - {0} - das erlaubte Format ist: {1}";
            }
        }

        public override string ConfirmExecution { get { return "Bestätigen Sie Durchführung?"; } }

        public override string ExecutionCompleted { get { return "komplette Ausführung"; } }

        public override string ErrorInStartRun { get { return "Fehler  {0}"; } }

        public override string ControlMustBeUpdated { get { return "Kontrolle muss aktualisiert werden"; } }


        public override MessageBoxOptions MessageBoxOptions { get { return default(MessageBoxOptions); } }
        public override string Digits { get { return "Zahl"; } }
        public override string UpdateNotAllowedInBrowseMode { get { return "Aktualisierung ist nicht erlaubt, während durchsuchung"; } }
        public override string RowWasChanged { get { return "Datensatz wurde geändert"; } }
        public override string RowDoesNotExist { get { return "Reihe wurde verloren"; } }
        public override string ExportType { get { return "Typ"; } }
        public override string FileName { get { return "File Name"; } }
        public override string Template { get { return "Vorlage"; } }
        public override string ExportData { get { return "Export Daten"; } }
        public override string OpenFile { get { return "File eröffnen"; } }
        public override string Import { get { return "Import"; } }
        public override string ErrorInExportData { get { return "Fehler während Daten erzeugend"; } }
        public override string DefaultBoolInputRange { get {
                if (UserSettings.VersionXpaCompatible)
                    return base.DefaultBoolInputRange;
                return "Wahr, Falsch";
            } }
        public override string SecuredValues { get { return "Sichere Wert "; } }
        public override string NameAlreadyExist { get { return "Wert existiert "; } }
        public override string ReadOnlyEntityUpdate { get { return "Cannot update readonly entity"; } }
        public override string FilterExpression { get { return "Ausdruck"; } }
        public override string ErrorOpeningTable { get { return "Fehler Tabelle Eröffnung:"; } }
        public override string InvalidTableStructure { get { return "Tabellen definitions nicht übereinstimmung"; } }
        public override string UIControllerWithoutView { get { return "Aufgabe ohne Schirm"; } }
        public override string Delimiter { get { return "Delimiter"; } }
        public override string AvailableColumns { get { return "Verfügbar Spalte"; } }
        public override string SelectedColumns { get { return "Gewählte Spalte"; } }
        public override string InvalidDate { get { return "Invalid Datum"; } }
        public override string PasswordWasChangedSuccessfully { get { return "Passwort erfolgreich geändert"; } }
        public override string UserAlreadyExists { get { return "Benutzer bereit existiert"; } }
        public override string GroupAlreadyExists { get { return "Gruppe bereit existiert"; } }
        public override string UserFileIsLocked { get { return "Benutzer Datei eingesperrt"; } }
        public override string UserFileIsLockedEnterReadOnly { get { return "Benutzer Datei eingesperrt. Möchten Sie die readonly modus zugreifen?"; } }
        public override string ExpandTextBox { get { return "WIDE"; } }
        public override string TextboxInsertMode { get { return "INS"; } }
        public override string TextboxOverwriteMode { get { return "OVR"; } }
        public override string ConfirmExitApplication { get { return "Möchten Sie die Anwendung verlassen?"; } }
        public override string ExitApplication { get { return "Anwendung verlassen"; } }
        public override string Yes { get { return "&Ja"; } }
        public override string No { get { return "&Nein"; } }
        public override string YesHotKeys { get { return "JJ"; } }
        public override string NoHotKeys { get { return "nN"; } }
        public override string NullInOnlyOnePartOfDateTimePair { get { return "Null in only one part of date-time pair"; } }

        public override bool SupportVisualLogicalFunctions { get { return false; } }

        public override string GetErrorFor(DatabaseErrorType error)
        {
            switch (error)
            {
                case DatabaseErrorType.DataChangeFailed:
                    return "Aktualisierung Scheitert";
                case DatabaseErrorType.DuplicateIndex:
                    return "Index Duplikat";
                case DatabaseErrorType.LockedRow:
                    return "Eingesperrte Reihe";
                case DatabaseErrorType.RowDoesNotExist:
                    return "[Rekord wurde verloren]";
                case DatabaseErrorType.RowWasChangedSinceLoaded:
                    return "[Rekord wurde aktualisiert]";
                case DatabaseErrorType.ReadOnlyEntityUpdate:
                    return "Ready only Aktualisierung";
                default:
                    return "Unbekannte Error";
            }
        }
        public override string PleaseEnterIdAndPassword { get { return "Bitte user ID und Password eingeben "; } }

        public override string LogonParameters
        {
            get { return "Logon Parameters"; }
        }

        public override string StartOnRowWhereErrorRepositionToFirstRow { get { return "Rekord nicht gefunden - eingestellt zuerst"; } }

        public override string StartOnRowWhereError { get { return "Rekord nicht gefunden - eingestellt als nächstes"; } }
        public override string Date { get { return "Datum"; } }
        public override string UserNotAuthorizedForThisApplication
        {
            get
            {
                return "Benutzer wird für diese Anwendung nicht erlaubt";
            }
        }
        public override string PrintPreview { get { return "Druckansicht"; } }
        public override string Print { get { return "Drucken"; } }
        public override string PrevieAsWord { get { return " als "; } }
        public override string Save { get { return "Speichern"; } }
        public override string Send { get { return "Senden"; } }
        public override string View { get { return "Ansicht"; } }
        public override string OnePageView { get { return "Ansicht 1 Seite"; } }
        public override string TwoPageView { get { return "Ansicht 2 Seiten"; } }
        public override string FourPageView { get { return "Ansicht 3 Seiten"; } }
        public override string PrintPreviewFit { get { return "Ausrichten"; } }
        public override string PrintPreviewZoom { get { return "Zoom"; } }
        public override string Page { get { return "Seite"; } }
        public override string PrintPreviewOf { get { return "von "; } }
    }
    public class PolishLocalizationInfo : LocalizationInfo
    {
        internal override char CharTypedByUserWithUFormatToUpper(char c)
        {
            return char.ToUpper(c);
        }
        public override System.Windows.Forms.RightToLeft RightToLeft { get { return System.Windows.Forms.RightToLeft.No; } }

        public override string DuplicateIndex { get { return "Powtórzona wartość klucza unikalnego"; } }

        public override string LockedRow { get { return "Bieżący rekord jest zablokowany"; } }


        public override string TransactionRolledBack { get { return "Transakcja została wycofana"; } }

        public override string IllegalActivity { get { return "Czynność niedozwolona"; } }

        public override string RowNotFoundInEntity { get { return "Nie znaleziono rekordu"; } }

        public override string Supervisor { get { return "Supervisor"; } }

        public override string CreateAutomaticallyAsPartOfTheGeneration { get { return "Create automatically as part of the generation"; } }

        public override string DoYouWantToDelete { get { return "Usunąć ?"; } }

        public override string Ok { get { return "OK"; } }

        public override string Cancel { get { return "\\Anuluj"; } }

        public override string Sort { get { return "Sort"; } }

        public override string Error { get { return "Błąd"; } }

        public override string Warning { get { return "Ostrzeżenie"; } }

        public override string SortByIndex { get { return "Sort wg indeksu"; } }

        public override string Filter { get { return "Filtr"; } }

        public override string Update { get { return "Zmień"; } }

        public override string Insert { get { return "Twórz"; } }

        public override string Delete { get { return "Usuń"; } }

        public override string Browse { get { return "Przeglądanie"; } }

        public override string Options { get { return "Opcje"; } }

        public override string Users { get { return "Użytkownicy"; } }

        public override string Groups { get { return "Grupy"; } }


        public override string Local { get { return "Wirtualny"; } }


        public override string Parameter { get { return "Parameter"; } }

        public override string Expand { get { return "Zoom"; } }

        public override string UserNotExistsOrPasswordWasWrong
        {
            get
            {
                return
                    "Nieprawidłowe poświadczenia";

            }
        }
        public override string FailedToChangePassword
        {
            get
            {
                return
                "Zmiana hasła nie została wykonana";
            }
        }

        public override string NewPassword { get { return "Nowe hasło"; } }


        public override string CurrentPassword { get { return "Bieżące hasło"; } }


        public override string ConfirmNewPassword { get { return "Potwierdź nowe hasło"; } }


        public override string ChangePassword { get { return "Zmień hasło"; } }


        public override string WrongCurrentPassword { get { return "Niepoprawne hasło bieżące"; } }


        public override string NewPasswordSameAsOldPassword { get { return "Nowe hasło jest takie samo jak bieżące"; } }


        public override string ConfirmNewPasswordNotSameAsNewPassword { get { return "Potwierdzone nowe hasło nie zgadza sie z wprowadzonym nowym hasłem "; } }


        public override string Exit { get { return "Wyjście"; } }


        public override string Name { get { return "Nazwa"; } }


        public override string UserName { get { return "Użytkownik ID"; } }


        public override string Password { get { return "Hasło"; } }

        public override string Login { get { return "\\Login"; } }

        public override string SystemLogin { get { return "Login"; } }


        public override string ThisRightAlreadyExists { get { return "To prawo już istnieje"; } }

        public override string Details { get { return "Szczegóły"; } }


        public override string UserGroups { get { return "Grupy użytkowników"; } }


        public override string Roles { get { return "Role"; } }


        public override string RoleName { get { return "Nazwa roli"; } }


        public override string Description { get { return "Opis"; } }


        public override string Choose { get { return "Wybierz"; } }


        public override string ChooseRole { get { return "Wybierz rolę"; } }


        public override string AdditionalInfo { get { return "Dodatkowe informacje"; } }


        public override string IsEqualTo { get { return "Równe"; } }


        public override string GreaterOrEqual { get { return "Większe lub równe"; } }


        public override string Greater { get { return "Większe niż"; } }


        public override string LessOrEqual { get { return "Mniejsze lub równe"; } }


        public override string LessThen { get { return "Mniejsze niż"; } }


        public override string Between { get { return "Między"; } }


        public override string Column { get { return "Kolumna"; } }


        public override string FilterType { get { return "Typ filtra"; } }


        public override string Values { get { return "Wartości"; } }


        public override string FilterRows { get { return "Rng"; } }


        public override string FindARow { get { return "Loc"; } }


        public override string ConfirmUpdateMessage { get { return "Zapisać zmiany ?"; } }


        public override string ConfirmUpdateTitle { get { return "Potwierdź modyfikację"; } }


        public override string DoYouWantToUndo { get { return "Potwierdź wycofanie zmian"; } }


        public override string Undo { get { return "Wycofanie zmian"; } }

        public override string ThisGroupAlreayExists
        {
            get
            {
                return
                "Grupa już istnieje";
            }
        }
        public override string UserModuleNotLoaded { get { return "User module not found/loaded - {0}"; } }

        public override string InvalidChar
        {
            get
            {
                return
                "Niepoprawny znak - {0}";
            }
        }

        public override string InputDoesntMatchRange
        {
            get
            {
                return
                    "Niepoprawne dane wejściowe - {0} - dozwolone wartości: {1}";
            }
        }

        public override string InvalidValue
        {
            get
            {
                return

                    "Niepoprawne dane wejściowe - {0} - dozwolony format: {1}";
            }
        }

        public override string ConfirmExecution { get { return "Potwierdź uruchomienie ?"; } }

        public override string ExecutionCompleted { get { return "Zakończone"; } }

        public override string ErrorInStartRun { get { return "Error Loading {0}"; } }

        public override string ControlMustBeUpdated { get { return "Control must be updated"; } }
        public override MessageBoxOptions MessageBoxOptions { get { return default(MessageBoxOptions); } }


        public override string Digits { get { return "cyfr"; } }

        public override string UpdateNotAllowedInBrowseMode { get { return "Modyfikacja niedozwolona w trybie przeglądania"; } }

        public override string RowWasChanged { get { return "Wiersz został zmieniony"; } }

        public override string RowDoesNotExist { get { return "Wiersz został utracony"; } }


        public override string ExportType { get { return "Typ"; } }


        public override string FileName { get { return "Nazwa pliku"; } }


        public override string Template { get { return "Szablon"; } }


        public override string ExportData { get { return "Eksport danych"; } }


        public override string OpenFile { get { return "Otwórz plik"; } }

        public override string Import { get { return "Import"; } }


        public override string ErrorInExportData { get { return "Błąd podczas generowania danych"; } }

        public override string DefaultBoolInputRange { get { return "True,False"; } }


        public override string SecuredValues { get { return "Wartości chronione"; } }


        public override string NameAlreadyExist { get { return "Powtórzone dane"; } }


        public override string ReadOnlyEntityUpdate { get { return "Próba modyfikacji danych tylko do odczytu"; } }


        public override string FilterExpression { get { return "Wyrażenie"; } }


        public override string ErrorOpeningTable { get { return "Błąd otwarcia tabeli:"; } }


        public override string InvalidTableStructure { get { return "Błąd definicji struktury tabeli"; } }


        public override string UIControllerWithoutView { get { return "Zadanie bez okna"; } }


        public override string Delimiter { get { return "Znak rozdzielający"; } }


        public override string AvailableColumns { get { return "Dostępne kolumny"; } }


        public override string SelectedColumns { get { return "Wybrane kolumny"; } }



        public override string InvalidDate { get { return "Błędna data"; } }


        public override string PasswordWasChangedSuccessfully { get { return "Hasło zostało zmienione"; } }


        public override string UserAlreadyExists { get { return "Użytkownik już istnieje"; } }


        public override string GroupAlreadyExists { get { return "Grupa już istnieje"; } }


        public override string UserFileIsLocked { get { return "Plik użytkownika jest zablokowany"; } }


        public override string UserFileIsLockedEnterReadOnly { get { return "Plik użytkownika jest zablokowany.\r\nCzy chcesz go otworzyć w trybie tylko do odczytu?"; } }



        public override string ExpandTextBox { get { return "SZEROKI"; } }

        public override string TextboxInsertMode { get { return "INS"; } }

        public override string TextboxOverwriteMode { get { return "OVR"; } }


        public override string ConfirmExitApplication { get { return "Czy chcesz wyjść z aplikacji?"; } }


        public override string ExitApplication { get { return "Wyjście z aplikacji"; } }


        public override string Yes { get { return "&Tak"; } }
        public override string No { get { return "&Nie"; } }
        public override string YesHotKeys { get { return "tT"; } }
        public override string NoHotKeys { get { return "nN"; } }




        public override string NullInOnlyOnePartOfDateTimePair { get { return "Null in only one part of date-time pair"; } }

        public override bool SupportVisualLogicalFunctions { get { return false; } }

        public override string GetErrorFor(DatabaseErrorType error)
        {
            switch (error)
            {
                case DatabaseErrorType.DataChangeFailed:
                    return "Modyfikacja nieudana";
                case DatabaseErrorType.DuplicateIndex:
                    return "Duplikat indeksu";
                case DatabaseErrorType.LockedRow:
                    return "Wiersz zablokowany";
                case DatabaseErrorType.RowDoesNotExist:
                    return "[Wiersz utracony]";
                case DatabaseErrorType.RowWasChangedSinceLoaded:
                    return "[Wiersz został zmodyfikowany w tle]";
                case DatabaseErrorType.ReadOnlyEntityUpdate:
                    return "ReadOnly Entity Update";
                default:
                    return "Nieznany błąd";
            }
        }


        public override string PleaseEnterIdAndPassword { get { return "Proszę podać Twój identyfikator i hasło"; } }

        public override string LogonParameters
        {

            get { return "Parametry logowania"; }
        }


        public override string StartOnRowWhereErrorRepositionToFirstRow { get { return "Wiersz nie znaleziony - ustawiono na pierwszym"; } }


        public override string StartOnRowWhereError { get { return "Wiersz nie znaleziony - ustawiono na następnym"; } }


        public override string Date { get { return "Data"; } }

        public override string UserNotAuthorizedForThisApplication
        {
            get
            {
                return "Użytkownik nie jest zalogowany do aplikacji";

            }
        }


        public override string UserIsNotAuthorized { get { return "Użytkownik nie jest zalogowany"; } }


        public override string HelpRequestFailed { get { return "Help request failed '{0}'"; } }
        public override Text.ITextComparer GetCaseInsensitiveTextComparer()
        {
            return new Common.CultureAwareIgnoreCaseTextComparer();
        }
    }

    public class DutchLocalizationInfo : LocalizationInfo
    {
        internal override char CharTypedByUserWithUFormatToUpper(char c)
        {
            return char.ToUpper(c);
        }

        public override System.Windows.Forms.RightToLeft RightToLeft { get { return System.Windows.Forms.RightToLeft.No; } }

        public override string DuplicateIndex { get { return "Dubbele index - Er is een record met dezelfde index waarde(n)"; } }

        public override string LockedRow { get { return "Huidig record is gelocked, wacht op vrijgave"; } }

        public override string TransactionRolledBack { get { return "Transactie teruggedraaid"; } }

        public override string IllegalActivity { get { return "Activiteit niet toegestaan"; } }

        public override string RowNotFoundInEntity { get { return "Record niet aanwezig in tabel"; } }

        public override string Supervisor { get { return "Supervisor"; } }

        public override string CreateAutomaticallyAsPartOfTheGeneration { get { return "Create automatically as part of the generation"; } }

        public override string DoYouWantToDelete { get { return "Wilt u het record verwijderen?"; } }

        public override string Ok { get { return "OK"; } }

        public override string Cancel { get { return "Cancel"; } }

        public override string Sort { get { return "Sort"; } }
        public override string Error { get { return "Fout"; } }
        public override string Warning { get { return "Waarschuwing"; } }

        public override string SortByIndex { get { return "Sortering op Index"; } }

        public override string Filter { get { return "Filter"; } }

        public override string Update { get { return "Wijzigen"; } }

        public override string Insert { get { return "Nieuw"; } }

        public override string Delete { get { return "Verwijderen"; } }

        public override string Browse { get { return "Bekijken"; } }

        public override string Options { get { return "Opties"; } }

        public override string Users { get { return "Gebruikers"; } }

        public override string Groups { get { return "Groepen"; } }

        public override string Local { get { return "Virtual"; } }

        public override string Parameter { get { return "Parameter"; } }

        public override string Expand { get { return "Zoom"; } }

        public override string UserNotExistsOrPasswordWasWrong
        {
            get
            {
                return
                    "Ongeldige inloggegevens";
            }
        }
        public override string FailedToChangePassword
        {
            get
            {
                return
                    "Wachtwoord wijzigen mislukt";
            }
        }
        public override string NewPassword { get { return "Nieuw wachtwoord"; } }
        public override string CurrentPassword { get { return "Huidig wachtwoord"; } }
        public override string ConfirmNewPassword { get { return "Bevestig nieuw wachtwoord"; } }
        public override string ChangePassword { get { return "Wijzig wachtwoord"; } }
        public override string WrongCurrentPassword { get { return "Ongeldig nieuw wachtwoord"; } }
        public override string NewPasswordSameAsOldPassword { get { return "Het nieuwe wachtwoord is gelijk aan het huidige wachtwoord"; } }
        public override string ConfirmNewPasswordNotSameAsNewPassword { get { return "Wachtwoord niet gelijk"; } }
        public override string Exit { get { return "Exit"; } }

        public override string Name { get { return "Naam"; } }

        public override string UserName { get { return "Gebruiker-ID"; } }

        public override string Password { get { return "Wachtwoord"; } }

        public override string Login { get { return "\\Login"; } }

        public override string SystemLogin { get { return "Login"; } }

        public override string ThisRightAlreadyExists { get { return "Het recht is al aanwezig"; } }

        public override string Details { get { return "Details"; } }

        public override string UserGroups { get { return "Gebruikersgroepen"; } }

        public override string Roles { get { return "Rollen"; } }

        public override string RoleName { get { return "Rolnaam"; } }

        public override string Description { get { return "Omschrijving"; } }

        public override string Choose { get { return "Kies"; } }

        public override string ChooseRole { get { return "Kies rol"; } }

        public override string AdditionalInfo { get { return "Additionele Info"; } }

        public override string IsEqualTo { get { return "Gelijk aan"; } }

        public override string GreaterOrEqual { get { return "Groter dan of gelijk aan"; } }

        public override string Greater { get { return "Groter dan"; } }

        public override string LessOrEqual { get { return "Kleiner dan of gelijk aan"; } }

        public override string LessThen { get { return "Kleiner dan"; } }

        public override string Between { get { return "Tussen"; } }

        public override string Column { get { return "Kolom"; } }

        public override string FilterType { get { return "Filter Type"; } }

        public override string Values { get { return "Waarden"; } }

        public override string FilterRows { get { return "Bereik"; } }

        public override string FindARow { get { return "Zoeken"; } }

        public override string ConfirmUpdateMessage { get { return "Wilt u de wijzigingen opslaan?"; } }

        public override string ConfirmUpdateTitle { get { return "Bevestig wijziging"; } }

        public override string DoYouWantToUndo { get { return "Bevestig ongedaan maken"; } }

        public override string Undo { get { return "Ongedaan maken"; } }

        public override string ThisGroupAlreayExists
        {
            get
            {
                return
                    "Deze groep bestaal al";
            }
        }
        public override string UserModuleNotLoaded { get { return "User module niet aanwezig - {0}"; } }

        public override string InvalidChar
        {
            get
            {
                return
                    "Ongeldig teken - {0}";
            }
        }

        public override string InputDoesntMatchRange
        {
            get
            {
                return
                    "Ongeldige invoer - {0} - Toegestaan is:  {1}";
            }
        }

        public override string InvalidValue
        {
            get
            {
                return
                    "Ongeldige invoer - {0} - Toegestaan formaat is: {1}";
            }
        }

        public override string ConfirmExecution { get { return "Bevestig uitvoer"; } }

        public override string ExecutionCompleted { get { return "Uitvoer gereed"; } }

        public override string ErrorInStartRun { get { return "Fout in laden  {0}"; } }
        public override string ControlMustBeUpdated { get { return "Gegeven moet bijgewerkt worden"; } }
        public override MessageBoxOptions MessageBoxOptions { get { return default(MessageBoxOptions); } }
        public override string Digits { get { return "cijfers"; } }
        public override string UpdateNotAllowedInBrowseMode { get { return "Bijwerken is niet toegestaan in Bekijk-mode"; } }
        public override string RowWasChanged { get { return "Record was gewijzigd"; } }
        public override string RowDoesNotExist { get { return "Record is niet (meer) aanwezig"; } }
        public override string ExportType { get { return "Type"; } }
        public override string FileName { get { return "Naam"; } }
        public override string Template { get { return "Template"; } }
        public override string ExportData { get { return "Export Data"; } }
        public override string OpenFile { get { return "Open File"; } }
        public override string Import { get { return "Import"; } }
        public override string ErrorInExportData { get { return "Fout tijdens genereren"; } }
        public override string DefaultBoolInputRange { get { return "True,False"; } }
        public override string SecuredValues { get { return "Secured Values"; } }
        public override string NameAlreadyExist { get { return "Waarde bestaat al"; } }
        public override string ReadOnlyEntityUpdate { get { return "Kan ReadOnly data niet wijzigen"; } }
        public override string FilterExpression { get { return "Expressie"; } }
        public override string ErrorOpeningTable { get { return "Fout in openen tabel:"; } }
        public override string InvalidTableStructure { get { return "Table definition mismatch"; } }
        public override string UIControllerWithoutView { get { return "Taak zonder scherm"; } }
        public override string Delimiter { get { return "Scheidingsteken"; } }
        public override string AvailableColumns { get { return "Beschikbare kolommen"; } }
        public override string SelectedColumns { get { return "Geselecteerde kolommen"; } }
        public override string InvalidDate { get { return "Ongeldige datum"; } }
        public override string PasswordWasChangedSuccessfully { get { return "Wachtwoord succesvol gewijzigd"; } }
        public override string UserAlreadyExists { get { return "Gebruiker-ID bestaat al"; } }
        public override string GroupAlreadyExists { get { return "Groep-ID bestaal al"; } }
        public override string UserFileIsLocked { get { return "Gebruikersbestand is gelocked"; } }
        public override string UserFileIsLockedEnterReadOnly { get { return "Gebruikersbestand is gelocked.\r\nWilt u alleen bekijken?"; } }
        public override string ExpandTextBox { get { return "WIDE"; } }
        public override string TextboxInsertMode { get { return "INS"; } }
        public override string TextboxOverwriteMode { get { return "OVR"; } }
        public override string ConfirmExitApplication { get { return "Wilt u de applicatie sluiten?"; } }
        public override string ExitApplication { get { return "Sluiten"; } }
        public override string Yes { get { return "&Yes"; } }
        public override string No { get { return "&No"; } }
        public override string YesHotKeys { get { return "jJyY"; } }
        public override string NoHotKeys { get { return "nN"; } }
        public override string NullInOnlyOnePartOfDateTimePair { get { return "Null in only one part of date-time pair"; } }

        public override bool SupportVisualLogicalFunctions { get { return false; } }

        public override string GetErrorFor(DatabaseErrorType error)
        {
            switch (error)
            {
                case DatabaseErrorType.DataChangeFailed:
                    return "Update failed";
                case DatabaseErrorType.DuplicateIndex:
                    return "Duplicate Index";
                case DatabaseErrorType.LockedRow:
                    return "Locked Row";
                case DatabaseErrorType.RowDoesNotExist:
                    return "[Record lost]";
                case DatabaseErrorType.RowWasChangedSinceLoaded:
                    return "[Record has been updated]";
                case DatabaseErrorType.ReadOnlyEntityUpdate:
                    return "ReadOnly Entity Update";
                default:
                    return "Unknown Error";
            }
        }
        public override string PleaseEnterIdAndPassword { get { return "Please enter your system user ID and password"; } }

        public override string LogonParameters
        {
            get { return "Logon Parameters"; }
        }

        public override string StartOnRowWhereErrorRepositionToFirstRow { get { return "Record niet aanwezig, gepositioneerd op begin"; } }

        public override string StartOnRowWhereError { get { return "Record niet aanwezig, gepositioneerd op volgende"; } }
        public override string Date { get { return "Datum"; } }
        public override string UserNotAuthorizedForThisApplication
        {
            get
            {
                return "Gebruiker niet geautoriseerd";
            }
        }

        public override string UserIsNotAuthorized { get { return "Gebruiker niet geautoriseerd"; } }
        public override string HelpRequestFailed { get { return "Help request failed '{0}'"; } }
        public override Text.ITextComparer GetCaseInsensitiveTextComparer()
        {
            return new Common.CultureAwareIgnoreCaseTextComparer();
        }
    }

    public class NorwegianLocalizationInfo : LocalizationInfo
    {
        internal override char CharTypedByUserWithUFormatToUpper(char c)
        {
            return char.ToUpper(c);
        }
        public override System.Windows.Forms.RightToLeft RightToLeft { get { return System.Windows.Forms.RightToLeft.No; } }

        public override string DuplicateIndex { get { return "Dupliseringsindeks - Det finnes alt en post med samme indeksverdi(n)"; } }

        public override string LockedRow { get { return "Aktuell post er låst"; } }

        public override string TransactionRolledBack { get { return "Transaksjonen er reservert"; } }

        public override string IllegalActivity { get { return "Aktivitet er ikke tillatt"; } }

        public override string RowNotFoundInEntity { get { return "Posten finnes ikke i tabellen"; } }

        public override string Supervisor { get { return "Supervisor"; } }

        public override string CreateAutomaticallyAsPartOfTheGeneration { get { return "Opprett automatisk som en del av oppgaven"; } }

        public override string DoYouWantToDelete { get { return "Ønsker du å slette posten?"; } }

        public override string Ok { get { return "OK"; } }

        public override string Cancel { get { return "Avbryt"; } }

        public override string Sort { get { return "Sorter"; } }
        public override string Error { get { return "Feil"; } }
        public override string Warning { get { return "Advarsel"; } }

        public override string SortByIndex { get { return "Sortering etter index"; } }

        public override string Filter { get { return "Filter"; } }

        public override string Update { get { return "Oppdater"; } }

        public override string Insert { get { return "Sett inn"; } }

        public override string Delete { get { return "Slett"; } }

        public override string Browse { get { return "Let"; } }

        public override string Options { get { return "Valg"; } }

        public override string Users { get { return "Brukere"; } }

        public override string Groups { get { return "Grupper"; } }

        public override string Local { get { return "Lokal"; } }

        public override string Parameter { get { return "Parameter"; } }

        public override string Expand { get { return "Zoom"; } }

        public override string UserNotExistsOrPasswordWasWrong
        {
            get
            {
                return
                    "Ugyldige innloggingsdetaljer";
            }
        }
        public override string FailedToChangePassword
        {
            get
            {
                return
                    "Endring av passord mislyktes";
            }
        }
        public override string NewPassword { get { return "Nytt passord"; } }
        public override string CurrentPassword { get { return "Gjeldende passord"; } }
        public override string ConfirmNewPassword { get { return "Bekreft nytt passord"; } }
        public override string ChangePassword { get { return "Endre passord"; } }
        public override string WrongCurrentPassword { get { return "Feil nåværende passord"; } }
        public override string NewPasswordSameAsOldPassword { get { return "Det nye passordet er det samme som det nåværende passordet"; } }
        public override string ConfirmNewPasswordNotSameAsNewPassword { get { return "Passordene er ikke like"; } }
        public override string Exit { get { return "Avslutt"; } }

        public override string Name { get { return "Navn"; } }

        public override string UserName { get { return "Bruker ID"; } }

        public override string Password { get { return "Passord"; } }

        public override string Login { get { return "\\Logg inn"; } }

        public override string SystemLogin { get { return "Logg inn"; } }

        public override string ThisRightAlreadyExists { get { return "Denne rettigheten eksisterer allerede"; } }

        public override string Details { get { return "Detaljer"; } }

        public override string UserGroups { get { return "Brukergrupper"; } }

        public override string Roles { get { return "Rolle"; } }

        public override string RoleName { get { return "Rollenavn"; } }

        public override string Description { get { return "Beskrivelse"; } }

        public override string Choose { get { return "Velg"; } }

        public override string ChooseRole { get { return "Velg rolle"; } }

        public override string AdditionalInfo { get { return "Tilleggsinformasjon"; } }

        public override string IsEqualTo { get { return "Er lik som"; } }

        public override string GreaterOrEqual { get { return "Større enn eller lik"; } }

        public override string Greater { get { return "Større enn"; } }

        public override string LessOrEqual { get { return "Mindre enn eller lik"; } }

        public override string LessThen { get { return "Mindre enn"; } }

        public override string Between { get { return "Mellom"; } }

        public override string Column { get { return "Kolonne"; } }

        public override string FilterType { get { return "Filtertype"; } }

        public override string Values { get { return "Verdier"; } }

        public override string FilterRows { get { return "Filtrer rader"; } }

        public override string FindARow { get { return "Finn en rad"; } }

        public override string ConfirmUpdateMessage { get { return "Ønsker du å lagre endringene?"; } }

        public override string ConfirmUpdateTitle { get { return "Bekreft endring"; } }

        public override string DoYouWantToUndo { get { return "Bekreft angre"; } }

        public override string Undo { get { return "Angre"; } }

        public override string ThisGroupAlreayExists
        {
            get
            {
                return
                    "Denne gruppen eksisterer allerede";
            }
        }
        public override string UserModuleNotLoaded { get { return "Brukermodulen finnes ikke - {0}"; } }

        public override string InvalidChar
        {
            get
            {
                return
                    "Ugyldig tegn - {0}";
            }
        }

        public override string InputDoesntMatchRange
        {
            get
            {
                return
                    "Ugyldig oppføring - {0} - tillat:  {1}";
            }
        }

        public override string InvalidValue
        {
            get
            {
                return
                    "Ugyldig oppføring - {0} - Tillatt format er: {1}";
            }
        }

        public override string ConfirmExecution { get { return "Bekreft kjøring"; } }

        public override string ExecutionCompleted { get { return "Kjøring klar"; } }

        public override string ErrorInStartRun { get { return "Feil ved innlasting  {0}"; } }
        public override string ControlMustBeUpdated { get { return "Dataene må oppdateres"; } }
        public override MessageBoxOptions MessageBoxOptions { get { return default(MessageBoxOptions); } }
        public override string Digits { get { return "Siffer"; } }
        public override string UpdateNotAllowedInBrowseMode { get { return "Oppdatering er ikke tillatt i oppslagsmodus"; } }
        public override string RowWasChanged { get { return "Posten ble endret"; } }
        public override string RowDoesNotExist { get { return "Posten eksisterer ikke lenger"; } }
        public override string ExportType { get { return "Eksporttype"; } }
        public override string FileName { get { return "Filnavn"; } }
        public override string Template { get { return "Mal"; } }
        public override string ExportData { get { return "Exporter data"; } }
        public override string OpenFile { get { return "Åpne fil"; } }
        public override string Import { get { return "Importer"; } }
        public override string ErrorInExportData { get { return "Feil ved eksportering"; } }
        public override string DefaultBoolInputRange { get { return "Sant,Falsk"; } }
        public override string SecuredValues { get { return "Sikre verdier"; } }
        public override string NameAlreadyExist { get { return "Verdi eksisterer allerede"; } }
        public override string ReadOnlyEntityUpdate { get { return "Kan ikke endre ReadOnly-data"; } }
        public override string FilterExpression { get { return "Uttrykk"; } }
        public override string ErrorOpeningTable { get { return "Feil ved åpning av tabellen:"; } }
        public override string InvalidTableStructure { get { return "Feil i tabelldefinisjonen"; } }
        public override string UIControllerWithoutView { get { return "Taak zonder scherm"; } }
        public override string Delimiter { get { return "Separasjonstegn"; } }
        public override string AvailableColumns { get { return "Tilgjenglige kolonner"; } }
        public override string SelectedColumns { get { return "Valgt kolonne"; } }
        public override string InvalidDate { get { return "Ugyldig dato"; } }
        public override string PasswordWasChangedSuccessfully { get { return "Passordet ble endret"; } }
        public override string UserAlreadyExists { get { return "Bruker ID finnes allerede"; } }
        public override string GroupAlreadyExists { get { return "Gruppe ID finnes allerede"; } }
        public override string UserFileIsLocked { get { return "Brukerfilen er låst"; } }
        public override string UserFileIsLockedEnterReadOnly { get { return "Brukerfilen er låst.\r\n Gå til visningsmodus?"; } }
        public override string ExpandTextBox { get { return "Ekspander"; } }
        public override string TextboxInsertMode { get { return "Sett inn"; } }
        public override string TextboxOverwriteMode { get { return "Skriv over"; } }
        public override string ConfirmExitApplication { get { return "Vil du lukke programmet?"; } }
        public override string ExitApplication { get { return "Avslutt program"; } }
        public override string Yes { get { return "&Ja"; } }
        public override string No { get { return "&Nei"; } }
        public override string YesHotKeys { get { return "jJ"; } }
        public override string NoHotKeys { get { return "nN"; } }
        public override string NullInOnlyOnePartOfDateTimePair { get { return "Ingen verdi i dato eller tid"; } }

        public override bool SupportVisualLogicalFunctions { get { return false; } }

        public override string GetErrorFor(DatabaseErrorType error)
        {
            switch (error)
            {
                case DatabaseErrorType.DataChangeFailed:
                    return "Oppdatering feilet";
                case DatabaseErrorType.DuplicateIndex:
                    return "Duplisert index";
                case DatabaseErrorType.LockedRow:
                    return "Låst post";
                case DatabaseErrorType.RowDoesNotExist:
                    return "[Post eksisterer ikke]";
                case DatabaseErrorType.RowWasChangedSinceLoaded:
                    return "[Post har blitt oppdatert siden lasting]";
                case DatabaseErrorType.ReadOnlyEntityUpdate:
                    return "Post har kun lesetilgang";
                default:
                    return "Ukjent feil";
            }
        }
        public override string PleaseEnterIdAndPassword { get { return "Vennligst skriv inn brukernavn og passord"; } }

        public override string LogonParameters
        {
            get { return "Påloggingsparametre"; }
        }

        public override string StartOnRowWhereErrorRepositionToFirstRow { get { return "Post finnes ikke, fokus settes til første post"; } }

        public override string StartOnRowWhereError { get { return "Post finnes ikke, fokus settes til neste post"; } }
        public override string Date { get { return "Dato"; } }
        public override string UserNotAuthorizedForThisApplication
        {
            get
            {
                return "Bruker ikke autorisert";
            }
        }

        public override string UserIsNotAuthorized { get { return "Bruker har ikke autorisasjon"; } }
        public override string HelpRequestFailed { get { return "Hjelp forespørselen mislyktes '{0}'"; } }
        public override Text.ITextComparer GetCaseInsensitiveTextComparer()
        {
            return new Common.CultureAwareIgnoreCaseTextComparer();
        }
    }

}


