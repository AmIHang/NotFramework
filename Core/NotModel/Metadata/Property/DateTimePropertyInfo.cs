using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.Model.Metadata.Property
{
    public class DateTimePropertyInfo<BO> : PropertyInfo<BO, DateTime>
        where BO : BusinessObject
    {
        public DateTimePropertyInfo(string propertyName)
            : base(propertyName) { }
    }
}
