using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ENV.Utilities
{
     class Lazy<baseType>
    {
        Create _create;

        public Lazy(Lazy<baseType>.Create create)
        {
            _create = create;
        }
        public Lazy()
            : this(delegate()
            {
                return (baseType)Activator.CreateInstance(typeof(baseType));
            })
        {

        }

        baseType _val;
        public baseType Instance
        {
            get
            {
                if (_val != null)
                    return _val;
                return _val = _create();
            }
        }
        public delegate baseType Create();
        public static implicit operator baseType(Lazy<baseType> a)
        {
            return a.Instance;
        }

        public void DoIfInstanciated(System.Action<baseType> action)
        {
            if (_val != null)
                action(_val);
        }

        public void CreateInstance()
        {
            _val = _create();
        }
        public override string ToString()
        {
            return Instance.ToString();
        }

        public void Dispose()
        {
            IDisposable p = _val as IDisposable;
            if (p != null)
                p.Dispose();

        }
    }
}
