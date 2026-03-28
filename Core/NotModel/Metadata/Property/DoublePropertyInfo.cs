using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.Model.Metadata.Property
{
    public class DoublePropertyInfo<BO> : PropertyInfo<BO, double>
        where BO : BusinessObject
    {
        public DoublePropertyInfo(string propertyName)
            : base(propertyName) { }
    }
}
