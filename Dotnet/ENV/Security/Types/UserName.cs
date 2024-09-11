using System;
using System.Collections.Generic;
using System.Text;
using Firefly.Box;

namespace ENV.Security.Types
{
    class UserName:ENV.Data.TextColumn
    {
        public UserName():base("UserName","20")
        {
            StorageType = ENV.Data.TextStorageType.Ansi;
        }
    }
}
