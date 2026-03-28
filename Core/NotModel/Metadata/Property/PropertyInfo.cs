using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.Model.Metadata.Property
{
    public abstract class PropertyInfo
    {
        internal System.Reflection.PropertyInfo _propertyInfo { get; init; }
        public string PropertyName { get =>  _propertyInfo.Name; }
        public bool IsRequired { get; set; }
        public Type PropertyType { get => _propertyInfo.PropertyType; }

        private string? _columnName = null;
        public string ColumnName
        {
            get => _columnName ?? (_columnName = PropertyName);
            set => _columnName = value;
        }

    }

    public abstract class PropertyInfo<BO, PV> : PropertyInfo
        where BO: BusinessObject
    {
        public PropertyInfo(string propertyName)
        {
            _propertyInfo = typeof(BO).GetProperty(propertyName)
                ?? throw new Exception($"Property '{propertyName}' not found for {typeof(BO).FullName}");
        }
    }
}
