using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.Model.Metadata.Property
{
    public class LongPropertyInfo<BO> : PropertyInfo<BO, long>
        where BO : BusinessObject
    {
        public LongPropertyInfo(string propertyName)
            : base(propertyName) { }
    }
}
