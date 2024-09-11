using System;
using System.Collections.Generic;
using System.Text;
using ENV.Security.Types;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Security.Entities
{
    class Users :ENV.Data.Entity
    {
        [PrimaryKey]
        internal readonly Types.UserId ID = new Types.UserId();
        internal readonly UserName UserName = new UserName();
        internal readonly ENV.Data.TextColumn Description = new ENV.Data.TextColumn("Description", "1000");
        internal readonly Password Password = new Password();
        internal readonly ENV.Data.TextColumn AdditionalInfo = new ENV.Data.TextColumn("AdditionalInfo", "1000", "Additional Info");
        public Users(IEntityDataProvider db)
            : base("Users", db)
        {
        }

        internal User GetUser()
        {
            return new User(ID, UserName.TrimEnd(), Password.TrimEnd(), Description.TrimEnd(), AdditionalInfo.TrimEnd());
        }


        public Number CountRoles(IEntityDataProvider db)
        {
            
            var e = new Roles(db);
            var bp = new BusinessProcess {From = e};
            bp.Where.Add(e.ParentID.IsEqualTo(ID));
            bp.Run();
            return bp.Counter;


        }

        public Number CountGroups(IEntityDataProvider db)
        {
            var e = new Entities.UserGroups(db);
            var bp = new BusinessProcess { From = e };
            bp.Where.Add(e.ParentID.IsEqualTo(ID));
            bp.Run();
            return bp.Counter;
        }
    }
}
