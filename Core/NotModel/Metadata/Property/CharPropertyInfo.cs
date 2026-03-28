using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.Model.Metadata.Property
{
    public class CharPropertyInfo<BO> : PropertyInfo<BO, char>
        where BO : BusinessObject
    {
        public CharPropertyInfo(string propertyName)
            : base(propertyName) { }
    }
}
