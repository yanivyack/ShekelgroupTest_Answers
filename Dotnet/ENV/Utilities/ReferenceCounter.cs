using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ENV.Utilities
{
    class ReferenceCounter
    {
        private ReferenceCounter()
        {
        }

        public static ReferenceCounter Instance = new ReferenceCounter();

        Dictionary<object, int> _references = new Dictionary<object, int>();

        public void AddReference(object o)
        {
            lock (_references)
            {
                if (_references.ContainsKey(o))
                    _references[o]++;
                else
                    _references.Add(o, 1);
            }
        }

        public void RemoveReference(object o, Action dispose)
        {
            bool x = false;
            lock (_references)
            {

                x = _references.ContainsKey(o) && --_references[o] == 0;
                if (x)
                {
                    
                    _references.Remove(o);
                }
            }
            if (x)
                dispose();
        }
    }
}
