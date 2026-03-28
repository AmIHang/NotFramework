using Microsoft.EntityFrameworkCore;
using Not.Core.Model.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.EF.Persistence.Model
{
    public class TptEntityTablingMappingStrategy : CommonEntityTableMappingStrategy
    {
        public override string ShortName => "TPT";
        public override void Register(ModelBuilder builder, ClassInfo classInfo)
        {
            builder.Entity(classInfo.Type, x =>
            {
                ArgumentNullException.ThrowIfNull(builder);
                ArgumentNullException.ThrowIfNull(classInfo);

                x.UseTptMappingStrategy()
                .ToTable(classInfo.TableName);

                if (classInfo.IsDerivation)
                    x.HasBaseType(classInfo.BaseType);
                else
                    x.HasKey("OID");

                BuildProperties(x, classInfo);
            });
        }
    }
}
