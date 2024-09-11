using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Firefly.Box;
using Firefly.Box.Data;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;
using Firefly.Box.UI.Advanced;

namespace ENV
{
    public static class JapaneseMethods
    {
        public static string MatchForJapaneseLength(string text, string format)
        {
            if (!string.IsNullOrEmpty(format))
            {
                var fi = new Firefly.Box.Data.Advanced.TextFormatInfo(format);
                text = MatchForJapaneseLength(text, fi);
            }
            return text;
        }

        public static Text MatchForJapaneseLength(Text text, TextFormatInfo fi)
        {
            if (!string.IsNullOrEmpty(text) && fi.MaxDataLength > 0 && !text.Contains("$"))
                text = JapaneseMethods.Mid(text, 0, fi.MaxDataLength);
            return text;
        }

        static Encoding _enc = System.Text.Encoding.GetEncoding(932);
        static bool _enabled;

        public static Text Mid(Text text, Number fromPosition, Number length)
        {
            if (fromPosition < 1)
                fromPosition = 1;
            var result = new List<byte>();
            int charsSoFar = 1;
            foreach (var c in text.ToString().ToCharArray())
            {
                var ba = _enc.GetBytes(new Char[] { c });
                if (result.Count>0&& result.Count + ba.Length > length)
                    break;
                if (charsSoFar >= fromPosition)
                    result.AddRange(ba);
                else if (ba.Length > 1 && charsSoFar + ba.Length > fromPosition)
                    result.Add(32);
                charsSoFar += ba.Length;

            }
            return _enc.GetString(result.ToArray());

        }

        public static int GetTextByteLength(string text)
        {
            return _enc.GetBytes(text).Length;
        }
        public static ImeMode TranslateImeMode(Time x) {
            return TranslateImeMode (ENV.UserMethods.Instance.ToNumber(x));
        }
        public static ImeMode TranslateImeMode(int x)
        {
            return x == 0
                ? ImeMode.Off
                : x == 1 || x == 2
                    ? ImeMode.Hiragana
                    : x == 3 || x == 4
                        ? ImeMode.Katakana
                        : x == 5 || x == 6
                            ? ImeMode.KatakanaHalf
                            : x == 7
                                ? ImeMode.AlphaFull
                                : x == 8 ? ImeMode.Alpha : ImeMode.NoControl;
        }

