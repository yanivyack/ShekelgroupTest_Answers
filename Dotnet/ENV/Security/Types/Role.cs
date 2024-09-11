using System;
using System.Collections.Generic;
using System.Text;

namespace ENV.Security.Types
{
    class Role:ENV.Data.TextColumn
    {
        public Role():base("RoleName","30")
        {
            Expand += new Action(Role_Expand);
            StorageType = ENV.Data.TextStorageType.Ansi;
        }

        void Role_Expand()
        {
            new Tasks.SelectRole().Run(this);
        }

    }
}