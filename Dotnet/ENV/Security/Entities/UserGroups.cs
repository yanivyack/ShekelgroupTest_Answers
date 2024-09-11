using System;
using System.Collections.Generic;
using System.Text;
using ENV.Security.Types;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Security.Entities
{
    class UserGroups : ENV.Data.Entity
    {
        [PrimaryKey]
        internal ID ParentID = new ID("ParentID");
        [PrimaryKey]
        internal ID GroupID = new ID("GroupID");

        public UserGroups(IEntityDataProvider db)
            : base("UserGroups", db)
        {
        }


        
    }
}
