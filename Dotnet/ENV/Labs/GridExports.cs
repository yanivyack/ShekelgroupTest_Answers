using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;
using ENV.UI;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;
using Firefly.Box.UI.Advanced;
using System.Linq;

namespace ENV.Labs
{
    public class GridExports
    {

        public static void ExportToExcel(Firefly.Box.UIController task)
        {
            string fileName = null;
            ExportTo(task,
                     (rightToLeft, title, columns) =>
                     {
                         fileName = GetAvailableFileName(title, ".xls");
                         return new HtmlWriter(fileName, rightToLeft, title, columns.ToArray());
                     }, 0);
            if (fileName != null)
            {
                fileName = LocalizationInfo.Current.OuterEncoding.GetString(LocalizationInfo.Current.InnerEncoding.GetBytes(fileName));
                ENV.Windows.OSCommand("excel \"" + fileName + "\"");
            }
        }


        public static string GetTestCsvData(int numOfRows)
        {
            using (var sw = new StringWriter())
            {
                ENV.UI.Grid.DoOnCurrentUIController(task => ExportTo(task, (rightToLeft, title, columns) =>
                {

                    return new TxtWriter(sw, ",", true, true,"'", columns.ToArray());
                }, numOfRows),null);
                return sw.ToString();
            }
        }


        public static void ExportToText(Firefly.Box.UIController task)
        {
            string fileName = null;
            ExportTo(task, (rightToLeft, title, columns) =>
            {
                fileName = GetAvailableFileName(title, ".txt");
                return new TxtWriter(new StreamWriter(fileName), "\t", true, false, "'", columns.ToArray());
            }, 0);
            ENV.Windows.OSCommand("notepad " + "\"" + fileName + "\"");
        }
        public static void GridToClipBoard(Firefly.Box.UIController task)
        {
            ExportTo(task, (rightToLeft, title, columns) =>
            {
                return new ClipBoardWriter(rightToLeft, title, columns.ToArray());
            }, 0);
        }

        public class ClipBoardWriter : IWriter
        {
            List<IWriter> _writers = new List<IWriter>();

            StringWriter _text = new StringWriter();
            StringWriter _comma = new StringWriter();
            StringWriter _html = new StringWriter();

            public ClipBoardWriter(System.Windows.Forms.RightToLeft rightToLeft, string title, IColumnListItem[] columns)
            {
                _writers.Add(new TxtWriter(_text, "\t", true, false, "'", columns));
                _writers.Add(new TxtWriter(_comma, ",", true, true, "'", columns));
                //HtmlWriter.SuppressMetaCharset = true;
                _writers.Add(new HtmlWriter(_html, "", rightToLeft, title, columns));
            }
            public static void ToHtmlClipboard(string html)
            {
                var dataObject = new DataObject();
                AddHtmlToDataObject(html, dataObject);

                Clipboard.SetDataObject(dataObject);
            }
            static void AddHtmlToDataObject(string html, DataObject dataObject)
            {
                var x = html;
                //x =new string(System.Text.Encoding.Default.GetChars(System.Text.Encoding.UTF8.GetBytes(x.ToCharArray())));
                var addition =
                    string.Format("Version:1.0\r\nStartHTML:00085\r\nEndHTML:{0}\r\nStartFragment:00105\r\nEndFragment:{1}\r\n<!--StartFragment-->", ((Firefly.Box.Number)x.Length + 85 + 18 + 20).ToString("5P0"),
                    ((Firefly.Box.Number)85 + 20 + x.Length).ToString("5P0"));
                x = addition + x + "<!--EndFragment-->";
             //   System.IO.File.WriteAllText(@"c:\net", x);
                dataObject.SetText(x, TextDataFormat.Html);
            }

            public void Dispose()
            {
                foreach (var writer in _writers)
                {
                    writer.Dispose();
                }
                var dataObject = new DataObject();
                dataObject.SetText(_text.ToString());

                //   dataObject.SetText(_comma.ToString(), TextDataFormat.CommaSeparatedValue);
                var x = _html.ToString();
                AddHtmlToDataObject(x, dataObject);
                Clipboard.SetDataObject(dataObject);
                //HtmlWriter.SuppressMetaCharset = false;
            }

            public void WriteLine()
            {
                foreach (var writer in _writers)
                {
                    writer.WriteLine();
                }
            }
        }
        

        public static void ExportToDataTable(Firefly.Box.UIController controller, Action<DataTableBuilder> to)
        {
            var dt = new DataTableBuilder(){ SplitDateToYearMonthDay=true, SplitHourOutOfTime=true};
            ExportToDataTableBuilder(controller, ()=>to(dt), dt);
        }
        public static void ExportToDataTableBuilder(UIController controller, Action done, DataTableBuilder dt)
        {
            ExportTo(controller,
                (left, s, arg3) =>
                {

                    foreach (var controlListItem in arg3)
                    {
                        controlListItem.AddToDataset(dt);
                    }
                },
                () => { dt.AddRow(); },
                    

                () => { done(); }, 0);

        }

