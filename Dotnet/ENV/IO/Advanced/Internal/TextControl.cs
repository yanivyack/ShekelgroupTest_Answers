using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ENV.IO.Advanced.Internal
{
    interface TextControl
    {
        void WriteTo(TextControlWriter form, int doNotExceedWidth,int doNotExceedHeight);
        void ReadFrom(TextControlReader reader);
    }
}
