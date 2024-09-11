using System;
using System.Collections.Generic;

namespace ENV
{
    public class ApplicationEntityCollection
    {

        public Dictionary<int, Type> _entities = new Dictionary<int, Type>();
        internal Dictionary<Type, int> _entitiesIndexes = new Dictionary<Type, int>();

        public Firefly.Box.Data.Entity GetByIndex(int index)
        {
            if (_entities.ContainsKey(index))
                return (Firefly.Box.Data.Entity)System.Activator.CreateInstance(_entities[index]);
            return null;
        }
        public int IndexOf(Type type)
        {
            return _entitiesIndexes[type];
        }



        public void Add(int index, Type entityType)
        {
            if (_entitiesIndexes.ContainsKey(entityType))
                return;
            if (index>0)
            _entities.Add(index, entityType);
            _entitiesIndexes.Add(entityType, index);
        }

        public void Add(Type entityType)
        {
            Add(0,entityType);
        }
    }
}