        static void ExportTo(Firefly.Box.UIController task, Func<RightToLeft, string, List<IColumnListItem>, IWriter> getWriter, int numOfRows)
        {
            IWriter writer = null;
            ExportTo(task, (left, s, arg3) =>
            {
                var y = new List<IColumnListItem>();
                foreach (var controlListItem in arg3)
                {
                    y.Add(controlListItem);
                }
                writer = getWriter(left, s, y);
            }, () => writer.WriteLine(), () => writer.Dispose(), numOfRows);


        }

        static void ExportTo(Firefly.Box.UIController task, Action<RightToLeft, string, List<ControlListItem>> create, Action writeLine, Action close, int numOfRows)
        {
            try
            {
                Firefly.Box.UI.Grid grid = null;
                foreach (var control in task.View.Controls)
                {
                    if (control is Firefly.Box.UI.Grid)
                    {
                        grid = (Firefly.Box.UI.Grid)control;
                        break;
                    }
                }

                if (grid != null)
                {

                    var columns = new List<ControlListItem>();
                    foreach (var columnControl in grid.Controls)
                    {
                        var gc = columnControl as Firefly.Box.UI.GridColumn;
                        if (gc != null)
                        {
                            if (gc.Visible)
                            {
                                var inputControls = new List<ControlListItem>();
                                foreach (var inputControl in gc.Controls)
                                {
                                    var c = inputControl as InputControlBase;
                                    if (c != null)
                                    {

                                        inputControls.Add(new ControlListItem(c, gc.Text));
                                    }
                                }
                                inputControls.Reverse();
                                columns.AddRange(inputControls);
                            }
                        }
                    }
                    if (grid.RightToLeft == RightToLeft.Yes)
                    {
                        columns.Reverse();
                    }
                    string title = task.View.Text;
                    if (title != null)
                        title = title.Trim();
                    title =
                        String.IsNullOrEmpty(title) ? UserMethods.Instance.AppName().ToString() : title;

                    create(grid.RightToLeft, title, columns);
                    try
                    {
                        if (grid.MultiSelectRowsCount > 0)
                            grid.ReadSelectedRows(writeLine);
                        else
                        {
                            int i = 1;
                            ShowProgressInNewThread.ReadAllRowsWithProgress(task, title,
                                () =>
                                {
                                    if (i++ == numOfRows)
                                        throw new FlowAbortException();

                                    writeLine();
                                });
                            
                        }
                    }
                    finally
                    {
                        close();
                    }


                }
            }
            catch (Exception ex)
            {
                Common.ShowMessageBox(LocalizationInfo.Current.ErrorInExportData, MessageBoxIcon.Error, ex.Message);
            }
        }

        public static string GetAvailableFileName(string title, string fileExtension)
        {
            string fileName = title + fileExtension;
            fileName = PathDecoder. FixFileName(fileName);
            fileName = Path.Combine(Path.GetTempPath(), fileName);
            string newFileName = fileName;

            if (File.Exists(fileName))
            {
                int counter = 1;
                do
                {
                    newFileName = fileName.Insert(fileName.Length - 4, String.Format(" ({0})", counter++));
                } while (File.Exists(newFileName));
            }
            return newFileName;
        }

     

        class ControlListItem : IColumnListItem
        {

            InputControlBase _control;
            string _caption;

            public ControlListItem(InputControlBase control, string caption)
            {
                _control = control;
                _caption = caption;
            }

            public string GetValueString(bool xmlStyleDate)
            {
                if (xmlStyleDate)
                {
                    var tb = _control as Firefly.Box.UI.TextBox;
                    if (tb != null)
                    {
                        {
                            var d = tb.Data.Value as Date;
                            if (d != null)
                                return d.ToString("YYYY-MM-DD");
                        }
                        {
                            var d = tb.Data.Value as Time;
                            if (d != null)
                                return d.ToString("HH:MM:SS");
                        }
                    }
                }
                var r =  _control.Text;
                var cb = _control as ENV.UI.CheckBox;
                if (cb != null)
                {
                    if (cb.Data != null && cb.Data.Column != null)
                        r = cb.Data.Column.ToString();
                    else
                        r = cb.Checked.ToString();
                            
                }
                return r;
            }

            public ColumnBase GetColumn()
            {
                return _control.GetColumn();
            }

            public bool IsText()
            {
                return true;
            }

            public override string ToString()
            {
                return _caption;
            }

            public void AddToDataset(DataTableBuilder dt)
            {
                dt.AddColumn(_control,_caption);
                

            }
            
        }

    }
}