        public static Text Rep(Text originalText, Text textToInsert, Number position, Number length)
        {
            if (position < 1)
                position = 1;
            var result = _enc.GetBytes(originalText);
            var b = _enc.GetBytes(textToInsert);
            Array.Copy(b, 0, result, position - 1, length);
            return _enc.GetString(result);

        }
        public static bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                Firefly.Box.Printing.Advanced.PageDrawerClass.FlatLineCaps = _enabled;
            }
        }

        public static string HandleControlText(string text, string format, ControlData data)
        {
            if (Enabled)

                if (data != null)
                {
                    var x = data.Column as TextColumn;
                    if (x != null && !string.IsNullOrEmpty(format) && x.Format != format)
                    {
                        var fi = new TextFormatInfo(format);
                        if (fi.MaxDataLength != 0 && fi.MaxDataLength != data.Column.FormatInfo.MaxLength)
                            return Mid(text, 1, fi.MaxDataLength);
                    }
                }
            return text;
        }

        public static Text Zen(Text str)
        {
            return ToFullWidth(str.TrimEnd()).Replace((char)12288, ' ');
        }

        public static Text Han(Text str)
        {
            return ToHalfWidth(str.TrimEnd());
        }


        private const uint LOCALE_SYSTEM_DEFAULT = 0x0800;
        private const uint LCMAP_HALFWIDTH = 0x00400000;

        public static string ToHalfWidth(string fullWidth)
        {
            StringBuilder sb = new StringBuilder(256);
            LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_HALFWIDTH, fullWidth, -1, sb, sb.Capacity);
            return sb.ToString();
        }
        private const uint LCMAP_FULLWIDTH = 0x00800000;

        public static string ToFullWidth(string halfWidth)
        {
            StringBuilder sb = new StringBuilder(256);
            LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_FULLWIDTH, halfWidth, -1, sb, sb.Capacity);
            return sb.ToString();
        }


        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int LCMapString(uint Locale, uint dwMapFlags, string lpSrcStr, int cchSrc, StringBuilder lpDestStr, int cchDest);

        public static string TranslatePaste(string fullText, string selectedText, string pastedText, int maxLength)
        {
            return Mid(pastedText, 0, maxLength - GetTextByteLength(fullText) + GetTextByteLength(selectedText));
        }
    }
    public class JapaneseLocalizationInfo : ENV.LocalizationInfo
    {

        public override string DuplicateIndex { get { return "インデックスが重複しています"; } }

        public override string LockedRow { get { return "レコードロック解除待ちです"; } }

        public override string TransactionRolledBack { get { return "トランザクションロールバック"; } }

        public override string IllegalActivity { get { return "アクティビティは許可されていません。"; } }

        public override string RowNotFoundInEntity { get { return "該当レコードは存在しませんでした。"; } }

        public override string Supervisor { get { return "Supervisor"; } }

        public override string CreateAutomaticallyAsPartOfTheGeneration { get { return "生成の一部として自動的に作成します。"; } }

        public override string DoYouWantToDelete { get { return "削除しますか？"; } }

        public override string Ok { get { return "確定"; } }

        public override string Cancel { get { return "キャンセル"; } }

        public override string Sort { get { return "ソート"; } }
        public override string Error { get { return "エラー"; } }
        public override string Warning { get { return "警告"; } }

        public override string SortByIndex { get { return "インデックスでソートする"; } }

        public override string Filter { get { return "フィルター"; } }

        public override string Update { get { return "修正"; } }

        public override string Insert { get { return "登録"; } }

        public override string Delete { get { return "削除"; } }

        public override string Browse { get { return "照会"; } }

        public override string Options { get { return "オプション"; } }

        public override string Users { get { return "ユーザ"; } }

        public override string Groups { get { return "グループ"; } }

        public override string Local { get { return "バーチャル"; } }

        public override string Parameter { get { return "パラメータ"; } }

        public override string Expand { get { return "ズーム"; } }

        public override string UserNotExistsOrPasswordWasWrong
        {
            get
            {
                return
                    "無効なユーザIDまたはパスワード。";
            }
        }
        public override string FailedToChangePassword
        {
            get
            {
                return
                    "パスワード変更は失敗しました。";
            }
        }
        public override string NewPassword { get { return "新しいパスワード"; } }
        public override string CurrentPassword { get { return "古いパスワード"; } }
        public override string ConfirmNewPassword { get { return "パスワードの確認入力"; } }
        public override string ChangePassword { get { return "パスワード変更"; } }
        public override string WrongCurrentPassword { get { return "古いパスワードは正しくありません。"; } }
        public override string NewPasswordSameAsOldPassword { get { return "新しいパスワードとして指定された値は、現在にご利用されているパスワードと一致しています。"; } }
        public override string ConfirmNewPasswordNotSameAsNewPassword { get { return "入力されたパスワードは一致しませんでした。"; } }
        public override string Exit { get { return "終了"; } }

        public override string Name { get { return "名前"; } }

        public override string UserName { get { return "ユーザID"; } }

        public override string Password { get { return "パスワード"; } }

        public override string Login { get { return "OK"; } }

        public override string SystemLogin { get { return "ログイン"; } }

        public override string ThisRightAlreadyExists { get { return "この権限はすでに存在しています。"; } }

        public override string Details { get { return "詳細"; } }

        public override string UserGroups { get { return "ユーザグループ"; } }

        public override string Roles { get { return "役割"; } }

        public override string RoleName { get { return "役割名"; } }

        public override string Description { get { return "ディスクリプション"; } }

        public override string Choose { get { return "選択"; } }

        public override string ChooseRole { get { return "選択役割"; } }

        public override string AdditionalInfo { get { return "追加情報"; } }

        public override string IsEqualTo { get { return "Equals to"; } }

        public override string GreaterOrEqual { get { return "Greater or Equals to"; } }

        public override string Greater { get { return "Greater than"; } }

        public override string LessOrEqual { get { return "Less or Equal to"; } }

        public override string LessThen { get { return "Less then"; } }

        public override string Between { get { return "Between"; } }

        public override string Column { get { return "列"; } }

        public override string FilterType { get { return "フィルタータイプ"; } }

        public override string Values { get { return "値"; } }

        public override string FilterRows { get { return "Rng"; } }

        public override string FindARow { get { return "Loc"; } }

        public override string ConfirmUpdateMessage { get { return "保存しますか？"; } }

        public override string ConfirmUpdateTitle { get { return "更新確認"; } }

        public override string DoYouWantToUndo { get { return "変更取消確認"; } }

        public override string Undo { get { return "変更取消"; } }

        public override string ThisGroupAlreayExists
        {
            get
            {
                return
                    "該当グループはすでに存在されています。";
            }
        }

        public override string UserModuleNotLoaded
        {
            get { return "ユーザモジュールが見つかりません/ロード - {0}"; }
        }

        public override string InvalidChar
        {
            get
            {
                return
                    "数字を入力してください。　{0}";
            }
        }

        public override string InputDoesntMatchRange
        {
            get
            {
                return
                    "無効入力： {0}　入力範囲：{1}";
            }
        }

        public override string InvalidValue
        {
            get
            {
                return
                    "無効入力：{0}　正しい書式：{1}";
            }
        }

        public override string ConfirmExecution { get { return "実行確認？"; } }

        public override string ExecutionCompleted { get { return "実行完了"; } }

        public override string ErrorInStartRun { get { return "ロードエラー：{0}"; } }
        System.Text.Encoding _outerEncoding = System.Text.Encoding.GetEncoding(932);
        public override System.Text.Encoding OuterEncoding { get { return _outerEncoding; } set { _outerEncoding = value; } }
        System.Text.Encoding _innerEncoding = System.Text.Encoding.GetEncoding(932);
        public override string ControlMustBeUpdated { get { return "コントロールは更新しなければなりません。"; } }

        public override System.Text.Encoding InnerEncoding
        {
            get { return _innerEncoding; }
            set { _innerEncoding = value; }
        }

        public override string Digits { get { return "digits"; } }
        public override string UpdateNotAllowedInBrowseMode { get { return "ブラウズアクティビティに更新できません。"; } }
        public override string RowWasChanged { get { return "行は更新されました。"; } }
        public override string RowDoesNotExist { get { return "行は失われました。"; } }
        public override string ExportType { get { return "タイプ"; } }
        public override string FileName { get { return "ファイル名"; } }
        public override string Template { get { return "テンプレート"; } }
        public override string ExportData { get { return "出力データ"; } }
        public override string OpenFile { get { return "ファイルを開く"; } }
        public override string Import { get { return "入力"; } }
        public override string ErrorInExportData { get { return "データ生成時のエラー"; } }
        public override string DefaultBoolInputRange { get { return "はい,いいえ"; } }
        public override string SecuredValues { get { return "Secured Values"; } }
        public override string NameAlreadyExist { get { return "該当値はすでに存在しています。"; } }
        public override string ReadOnlyEntityUpdate { get { return "読込専用ﾃｰﾌﾞﾙは更新できません。"; } }
        public override string FilterExpression { get { return "Expression"; } }
        public override string ErrorOpeningTable { get { return "ﾃｰﾌﾞﾙを開く時のエラー："; } }
        public override string InvalidTableStructure { get { return "テーブル定義の不一致"; } }
        public override string UIControllerWithoutView { get { return "画面のないタスク"; } }
        public override string Delimiter { get { return "区切り文字"; } }
        public override string AvailableColumns { get { return "利用可能なカラム"; } }
        public override string SelectedColumns { get { return "選択されたカラム"; } }
        public override string InvalidDate { get { return "無効日付"; } }
        public override string PasswordWasChangedSuccessfully { get { return "パスワードは変更されました。"; } }
        public override string UserAlreadyExists { get { return "該当ユーザはすでに存在されています。"; } }
        public override string GroupAlreadyExists { get { return "該当グループはすでに存在しています。"; } }
        public override string UserFileIsLocked { get { return "ユーザファイルはロックされました。"; } }
        public override string UserFileIsLockedEnterReadOnly { get { return "ユーザファイルはロックされました。\r\n読込専用モードで登録しますか?"; } }
        public override string ExpandTextBox { get { return "広域"; } }
        public override string TextboxInsertMode { get { return "挿入"; } }
        public override string TextboxOverwriteMode { get { return "上書"; } }
        public override string ConfirmExitApplication { get { return "終了しますか？"; } }
        public override string ExitApplication { get { return "終了"; } }
        public override string Yes { get { return "&はい"; } }
        public override string No { get { return "&いいえ"; } }
        public override string YesHotKeys { get { return "yY"; } }
        public override string NoHotKeys { get { return "nN"; } }
        public override string NullInOnlyOnePartOfDateTimePair { get { return "日付時刻のペアの一部のみにヌル"; } }

        public override bool SupportVisualLogicalFunctions { get { return false; } }


        public override string GetErrorFor(DatabaseErrorType error)
        {
            switch (error)
            {
                case DatabaseErrorType.DataChangeFailed:
                    return "更新失敗";
                case DatabaseErrorType.DuplicateIndex:
                    return "重複インデックス";
                case DatabaseErrorType.LockedRow:
                    return "ロックされた行";
                case DatabaseErrorType.RowDoesNotExist:
                    return "[レコード紛失]";
                case DatabaseErrorType.RowWasChangedSinceLoaded:
                    return "[レコードは更新されました。]";
                case DatabaseErrorType.ReadOnlyEntityUpdate:
                    return "読込専用エンティティ更新";
                default:
                    return "未知のエラー";
            }
        }

        public override string LogonParameters
        {
            get { return "ログオンパラメータ"; }
        }
        public override string PleaseEnterIdAndPassword
        {
            get
            {
                return "ユーザIDとパスワードを入力してください。";
            }
        }
    }
}
