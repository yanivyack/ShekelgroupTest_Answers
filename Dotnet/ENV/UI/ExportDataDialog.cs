using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ENV.Data;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using System.Text;
using Firefly.Box.Data.DataProvider;
using Firefly.Box.UI.Advanced;
using System.Linq;
using System.Web;
using ENV.Utilities;

namespace ENV.UI
{
    public partial class ExportDataDialog : System.Windows.Forms.Form
    {
        AbstractUIController _task;
        string userPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\";
        string fileName = PathDecoder.FixFileName(UserMethods.Instance.Sys()) + ".UserData";
        public static string templateFileName = "";
        const string OTHER = "\0";

        public ExportDataDialog()
        {
            InitializeComponent();
            typeComboBox.Items.Add(new ExportTypeListItem("Text", (writer, columns) => new TxtWriter(writer,
                OtherSelected(delimiterComboBox) ? txtOtherDelimeter.Text : ((DelimiterListItem)delimiterComboBox.SelectedItem).DelimiterString,
                xsdOrHeaderCheckbox.Checked, true,
                OtherSelected(cboStringIdentifier) ? txtOtherStringIdentifier.Text : ((DelimiterListItem)cboStringIdentifier.SelectedItem).DelimiterString,
                columns)));
            typeComboBox.Items.Add(new ExportTypeListItem("Html", (writer, columns) => new HtmlWriter(writer, templateTextBox.Text, RightToLeft, "", columns)));
            typeComboBox.Items.Add(new ExportTypeListItem("Xml", (writer, columns) => new XmlWriter(xsdOrHeaderCheckbox.Checked, writer, columns)));
            typeComboBox.SelectedIndex = 0;

            delimiterComboBox.Items.Add(new DelimiterListItem("None", ""));
            delimiterComboBox.Items.Add(new DelimiterListItem("Comma", ","));
            delimiterComboBox.Items.Add(new DelimiterListItem("Colon", ":"));
            delimiterComboBox.Items.Add(new DelimiterListItem("Tab", "\t"));
            delimiterComboBox.Items.Add(new DelimiterListItem("Other", OTHER));
            delimiterComboBox.SelectedIndex = 1;
            cboStringIdentifier.Items.AddRange(new[] {
                new DelimiterListItem("None",""),
                new DelimiterListItem("Single Quotation","'"),
                new DelimiterListItem("Double Quatation","\""),
                new DelimiterListItem("Other",OTHER)
            });
            cboStringIdentifier.SelectedIndex = 1;

            this.okButton.Text = LocalizationInfo.Current.Ok;
            this.cancelButton.Text = LocalizationInfo.Current.Cancel;
            this.typeLabel.Text = LocalizationInfo.Current.ExportType;
            this.delimiterLabel.Text = LocalizationInfo.Current.Delimiter;
            this.fileNameLabel.Text = LocalizationInfo.Current.FileName;
            this.templateLabel.Text = LocalizationInfo.Current.Template;
            this.columnsLabel.Text = LocalizationInfo.Current.AvailableColumns;
            this.selectedColumnsLabel.Text = LocalizationInfo.Current.SelectedColumns;
            this.templateTextBox.Text = templateFileName;
            this.openFileCheckBox.Text = LocalizationInfo.Current.OpenFile;
            this.RightToLeft = LocalizationInfo.Current.RightToLeft;
            RightToLeftLayout = true;
            Text = LocalizationInfo.Current.ExportData;
            LoadUserData();
            SetControlsEnabled();
        }

