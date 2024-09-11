using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ENV.IO.Advanced.Internal
{
    interface ReaderInterface
    {
        void ReadSection(int width, int height, System.Action<TextControlReader> readCommand);
        void ReadSection(int width, char separator, Action<ValueProviderDelegate> sendResultTo);
        void ReadSectionDoubleSeparator(int width, char separator, Action<ValueProviderDelegate> sendResultTo);
    }
    public interface ITextSectionReader
    {
        void Read(TextSection ts);
    }
}
