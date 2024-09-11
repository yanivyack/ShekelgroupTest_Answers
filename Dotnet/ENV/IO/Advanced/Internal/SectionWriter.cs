using System;

namespace ENV.IO.Advanced.Internal
{
    public interface SectionWriter
    {
        void WriteSection(int width, int height, TextPrintingStyle style, Action<TextControlWriter> writeCommand, Action newPageStatedDueToLackOfSpace);
    }
}