        public ExportDataDialog(AbstractUIController task)
            : this()
        {
            _task = task;

            Action<Control, bool> addColumnByControl =
                (control, parentTasksOnly) =>
                {
                    var icb = control as InputControlBase;

                    if (icb == null || (UserSettings.Version10Compatible && !icb.Available) || icb is Firefly.Box.UI.Button) return;

                    var c = icb.GetColumn();

                    if (c == null && icb.Tag == null) return;

                    if (parentTasksOnly && _task.Columns.Contains(c)) return;

                    selectedColumnsListBox.Items.Add(new ColumnListItem(c, icb));
                };

            if (UserSettings.Version10Compatible)
            {
                _task._provideColumnsForFilter(
                    (c, icb) =>
                    {
                        if (icb != null && icb.Available && (c != null || icb.Tag != null))
                            selectedColumnsListBox.Items.Add(new ColumnListItem(c, icb));
                    });
                if (task is FlowUIControllerBase)
                    _task._uiController.View.ForEachControlInTabOrder(control => addColumnByControl(control, true));
            }
            else
            {
                var l = new List<Control>();

                _task._uiController.View.ForEachControlInZOrder(
                    c =>
                    {
                        l.Add(c);
                        var grid = c as Firefly.Box.UI.Grid;
                        if (grid != null)
                            grid.ForEachControlInZOrder(c1 => l.Add(c1));
                    });
                var o = new List<Control>(l);
                l.Sort((x, y) => (x is ControlBase && y is ControlBase && ((ControlBase)x).ZOrder != 0 && ((ControlBase)y).ZOrder != 0 ? ((ControlBase)x).ZOrder.CompareTo(((ControlBase)y).ZOrder) : o.IndexOf(x).CompareTo(o.IndexOf(y))));
                foreach (var item in l)
                    addColumnByControl(item, false);
            }

            if (selectedColumnsListBox.Items.Count > 0)
            {
                selectedColumnsListBox.SelectedIndex = 0;
            }
        }

        public void ShowDialog(System.Windows.Forms.Form mdi)
        {
            SetButtonsEnabeld();
            Firefly.Box.Context.Current.InvokeUICommand(() =>
            {
                var result = base.ShowDialog(mdi);
                if (result == DialogResult.OK && openFileCheckBox.Checked)
                    ENV.Windows.OSCommand("\"" + fileNameTextBox.Text + "\"", false, System.Diagnostics.ProcessWindowStyle.Normal);
            });
        }

        void SetButtonsEnabeld()
        {
            upButton.Enabled = selectedColumnsListBox.SelectedIndex > 0;
            downButton.Enabled = selectedColumnsListBox.SelectedIndex < selectedColumnsListBox.Items.Count - 1 && selectedColumnsListBox.SelectedIndex > -1;
            addButton.Enabled = columnsListBox.SelectedItems.Count > 0;
            addAllButton.Enabled = columnsListBox.Items.Count > 0;
            removeButton.Enabled = selectedColumnsListBox.SelectedItems.Count > 0;
            removeAllButton.Enabled = selectedColumnsListBox.Items.Count > 0;
            okButton.Enabled = selectedColumnsListBox.Items.Count > 0 && fileNameTextBox.Text.Trim() != string.Empty;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var openFileDialog = new SaveFileDialog();

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileNameTextBox.Text = openFileDialog.FileName;
            }
        }

        class ExportTypeListItem
        {
            string _name;
            Func<StreamWriter, IColumnListItem[], IWriter> _create;

            public ExportTypeListItem(string name, Func<StreamWriter, IColumnListItem[], IWriter> create)
            {
                _name = name;
                _create = create;
            }

            public IWriter CreateWriter(StreamWriter writer, IColumnListItem[] columns)
            {
                return _create(writer, columns);
            }
            public override string ToString()
            {
                return _name;
            }
        }
        class DelimiterListItem
        {
            string _name;
            string _delimiterString;

            public DelimiterListItem(string name, string delimiterString)
            {
                _name = name;
                _delimiterString = delimiterString;
            }

            public string DelimiterString
            {
                get
                {
                    return _delimiterString;
                }
            }

