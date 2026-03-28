using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.Persistence
{
    public interface ISessionFactory
    {
        ISession Create();
    }
}
