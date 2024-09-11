using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Firefly.Box;
using Firefly.Box.Data;

namespace ENV.Data
{
    public class SortCollection:IEnumerable<Sort>
    {
        Firefly.Box.Data.Advanced.SortCollection  _source;
        public int Count { get { return _source.Count; }
        }

        public SortCollection(Firefly.Box.Data.Advanced.SortCollection source)
        {
            _source = source;
        }

        public void Add(params Sort[] orderBy)
        {
            _source.Add(orderBy);
        }

        public Sort this[int oneBasedIndex]
        {
            get
            {
                if (oneBasedIndex < 1 || oneBasedIndex > _source.Count)
                    return new Sort();
                return _source[oneBasedIndex -1 ];
            }
        }

        public int IndexOf(Sort item)
        {
            return _source.IndexOf(item)+1;
        }

        public IEnumerator<Sort> GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _source).GetEnumerator();
        }

        public override string ToString()
        {
            return _source.ToString();
        }

        public void Clear()
        {
            _source.Clear();
        }
    }
}