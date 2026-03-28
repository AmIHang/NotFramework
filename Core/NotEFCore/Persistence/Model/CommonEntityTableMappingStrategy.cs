using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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

            foreach(var prop in properties)
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
