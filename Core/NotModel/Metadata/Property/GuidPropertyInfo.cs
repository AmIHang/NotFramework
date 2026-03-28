using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.Model.Metadata.Property
{
    public class GuidPropertyInfo<BO> : PropertyInfo<BO, Guid>
        where BO : BusinessObject
    {
        public GuidPropertyInfo(string propertyName)
            : base(propertyName) { }
    }
}
