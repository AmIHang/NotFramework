using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.Model.Metadata.Property
{
    public class IntPropertyInfo<BO> : PropertyInfo<BO, int>
        where BO : BusinessObject
    {
        public IntPropertyInfo(string propertyName)
            : base(propertyName)
        {
        }
    }
}
