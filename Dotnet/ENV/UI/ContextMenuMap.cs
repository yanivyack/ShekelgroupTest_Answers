using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ENV.UI
{
    public class ContextMenuMap
    {
        Dictionary<int, Func<System.Windows.Forms.ContextMenuStrip>> _menus =
            new Dictionary<int, Func<System.Windows.Forms.ContextMenuStrip>>();
        Dictionary<int, Func<System.Windows.Forms.ContextMenuStrip>> _menuFactories =
            new Dictionary<int, Func<System.Windows.Forms.ContextMenuStrip>>();
        protected void Add(int index,Func<System.Windows.Forms.ContextMenuStrip> contextMenu)
        {
            _menuFactories.Add(index, contextMenu);
            _menus.Add(index, () =>
                                   {
                                       var result = contextMenu();
                                       _menus[index] = () => result;
                                       _dispose.Add(result);
                                       return result;
                                   });
        }

        internal void InternalAdd(int index, Func<System.Windows.Forms.ContextMenuStrip> contextMenu)
        {
            Add(index, contextMenu);
        }

        List<IDisposable> _dispose = new List<IDisposable>();
        public System.Windows.Forms.ContextMenuStrip Find(int index)
        {
            if (_menus.ContainsKey(index))
            {
                return _menus[index]();
            }
            return null;
        }
        public System.Windows.Forms.ContextMenuStrip Create(int index)
        {
            if (_menus.ContainsKey(index))
            {
                var r =  _menuFactories[index]();
                _dispose.Add(r);
                return r;
            }
            return null;
        }
        public void Dispose()
        {
            foreach (var menu in _dispose)
            {
                menu.Dispose();
            }
        }
    }
}
