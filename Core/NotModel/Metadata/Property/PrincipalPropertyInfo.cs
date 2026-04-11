using System;
using Not.Core.Model.Metadata;

namespace Not.Core.Model.Metadata.Property
{
    /// <summary>
    /// Marks the principal (non-FK) side of a one-to-one relationship.
    /// The dependent side (<see cref="ReferencePropertyInfo"/>) owns the FK column and
    /// fully configures the EF Core relationship via HasOne.WithOne.HasForeignKey.
    /// This declaration is skipped during EF mapping — its only role is to make the
    /// navigation visible in <see cref="ClassInfo.Properties"/> and to express intent.
    /// </summary>
    public abstract class PrincipalPropertyInfo : PropertyInfo
    {
        public abstract Type TargetType { get; }

        /// <summary>Name of the back-reference on the dependent entity (optional).</summary>
        public string? InverseNavigation { get; protected init; }

        private ClassInfo? _targetClass;
        public ClassInfo TargetClass => _targetClass ??=
            (TargetType.GetField("ClassInfo")?.GetValue(null) as ClassInfo)!;
    }

    public class PrincipalPropertyInfo<BO, TargetBO> : PrincipalPropertyInfo
        where BO : BusinessObject
        where TargetBO : BusinessObject
    {
        public override Type TargetType => typeof(TargetBO);

        public PrincipalPropertyInfo(string propertyName, string? inverseNavigation = null)
        {
            _propertyInfo = typeof(BO).GetProperty(propertyName)
                ?? throw new Exception($"Property '{propertyName}' not found for {typeof(BO).FullName}");
            InverseNavigation = inverseNavigation;
        }
    }
}
