using System;
using System.Collections.Generic;
using System.Text;
using ENV.Security.Types;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Security.Entities
{
    class Groups : ENV.Data.Entity
    {
        [PrimaryKey]
        internal readonly ID ID = new ID();
        internal readonly ENV.Data.TextColumn Description = new ENV.Data.TextColumn("Name", "137");
        public Groups(IEntityDataProvider db)
            : base("Groups", db)
        {
        }

        internal Number CountRoles(IEntityDataProvider db)
        {
            var e = new Roles(db);
            var bp = new BusinessProcess { From = e };
            bp.Where.Add(e.ParentID.IsEqualTo(ID));
            bp.Run();
            return bp.Counter;
        }
    }
}
