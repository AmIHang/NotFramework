using System;
using Not.Core.Model.Metadata;

namespace Not.Core.Model.Metadata.Property
{
    public abstract class NavigationPropertyInfo : PropertyInfo
    {
        public abstract Type TargetType { get; }
        public string ForeignKeyProperty { get; }

        /// <summary>
        /// Name of the reference navigation property on the target entity that points back
        /// to this entity (the inverse side of the relationship). Null if no inverse navigation.
        /// </summary>
        public string? InverseNavigation { get; }

        private ClassInfo? _targetClass;
        public ClassInfo TargetClass => _targetClass ??=
            (TargetType.GetField("ClassInfo")?.GetValue(null) as ClassInfo)!;

        protected NavigationPropertyInfo(string foreignKeyProperty, string? inverseNavigation)
        {
            ForeignKeyProperty = foreignKeyProperty;
            InverseNavigation = inverseNavigation;
        }
    }

    public class NavigationPropertyInfo<BO, TargetBO> : NavigationPropertyInfo
        where BO : BusinessObject
        where TargetBO : BusinessObject
    {
        public override Type TargetType => typeof(TargetBO);

        public NavigationPropertyInfo(string propertyName, string foreignKeyProperty, string? inverseNavigation = null)
            : base(foreignKeyProperty, inverseNavigation)
        {
            _propertyInfo = typeof(BO).GetProperty(propertyName)
                ?? throw new Exception($"Property '{propertyName}' not found for {typeof(BO).FullName}");
        }
    }
}
