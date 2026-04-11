using System;
using Not.Core.Model.Metadata;

namespace Not.Core.Model.Metadata.Property
{
    public abstract class ReferencePropertyInfo : PropertyInfo
    {
        public abstract Type TargetType { get; }

        /// <summary>
        /// Name of the collection navigation property on the target entity that contains
        /// all instances of this entity (the inverse side of the relationship). Null if no inverse.
        /// </summary>
        public string? InverseNavigation { get; protected init; }

        private ClassInfo? _targetClass;
        public ClassInfo TargetClass => _targetClass ??=
            (TargetType.GetField("ClassInfo")?.GetValue(null) as ClassInfo)!;
    }

    public class ReferencePropertyInfo<BO, TargetBO> : ReferencePropertyInfo
        where BO : BusinessObject
        where TargetBO : BusinessObject
    {
        public override Type TargetType => typeof(TargetBO);

        public ReferencePropertyInfo(string propertyName, string? inverseNavigation = null)
        {
            _propertyInfo = typeof(BO).GetProperty(propertyName)
                ?? throw new Exception($"Property '{propertyName}' not found for {typeof(BO).FullName}");
            InverseNavigation = inverseNavigation;
        }
    }
}
