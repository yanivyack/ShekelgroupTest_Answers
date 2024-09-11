using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENV.Data;
using ENV.Data.DataProvider;
using Firefly.Box.Data.DataProvider;

namespace ENV.Security.Entities
{
    public class SecuredValues : ENV.Data.Entity
    {
        [PrimaryKey]
        internal readonly TextColumn Name = new TextColumn("Name", "50");
        internal readonly TextColumn Value = new TextColumn("Value", "500");

        public SecuredValues(IEntityDataProvider db)
            : base("SecuredValues", db)
        {
        }

        static PathDecoder Decoder = new PathDecoder(() => PathDecoder.ContextPathDecoder);
        static bool _everLoaded = false;
        internal static void Load(IEntityDataProvider db)
        {
            _everLoaded = true;
            Decoder = new PathDecoder(() => PathDecoder.ContextPathDecoder);
            var e = new SecuredValues(db);
            var bp = new Firefly.Box.BusinessProcess { From = e };
            bp.ForEachRow(() => Decoder.AddTokenName(e.Name.Trim(), e.Value.Trim()));
        }

        public static string Decode(string value)
        {
            if (!_everLoaded)
                lock("SecuredValues")
                {
                    if(!_everLoaded)
                        UserManager.ChangeUserFile(db => false);
                }
            return Decoder.InternalDecodePath(value,false);
        }


        internal static void RegisterObserver(ChangeMonitor myChangeMonitor)
        {
            Decoder.RegisterObserver(myChangeMonitor);
        }
    }
}
