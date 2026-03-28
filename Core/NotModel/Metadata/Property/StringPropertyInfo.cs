using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.Model.Metadata.Property
{
    public class StringPropertyInfo<BO> : PropertyInfo<BO, string>
        where BO : BusinessObject
    {
        public int MaxLength { get; set; }
        public StringPropertyInfo(string propertyName) 
            : base(propertyName) { }
    }
}