            public override string ToString()
            {
                return _name;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Close();
            try
            {
                using (var fileWriter = new StreamWriter(fileNameTextBox.Text, false, LocalizationInfo.Current.OuterEncoding))
                {
                    var selectedColumns = new List<IColumnListItem>();
                    foreach (var item in selectedColumnsListBox.Items)
                    {
                        selectedColumns.Add((IColumnListItem)item);
                    }

                    using (var writer = ((ExportTypeListItem)typeComboBox.SelectedItem).CreateWriter(fileWriter, selectedColumns.ToArray()))
                    {
                        try
                        {
                            ENV.Data.DataProvider.DynamicSQLSupportingDataProvider.DontRunSql = true;
                            _task._uiController.ReadAllRows(writer.WriteLine);
                        }
                        finally {
                            ENV.Data.DataProvider.DynamicSQLSupportingDataProvider.DontRunSql = false;
                        }
                    }
                }
                SaveUserData();
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToLogFile(ex, "FileName: {0}", fileNameTextBox.Text);
                Common.ShowMessageBox(LocalizationInfo.Current.ErrorInExportData, MessageBoxIcon.Error, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        void SaveUserData()
        {
            try
            {
                using (var writer = new StreamWriter(userPath + fileName, false, LocalizationInfo.Current.OuterEncoding))
                {
                    writer.WriteLine(this.fileNameTextBox.Text);
                    writer.WriteLine(this.templateTextBox.Text);
                    writer.WriteLine(this.typeComboBox.SelectedIndex.ToString());
                    writer.WriteLine(this.delimiterComboBox.SelectedIndex.ToString());
                    writer.WriteLine(this.xsdOrHeaderCheckbox.Checked.ToString());
                    writer.WriteLine(cboStringIdentifier.SelectedIndex.ToString());
                    writer.WriteLine(txtOtherDelimeter.Text);
                    writer.WriteLine(txtOtherStringIdentifier.Text);
                }
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToLogFile(ex, "UserPath: {0}, FileName: {1}, Merged: {2}", userPath, fileName, userPath + fileName);
                Common.ShowMessageBox(LocalizationInfo.Current.ErrorInExportData, MessageBoxIcon.Error, ex.Message);
            }
        }
        void LoadUserData()
        {
            try
            {
                if (File.Exists(userPath + fileName))
                {
                    using (var reader = new StreamReader(userPath + fileName, LocalizationInfo.Current.OuterEncoding))
                    {
                        this.fileNameTextBox.Text = reader.ReadLine();
                        this.templateTextBox.Text = reader.ReadLine();
                        var type = reader.ReadLine();
                        if (!String.IsNullOrEmpty(type))
                            this.typeComboBox.SelectedIndex = int.Parse(type);
                        var delimiter = reader.ReadLine();
                        if (!String.IsNullOrEmpty(delimiter))
                            this.delimiterComboBox.SelectedIndex = int.Parse(delimiter);
                        var createXsd = reader.ReadLine();
                        if (!String.IsNullOrEmpty(createXsd))
                            this.xsdOrHeaderCheckbox.Checked = createXsd == "True";
                        string s = reader.ReadLine();
                        if (!string.IsNullOrEmpty(s))
                            cboStringIdentifier.SelectedIndex = int.Parse(s);
                        s = reader.ReadLine();
                        if (!string.IsNullOrEmpty(s))
                            txtOtherDelimeter.Text = s;
                        s = reader.ReadLine();
                        if (!string.IsNullOrEmpty(s))
                            txtOtherStringIdentifier.Text = s;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToLogFile(ex, "UserPath: {0}, FileName: {1}, Merged: {2}", userPath, fileName, userPath + fileName);
                Common.ShowMessageBox(LocalizationInfo.Current.ErrorInExportData, MessageBoxIcon.Error, ex.Message);
            }
        }

        private void upButton_Click(object sender, EventArgs e)
        {
            ChangeSelectColumnOrderInOutputList(-1);
        }
        void ChangeSelectColumnOrderInOutputList(int sortIdent)
        {
            int index = selectedColumnsListBox.SelectedIndex;
            var item = (IColumnListItem)selectedColumnsListBox.SelectedItem;
            selectedColumnsListBox.Items.Remove(item);
            selectedColumnsListBox.Items.Insert(index + sortIdent, item);
            selectedColumnsListBox.SelectedIndex = index + sortIdent;
            SetButtonsEnabeld();
        }

        private void downButton_Click(object sender, EventArgs e)
        {
            ChangeSelectColumnOrderInOutputList(1);
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            AddItem();
        }
        private void addAllButton_Click(object sender, EventArgs e)
        {
            while (columnsListBox.Items.Count > 0)
            {
                AddItem();
            }
        }
        void AddItem()
        {
            MoveItemsBettwenLists(columnsListBox, selectedColumnsListBox);
        }
        void RemoveItem()
        {
            MoveItemsBettwenLists(selectedColumnsListBox, columnsListBox);
        }
        void MoveItemsBettwenLists(System.Windows.Forms.ListBox source, System.Windows.Forms.ListBox destinition)
        {
            var list = new List<IColumnListItem>();
            foreach (var item in source.SelectedItems)
            {
                list.Add((IColumnListItem)item);
            }
            foreach (var listItem in list)
            {
                source.Items.Remove(listItem);
                destinition.Items.Add(listItem);
            }
            SetButtonsEnabeld();
            if (source.Items.Count > 0)
            {
                source.SelectedIndex = 0;
            }
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            RemoveItem();
        }
        private void removeAllButton_Click(object sender, EventArgs e)
        {
            while (selectedColumnsListBox.Items.Count > 0)
            {
                RemoveItem();
            }
        }
        private void columnsListBox_DoubleClick(object sender, EventArgs e)
        {
            AddItem();
        }

        private void selectedColumnsListBox_DoubleClick(object sender, EventArgs e)
        {
            RemoveItem();
        }

        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetButtonsEnabeld();
        }

        private void fileNameTextBox_TextChanged(object sender, EventArgs e)
        {
            SetButtonsEnabeld();
        }

        private void ExportDataDialog_Load(object sender, EventArgs e)
        {
            SetButtonsEnabeld();
        }

        private void templateBrowseButton_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                templateTextBox.Text = openFileDialog.FileName;
            }
        }

        private void typeComboBox_TextChanged(object sender, EventArgs e)
        {
            SetControlsEnabled();
        }

        void SetControlsEnabled()
        {
            var templateEnabled = typeComboBox.Text == "Html";
            this.templateBrowseButton.Enabled = templateEnabled;
            this.templateTextBox.Enabled = templateEnabled;
            var text = typeComboBox.Text == "Text";
            this.delimiterComboBox.Enabled = text;
            this.cboStringIdentifier.Enabled = text;
            this.txtOtherDelimeter.Enabled = text && OtherSelected(delimiterComboBox);
            this.txtOtherStringIdentifier.Enabled = text && OtherSelected(cboStringIdentifier);
        }
        bool OtherSelected(System.Windows.Forms.ComboBox c)
        {
            var y = c.SelectedItem as DelimiterListItem;
            if (y != null)
                return y.DelimiterString == OTHER;
            return false;
        }

        private void typeComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            switch (typeComboBox.Text)
            {
                case "Xml":
                    xsdOrHeaderCheckbox.Visible = true;
                    xsdOrHeaderCheckbox.Checked = false;
                    xsdOrHeaderCheckbox.Text = "Create Xsd";
                    break;
                case "Text":
                    xsdOrHeaderCheckbox.Visible = true;
                    xsdOrHeaderCheckbox.Checked = true;
                    xsdOrHeaderCheckbox.Text = "Header Line";
                    break;
                default:
                    xsdOrHeaderCheckbox.Visible = false;
                    break;
            }

        }
    }

