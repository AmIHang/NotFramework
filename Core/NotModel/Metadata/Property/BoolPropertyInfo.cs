using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.Model.Metadata.Property
{
    public class BoolPropertyInfo<BO> : PropertyInfo<BO, bool>
        where BO : BusinessObject
    {
        public BoolPropertyInfo(string propertyName) 
            : base(propertyName) { }
    }
}
