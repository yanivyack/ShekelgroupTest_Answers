using System;
using System.Collections.Generic;
using System.Text;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;
using Firefly.Box.Data.UnderConstruction;

namespace ENV.Security.Entities
{
    class AvailableRoles : ENV.Data.Entity
    {
        [PrimaryKey]
        internal readonly Types.Role Role = new Types.Role();
        internal readonly ENV.Data.TextColumn Description = new ENV.Data.TextColumn();
        public readonly Data.BoolColumn Public = new Data.BoolColumn("public");
        static readonly DataSetDataProvider Temp = new DataSetDataProvider();
        public AvailableRoles()
            : base("AvailbleRolse", Temp)
        {
        }

        
        public static void AddRole(string key, string name,bool @public)
        {
            
            var roles = new AvailableRoles();
            var i = new Iterator(roles);
            var r = i.GetRow(roles.Role.IsEqualTo(key));
            if (r == null)
            {
                r = i.CreateRow();
                r.Set(roles.Role, key);
                r.Set(roles.Description, name);
                r.Set(roles.Public, @public);
                r.UpdateDatabase();
            }

        }
        
    }
}
