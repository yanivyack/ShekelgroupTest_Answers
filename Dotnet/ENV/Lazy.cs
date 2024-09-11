using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ENV
{
    public class Lazy<T> where T : BusinessProcessBase
    {
        Func<T> _factory;

        public Lazy(Func<T> factory)
        {
            _factory = factory;
        }

        T _value;
        public T Instance
        {
            get
            {
                if (_value != null && !_value._inProcess)
                    return _value;
                _value = _factory();
                return _value;
            }
        }
    }
    


}
