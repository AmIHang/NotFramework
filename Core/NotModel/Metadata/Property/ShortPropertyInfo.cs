using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.Model.Metadata.Property
{
    public class ShortPropertyInfo<BO> : PropertyInfo<BO, short>
        where BO : BusinessObject
    {
        public ShortPropertyInfo(string propertyName)
            : base(propertyName) { }
    }
}
