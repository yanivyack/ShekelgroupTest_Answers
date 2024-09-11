using System;
using System.Collections.Generic;
using System.Text;
using Firefly.Box;

namespace ENV.Security.Types
{
    class Password:ENV.Data.TextColumn
    {
        public Password():base("Password","20")
        {
            StorageType = ENV.Data.TextStorageType.Ansi;
        }
    }
}
