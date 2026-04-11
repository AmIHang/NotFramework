using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Not.Core.Localization;
using Not.Core.Model.Localization;
using Not.Core.Model.Metadata.Property;
using Not.Core.Model.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.EF.Persistence.Model
{
    public abstract class CommonEntityTableMappingStrategy : IEntityTableMappingStrategy
    {
        public abstract string ShortName { get; }

        public abstract void Register(ModelBuilder builder, ClassInfo classInfo);

        protected virtual void BuildProperties(EntityTypeBuilder builder, ClassInfo classInfo)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(classInfo);

            var properties = classInfo.Type
                .GetFields()
                .Where(x => x.DeclaringType == classInfo.Type)
                .Select(x => x.GetValue(classInfo) as PropertyInfo)
                .Where(x => x != null);

            foreach (var prop in properties)
            {
                if (prop is PrincipalPropertyInfo)
                {
                    // The dependent side (ReferencePropertyInfo on the target entity) owns
                    // the EF configuration for 1:1 relationships. Skip here.
                }
                else if (prop is NavigationPropertyInfo navProp)
                {
                    // If the target entity declares a matching ReferencePropertyInfo for the
                    // inverse navigation, that side owns the relationship configuration
                    // (FK side is authoritative). Skip here to avoid duplicate EF setup.
                    bool inverseOwnsConfig = navProp.InverseNavigation != null
                        && navProp.TargetClass?.Properties
                            .OfType<ReferencePropertyInfo>()
                            .Any(r => r.PropertyName == navProp.InverseNavigation) == true;

                    if (!inverseOwnsConfig)
                    {
                        builder.HasMany(navProp.TargetType, navProp.PropertyName)
                            .WithOne(navProp.InverseNavigation)
                            .HasForeignKey(navProp.ForeignKeyProperty);
                    }
                }
                else if (prop is ReferencePropertyInfo refProp)
                {
                    // Detect 1:1: the inverse navigation on the target is a PrincipalPropertyInfo,
                    // not a NavigationPropertyInfo (no collection on the other side).
                    bool isOneToOne = refProp.InverseNavigation != null
                        && refProp.TargetClass?.Properties
                            .OfType<PrincipalPropertyInfo>()
                            .Any(p => p.PropertyName == refProp.InverseNavigation) == true;

                    if (isOneToOne)
                    {
                        // Dependent side (this entity) owns FK column.
                        // HasForeignKey(classInfo.Type) tells EF which side has the FK;
                        // the shadow property name is determined by EF convention.
                        builder.HasOne(refProp.TargetType, refProp.PropertyName)
                            .WithOne(refProp.InverseNavigation)
                            .HasForeignKey(classInfo.Type)
                            .IsRequired(refProp.IsRequired);
                    }
                    else
                    {
                        // 1:n — FK side fully configures the relationship.
                        // WithMany(InverseNavigation) registers the collection on the target too.
                        builder.HasOne(refProp.TargetType, refProp.PropertyName)
                            .WithMany(refProp.InverseNavigation)
                            .IsRequired(refProp.IsRequired);
                    }
                }
                else if (prop.PropertyType == typeof(LocalizedString))
                {
                    // Map the CLR property to the invariant column so EF Core can resolve the type.
                    builder.Property(prop.PropertyType, prop.PropertyName)
                           .HasColumnName(prop.ColumnName)
                           .HasConversion(new LocalizedStringInvariantConverter())
                           .IsRequired(false);

                    // Register one shadow property per non-invariant configured culture.
                    foreach (var culture in LocalizationConfig.Current.SupportedCultures
                        .Where(c => c.Code != ""))
                    {
                        var shadowKey = LocalizedStringSync.GetShadowPropName(prop.PropertyName, culture.Code);
                        var colName   = LocalizedStringSync.GetColumnName(prop.ColumnName, culture.Code);
                        builder.Property<string>(shadowKey)
                               .HasColumnName(colName)
                               .IsRequired(false);
                    }
                }
                else
                {
                    var pBuilder = builder.Property(prop.PropertyType, prop.PropertyName)
                        .HasColumnName(prop.ColumnName)
                        .IsRequired(prop.IsRequired);

                    if (prop is IDecimalProperty decimalProp)
                        pBuilder.HasPrecision(decimalProp.Precision, decimalProp.Scale);
                }
            }
        }
    }
}
