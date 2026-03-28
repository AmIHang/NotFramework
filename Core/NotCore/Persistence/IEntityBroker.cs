using Not.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.Persistence
{
    public interface IEntityBroker
    {
        public object Create(Type type);
        public BO Create<BO>()
            where BO : BusinessObject
            => (BO)Create(typeof(BO));

        public void Delete(BusinessObject bo);

        public EntityState GetEntityState(BusinessObject bo);
        //public bool IsModified(BusinessObject bo, )
    }
}
