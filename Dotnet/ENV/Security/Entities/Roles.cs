using System;
using System.Collections.Generic;
using System.Text;
using ENV.Security.Types;
using Firefly.Box;
using Firefly.Box.Data;
using Firefly.Box.Data.DataProvider;

namespace ENV.Security.Entities
{
    class Roles : ENV.Data.Entity
    {
        [PrimaryKey]
        internal readonly ID ParentID = new ID("parentID");
        [PrimaryKey]
        internal readonly Types.Role Role = new Types.Role();

        IEntityDataProvider _db;
        public Roles(IEntityDataProvider db)

            : base("Roles", db)
        {
            _db = db;
        }

        public static void ProvideRolesTo(string parentId, Action<string> addKey, IEntityDataProvider db)
        {
            Roles roles = new Roles(db);
            BusinessProcess bp = new BusinessProcess();
            bp.From = roles;
            bp.AddAllColumns();
            bp.Where.Add(roles.ParentID.IsEqualTo(parentId));
            bp.ForEachRow(() => addKey(roles.Role.TrimEnd()));
        }

        
    }
}