    public interface IColumnListItem
    {
        string GetValueString(bool xmlStyleDate);
        ColumnBase GetColumn();
        bool IsText();
        string ToString();
    }

    public class ColumnListItem : IColumnListItem
    {
        ColumnBase _column;
        InputControlBase _control;

        public ColumnListItem(ColumnBase column, InputControlBase control)
        {
            _column = column;
            _control = control;
        }


        public override string ToString()
        {
            if (_control != null)
                if (_control.Tag != null && _control.Tag.ToString().Trim() != "" && !UserSettings.Version10Compatible)
                    return _control.Tag.ToString();
            if (_column != null && _column.Caption != null)
                return _column.Caption;
            return "";
        }

        public string GetValueString(bool xmlStyleDate)
        {
            var cb = _control as Firefly.Box.UI.CheckBox;
            if (cb != null)
            {
                var va = cb.Data.Value as Bool;
                if (va == null)
                    return "";
                return va ? "1" : "0";
            }
            if (xmlStyleDate)
            {
                var tb = _control as Firefly.Box.UI.TextBox;
                if (tb != null)
                {
                    {
                        var v = tb.Data.Value as Date;
                        if (v != null)
                            return v.ToString("YYYY-MM-DD");
                    }
                    {
                        var v = tb.Data.Value as Time;
                        if (v != null)
                            return v.ToString("HH:MM:SS");
                    }

                }
            }
            if (_control != null)
                return _control.Text;

            if (_column.Value == null)
                return "";
            if (xmlStyleDate)
            {
                {
                    var c = _column as DateColumn;
                    if (c != null)
                        return c.ToString("YYYY-MM-DD");
                }
                {
                    var c = _column as TimeColumn;
                    if (c != null)
                        return c.ToString("HH:MM:SS");
                }
            }
            return _column.ToString();
        }

