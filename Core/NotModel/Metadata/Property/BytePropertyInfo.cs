using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.Model.Metadata.Property
{
    public class BytePropertyInfo<BO> : PropertyInfo<BO, byte>
        where BO : BusinessObject
    {
        public BytePropertyInfo(string propertyName)
            : base(propertyName) { }
    }
}
