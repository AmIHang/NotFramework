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
    public interface IEntityTableMappingStrategy
    {
        public string ShortName { get; }
        public void Register(ModelBuilder builder, ClassInfo classInfo);
    }
}
