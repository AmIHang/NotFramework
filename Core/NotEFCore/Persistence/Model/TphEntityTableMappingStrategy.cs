using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Not.Core.Model.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.EF.Persistence.Model
{
    public class TphEntityTableMappingStrategy : CommonEntityTableMappingStrategy
    {
        public override string ShortName => "TPH";
       
        public override void Register(ModelBuilder builder, ClassInfo classInfo)
        {
            builder.Entity(classInfo.Type, x =>
            {
                x.UseTphMappingStrategy();
                if(classInfo.IsDerivation)
                {
                    x.HasBaseType(classInfo.RootType);

                    builder.Entity(classInfo.RootType)
                        .HasDiscriminator<string>("@CID")
                        .HasValue(classInfo.RootType, classInfo.RootClassInfo.CID.ToString("X"))
                        .HasValue(classInfo.Type, classInfo.CID.ToString("X"));
                }
                else
                {
                    x.ToTable(classInfo.TableName)
                    .HasKey("OID");
                }

                BuildProperties(x, classInfo);
            });
        }
    }
}
