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
                if (prop.PropertyType == typeof(LocalizedString))
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
                    builder.Property(prop.PropertyType, prop.PropertyName)
                           .HasColumnName(prop.ColumnName)
                           .IsRequired(prop.IsRequired);
                }
            }
        }
    }
}
