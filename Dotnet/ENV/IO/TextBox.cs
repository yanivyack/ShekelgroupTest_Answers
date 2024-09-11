using System;
using ENV.Data;
using ENV.IO.Advanced.Internal;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using Firefly.Box.UI.Advanced;
using TextControlBase = ENV.IO.Advanced.TextControlBase;
using System.Drawing;
using Firefly.Box.UI.Designer;

namespace ENV.IO
{
    [ToolboxBitmap(typeof(System.Windows.Forms.TextBox))]
    [System.ComponentModel.Design.Serialization.DesignerSerializer(typeof(ControlWithDataSerializer),
            typeof(System.ComponentModel.Design.Serialization.CodeDomSerializer))]
    public class TextBox : TextControlBase, TextControl
    {
        public TextBox()
        {

        }

     


        protected override string Translate(string term)
        {
            return base.Translate(ENV.Languages.Translate(term));
        }
        
        internal override void DoOnColumn(System.Action<ColumnBase> column)
        {
            if (Data.Column != null)
                column(Data.Column);
        }

        internal override void WriteTo(TextControlBase.Write writer)
        {
            if (Data != null)
            {

                bool isText;
                var s = Data.LastValueToString(Format, out isText);
                if (HebrewDosCompatibleEditing)
                    s = s.PadRight(new TextFormatInfo(Format).MaxLength, ' ');

                writer(s, isText);
            }

        }



        internal override void SetYourValue(string value, IStringToByteArrayConverter sba)
        {

            if (Data.Column != null)
                try
                {
                    var nc = Data.Column as NumberColumn;
                    if (nc != null)
                        nc.Value = Number.Parse(value, Firefly.Box.Text.IsNullOrEmpty(Format) ? nc.Format : Format);
                    else
                    {
                        var bac = Data.Column as ByteArrayColumn;
                        if (bac != null && bac.ContentType == ByteArrayColumnContentType.BinaryUnicode)
                        {
                            bac.Value = sba.GetBytes(value);
                        }
                        else
                        {
                            var dc = Data.Column as DateColumn;
                            if (dc != null)
                            {
                                try
                                {
                                    dc.Value = Date.Parse(value,Firefly.Box.Text.IsNullOrEmpty( Format)? dc.Format:Format, true);
                                }
                                catch {
                                    dc.Value = new Date(7908, 1, 4);
                                }
                            }else
                            Data.Column.Value = Data.Column.Parse(value, Format);
                        }
                    }
                }
                catch
                {
                    Data.Column.ResetToDefaultValue();
                }


        }

      




    }
    interface IStringToByteArrayConverter
    {
        byte[] GetBytes(string s);
    }
}
