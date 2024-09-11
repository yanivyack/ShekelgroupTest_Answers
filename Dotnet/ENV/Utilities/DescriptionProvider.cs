using System.Collections.Generic;
using Firefly.Box.Data.Advanced;

namespace ENV.Utilities
{
    public class DescriptionProvider
    {
        class ColumnCache<valueType, descriptionType>
        {

            Dictionary<valueType, descriptionType> _cache = new Dictionary<valueType, descriptionType>();
            TypedColumnBase<valueType> _dataColumn;
            TypedColumnBase<descriptionType> _descriptionColumn;
            Firefly.Box.BusinessProcess _bp = new Firefly.Box.BusinessProcess();
            public ColumnCache(
                TypedColumnBase<valueType> dataColumn,
                ENV.Data.Entity e,
                TypedColumnBase<valueType> valueColumn,
                TypedColumnBase<descriptionType> descriptionColumn,FilterBase staticFilter)
            {
                _dataColumn = dataColumn;
                _descriptionColumn = descriptionColumn;
                _bp.From = e;
                _bp.Columns.Add(valueColumn, descriptionColumn);
                _bp.Where.Add(valueColumn.IsEqualTo(dataColumn));
                if (staticFilter != null)
                    _bp.Where.Add(staticFilter);
            }
            internal descriptionType GetResult()
            {
                if (ReferenceEquals(_dataColumn.Value, null))
                    return default(descriptionType);
                descriptionType result;
                if (!_cache.TryGetValue(_dataColumn.Value, out result))
                {
                    result = default(descriptionType);
                    _bp.ForFirstRow(() => { result = _descriptionColumn; });
                    _cache.Add(_dataColumn.Value, result);
                }
                return result;
            }
        }
        Dictionary<ColumnBase, object> _columns = new Dictionary<ColumnBase, object>();
        public  descriptionType GetDescription<valueType, descriptionType>(
            TypedColumnBase<valueType> dataColumn,
            ENV.Data.Entity e,
            TypedColumnBase<valueType> valueColumn,
            TypedColumnBase<descriptionType> descriptionColumn
            )
        {
            return GetDescription(dataColumn, e, valueColumn, descriptionColumn,null);
        }

        public  descriptionType GetDescription<valueType, descriptionType>(
            TypedColumnBase<valueType> dataColumn,
            ENV.Data.Entity e,
            TypedColumnBase<valueType> valueColumn,
            TypedColumnBase<descriptionType> descriptionColumn,FilterBase staticFilter
            )
        {
            object result;
            if (!_columns.TryGetValue(dataColumn, out result))
            {
                result = new ColumnCache<valueType, descriptionType>(dataColumn, e, valueColumn, descriptionColumn,staticFilter);
                _columns.Add(dataColumn, result);
            }
            return ((ColumnCache<valueType, descriptionType>)result).GetResult();
        }
    }
}