        public ColumnBase GetColumn()
        {
            return _column;
        }

        public bool IsText()
        {
            return _column is Firefly.Box.Data.TextColumn;
        }
    }

    interface IWriter : IDisposable
    {
        void WriteLine();
    }

    public class TxtWriter : IWriter
    {
        TextWriter _textWriter;
        IColumnListItem[] _columns;
        string _delimiter = ",";
        string _stringIdentifier;
        bool _useQuotes;
        public TxtWriter(TextWriter textWriter, string delimiter, bool withHeaders, bool useQuotes, string stringIdentifier, params IColumnListItem[] columns)
        {
            _stringIdentifier = stringIdentifier;

            _useQuotes = useQuotes;
            if (string.IsNullOrEmpty(_stringIdentifier))
                _useQuotes = false;
            _textWriter = textWriter;
            _columns = columns;
            _delimiter = delimiter;

            if (withHeaders)
            {
                bool isFirst = true;
                foreach (var column in columns)
                {
                    if (!isFirst)
                    {
                        _textWriter.Write(_delimiter);
                    }
                    isFirst = false;

                    string value = column.ToString();
                    if (_useQuotes)
                        value = _stringIdentifier + value.Replace(_stringIdentifier, _stringIdentifier + _stringIdentifier) + _stringIdentifier;
                    _textWriter.Write(value);
                }
                _textWriter.WriteLine();
            }
        }

        public void Dispose()
        {
            _textWriter.Dispose();
        }

        public void WriteLine()
        {
            bool isFirst = true;
            foreach (var column in _columns)
            {
                if (!isFirst)
                {
                    _textWriter.Write(_delimiter);
                }
                isFirst = false;

                string value = column.GetValueString(false).TrimEnd(' ');

                if (column.IsText() || value.Contains(_delimiter))
                {
                    if (_useQuotes)
                        value = _stringIdentifier + value.Replace(_stringIdentifier, _stringIdentifier + _stringIdentifier) + _stringIdentifier;
                }

                _textWriter.Write(value);
            }
            _textWriter.WriteLine();
        }
    }

