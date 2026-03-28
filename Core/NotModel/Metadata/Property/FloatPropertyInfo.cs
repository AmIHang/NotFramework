using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.Model.Metadata.Property
{
    public class FloatPropertyInfo<BO> : PropertyInfo<BO, float>
        where BO : BusinessObject
    {
        public FloatPropertyInfo(string propertyName)
            : base(propertyName) { }
    }
}
