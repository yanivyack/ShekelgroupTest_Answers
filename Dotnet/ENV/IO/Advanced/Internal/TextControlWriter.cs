using System.Drawing;
namespace ENV.IO.Advanced.Internal
{
    public interface TextControlWriter
    {
        void Write(string text, int lineNumber, int position, int length, TextPrintingStyle style, ContentAlignment alignment);
        string ProcessColumnData(string s, bool rightToLeft, bool HebrewDosCompatibleEditing);
        bool DoNotTrimToWidth();
        bool DoNotBreakMultiline();
    }
}