    public class HtmlWriter : IWriter
    {
        string _template;
        int startIndex, endIndex;
        TextWriter _textWriter;
        StringWriter _stringWriter = new StringWriter();
        IColumnListItem[] _columns;
        int _lineNum;
        RightToLeft _rightToLeft;
        public static bool SuppressMetaCharset { get; set; }

        public HtmlWriter(TextWriter textWriter, string templatePath, RightToLeft rightToLeft, string title, params IColumnListItem[] columns)
        {
            _textWriter = textWriter;
            _columns = columns;
            _rightToLeft = rightToLeft;
            if (!Text.IsNullOrEmpty(templatePath))
            {
                templatePath = PathDecoder.DecodePath(templatePath);
                if (File.Exists(templatePath))
                {
                    _template = File.ReadAllText(templatePath,LocalizationInfo.Current.OuterEncoding);
                }
            }
            if (_template == null)
                InitilizeTemplate();
            startIndex = _template.IndexOf("<MGTABLE", StringComparison.InvariantCultureIgnoreCase);
            endIndex = _template.IndexOf('>', startIndex + 1);
            _template = _template.Remove(startIndex, endIndex - startIndex + 1);

            _stringWriter.WriteLine("<table border=\"1\" cellspacing=0>");
            if (!Text.IsNullOrEmpty(title))
                _stringWriter.WriteLine("<caption><b>{0}</b></caption>", title);
            _stringWriter.WriteLine("<THEAD>");
            _stringWriter.WriteLine(" <tr>");

            foreach (var column in _columns)
            {
                _stringWriter.WriteLine("<td>{0}</td>", HttpUtility.HtmlEncode(column.ToString()));
            }
            _stringWriter.WriteLine(" </tr>");
            _stringWriter.WriteLine("</THEAD>");
            _stringWriter.WriteLine("<TBODY>");
        }

        public HtmlWriter(string fileName, RightToLeft rightToLeft, string title, params IColumnListItem[] columns)
            : this(new StreamWriter(fileName, false, LocalizationInfo.Current.OuterEncoding), "", rightToLeft, title, columns)
        {
        }

        void InitilizeTemplate()
        {
            string metaData = "";
            if (!SuppressMetaCharset)
                metaData = String.Format("<META content=\"text/html; charset={0}\" http-equiv=content-type>\r\n",
                                         LocalizationInfo.Current.OuterEncoding.HeaderName.Trim());

            _template = String.Format("<HTML{0}>\r\n", GetHtmlAttributes()) +
                        "<HEAD>\r\n" +
                        metaData +
                        "</HEAD>\r\n" +
                        "<BODY>\r\n" +
                        "<MGTABLE>\r\n" +
                        "</BODY>\r\n" +
                        "</HTML>";
        }

        string GetHtmlAttributes()
        {
            if (_rightToLeft == RightToLeft.Yes)
                return " dir=\"rtl\"";
            return "";
        }

        public void WriteLine()
        {
            _lineNum++;
            _stringWriter.WriteLine("<tr class=\"MG_{0}_Row\"" + ">", _lineNum % 2 == 0 ? "Even" : "Odd");
            foreach (var column in _columns)
            {
                _stringWriter.WriteLine("  <td>{0}  </td>", column.GetValueString(false).Trim());
            }
            _stringWriter.WriteLine("</tr>");
        }

        public void Dispose()
        {
            _stringWriter.WriteLine("</TBODY>");
            _stringWriter.WriteLine("</table>");

            _template = _template.Insert(startIndex, _stringWriter.ToString());
            _textWriter.Write(_template);
            _stringWriter.Dispose();
            _textWriter.Dispose();
        }
    }
    class DataTableWriter : IWriter
    {
        IColumnListItem[] _columns;
        public DataTableBuilder DataTableBuilder = new DataTableBuilder() { BackwardCompatibleTypes = true };
        public static bool SuppressMetaCharset { get; set; }

        public DataTableWriter( params IColumnListItem[] columns)
        {
            _columns = columns;
            foreach (var item in columns)
            {
                DataTableBuilder.AddColumn(item.GetColumn(), item.ToString());
            }
        }







