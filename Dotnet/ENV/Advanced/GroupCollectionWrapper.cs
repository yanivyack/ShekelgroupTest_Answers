using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;

namespace ENV.Advanced
{
    public class GroupCollectionWrapper
    {
        GroupCollection _source;
        LevelProvider _levelProvider;
        List<WrappedGroup> _groups = new List<WrappedGroup>();
        internal GroupCollectionWrapper(GroupCollection groups, LevelProvider levelProvider)
        {
            _source = groups;
            _levelProvider = levelProvider;
        }

        public int Count
        {
            get { return _groups.Count; }
        }

        public WrappedGroup Add(ColumnBase monitoredColumn)
        {
            var res =  new WrappedGroup(_source.Add(monitoredColumn),this);
            _groups.Add(res);
            return res;
        }

        public WrappedGroup this[ColumnBase column]
        {
            get
            {
                foreach (var g in _groups)
                {
                    if (g.Column == column)
                        return g;
                };
                return Add(column);
            }
        }


        public class WrappedGroup
        {
            Group _source;


            internal WrappedGroup(Group source, GroupCollectionWrapper parent)
            {
                _source = source;

                _source.Enter += () =>
                {
                    if (Enter != null)
                    {
                        var level = "GP_" + source.Column.Caption;
                        using (parent._levelProvider.StartContext(level,
                            ENV.UserSettings.Version8Compatible?"L"+(parent._groups.Count- parent._groups.IndexOf(this)+1 )+"P":
                                "GP_" + (parent._groups.IndexOf(this) + 1),level))
                        {
                            Enter();
                        }
                    }
                };
                _source.Leave += () =>
                {
                    if (Leave != null)
                    {
                        var level = "GS_" + source.Column.Caption;
                        using (parent._levelProvider.StartContext(level,
                            ENV.UserSettings.Version8Compatible ? "L" + (parent._groups.Count - parent._groups.IndexOf(this)+1) + "S" : 
                                "GS_" + (parent._groups.IndexOf(this) + 1),level))
                        {
                            Leave();
                        }
                    }
                };
            }


            public ColumnBase Column
            {
                get { return _source.Column; }
            }

            internal Group Group
            {
                get { return _source; }
               
            }

            public event Action Enter;


            public event Action Leave;

        }
    }
}