        public void WriteLine()
        {
            DataTableBuilder.AddRow();
        }

        public void Dispose()
        {

        }
    }

    public class XmlWriter : IWriter
    {
        string _template;
        int startIndex;
        string _PrintData = "Print_data";
        StreamWriter _textWriter;
        StringWriter _stringWriter = new StringWriter();
        readonly IColumnListItem[] _columns;
        bool _createXsd;
        string _xsdFileName;
        public static bool SuppressMetaCharset { get; set; }

        public XmlWriter(StreamWriter writer, string xsdFileName, params IColumnListItem[] columns)
            : this(!string.IsNullOrWhiteSpace(xsdFileName), writer, columns)
        {
            _xsdFileName = xsdFileName;
        }

        public XmlWriter(bool createXsd, StreamWriter textWriter, params IColumnListItem[] columns)
        {
            _createXsd = createXsd;
            _textWriter = textWriter;
            _columns = columns;
            if (_createXsd && string.IsNullOrWhiteSpace(_xsdFileName))
                _xsdFileName = Path.ChangeExtension(((FileStream)_textWriter.BaseStream).Name, ".xsd");
            InitilizeTemplate();

        }
        string EncodingName
        {
            get
            {
                if (_textWriter.Encoding== Encoding.UTF8)
                    return _textWriter.Encoding.HeaderName.Trim().ToUpper();
                return _textWriter.Encoding.HeaderName.Trim();
            }
        }

        void InitilizeTemplate()
        {
            string metaData = "";

            var openPrintData = _PrintData;
            if (!SuppressMetaCharset)
                metaData = String.Format("<?xml version=\"1.0\" encoding=\"{0}\"?>\r\n", EncodingName);

            if (_createXsd)
            {
                openPrintData = openPrintData + " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.w3.org/2001/XMLSchema";
                openPrintData += " " + _xsdFileName;
                openPrintData += "\" ";
            }

            _template = metaData +
                        "<" + openPrintData + ">\r\n";
            startIndex = _template.Length;
            _template += "</" + _PrintData + ">";
        }

        public void WriteLine()
        {
            _stringWriter.Write("<Record>");
            string _tagName;
            string _value;
            foreach (var column in _columns)
            {
                _tagName = MakeNameXmlComplient(column);
                _value = UserMethods.Instance.XMLVal(column.GetValueString(true).Trim());
                _stringWriter.Write("<{0}>{1}</{0}>", _tagName, _value);
            }
            _stringWriter.WriteLine("</Record>");
        }

        private static string MakeNameXmlComplient(IColumnListItem column)
        {
            return new System.Text.RegularExpressions.Regex("\\W").Replace(column.ToString().Trim(), "_");
        }

        public void Dispose()
        {
            _template = _template.Insert(startIndex, _stringWriter.ToString());
            _textWriter.Write(_template);
            if (_createXsd)
                WriteXSD();
            _stringWriter.Dispose();
            _textWriter.Dispose();
        }

        void WriteXSD()
        {
            if (string.IsNullOrWhiteSpace(_xsdFileName))
                _xsdFileName = Path.ChangeExtension(((FileStream)_textWriter.BaseStream).Name, ".xsd");
            using (var xsdWriter = new StreamWriter(_xsdFileName, false, _textWriter.Encoding))
            {
                xsdWriter.WriteLine("<?xml version=\"1.0\" encoding=\"{0}\"?>\r\n", EncodingName);
                xsdWriter.WriteLine("<!--W3C Schema auto-generated-->");
                xsdWriter.WriteLine("<xs:schema targetNamespace=\"urn:Firefly.ExportData\" xmlns=\"urn:Firefly.ExportData\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">");
                foreach (var columnListItem in _columns)
                {
                    if (!(columnListItem.GetColumn() is ByteArrayColumn))
                    {
                        xsdWriter.WriteLine("<xs:element name=\"{0}\">", MakeNameXmlComplient( columnListItem));
                        xsdWriter.WriteLine("   <xs:simpleType >");
                        UserMethods.CastColumn(columnListItem.GetColumn(), new Caster(xsdWriter));
                        xsdWriter.WriteLine("      </xs:restriction>");
                        xsdWriter.WriteLine("   </xs:simpleType>");
                        xsdWriter.WriteLine("</xs:element>");
                    }
                }
                xsdWriter.WriteLine("<xs:element name=\"Print_data\">");
                xsdWriter.WriteLine("   <xs:complexType>");
                xsdWriter.WriteLine("      <xs:sequence>");
                xsdWriter.WriteLine("         <xs:element ref=\"Record\" maxOccurs=\"unbounded\"/>");
                xsdWriter.WriteLine("      </xs:sequence>");
                xsdWriter.WriteLine("   </xs:complexType>");
                xsdWriter.WriteLine("</xs:element>");
                xsdWriter.WriteLine("<xs:element name=\"Record\">");
                xsdWriter.WriteLine("  <xs:complexType >");
                xsdWriter.WriteLine("     <xs:sequence>");
                foreach (var columnListItem in _columns)
                {
                    if (!(columnListItem.GetColumn() is ByteArrayColumn))
                    {
                        xsdWriter.WriteLine("        <xs:element ref=\"{0}\"/>", MakeNameXmlComplient( columnListItem));
                    }
                }
                xsdWriter.WriteLine("     </xs:sequence>");
                xsdWriter.WriteLine("  </xs:complexType>");
                xsdWriter.WriteLine("</xs:element>");
                xsdWriter.WriteLine("</xs:schema>");
            }
        }

        class Caster : UserMethods.IColumnSpecifier
        {
            TextWriter _xsdWriter;

            public Caster(TextWriter xsdWriter)
            {
                _xsdWriter = xsdWriter;
            }


            public void DoOnColumn(TypedColumnBase<Text> column)
            {
                _xsdWriter.WriteLine("      <xs:restriction base=\"xs:{0}\">", "string");
                _xsdWriter.WriteLine("         <xs:maxLength value=\"{0}\"/>", column.FormatInfo.MaxLength);
            }

            public void DoOnColumn(TypedColumnBase<Number> column)
            {
                var formatInfo = column.FormatInfo as NumberFormatInfo;
                if (formatInfo != null)
                {
                    if (formatInfo.DecimalDigits > 0)
                    {
                        _xsdWriter.WriteLine("      <xs:restriction base=\"xs:{0}\">", "decimal");
                        _xsdWriter.WriteLine("\t\t\t<xs:totalDigits value=\"{0}\"/>", formatInfo.TotalDigits);
                        _xsdWriter.WriteLine("\t\t\t<xs:fractionDigits value=\"{0}\"/>", formatInfo.DecimalDigits);
                    }
                    else
                    {
                        _xsdWriter.WriteLine("      <xs:restriction base=\"xs:{0}\">", "integer");
                        _xsdWriter.WriteLine("\t\t\t<xs:totalDigits value=\"{0}\"/>", formatInfo.TotalDigits);
                    }
                }
            }

            public void DoOnColumn(TypedColumnBase<Date> column)
            {
                _xsdWriter.WriteLine("      <xs:restriction base=\"xs:{0}\">", "date");
            }

            public void DoOnColumn(TypedColumnBase<Time> column)
            {
                _xsdWriter.WriteLine("\t\t<xs:restriction base=\"xs:{0}\">", "time");
            }

            public void DoOnColumn(TypedColumnBase<Bool> column)
            {
                _xsdWriter.WriteLine("      <xs:restriction base=\"xs:{0}\">", "boolean");
            }

            public void DoOnColumn(TypedColumnBase<byte[]> column)
            {
            }

            public void DoOnUnknownColumn(ColumnBase column)
            {
            }
        }
    }
}
