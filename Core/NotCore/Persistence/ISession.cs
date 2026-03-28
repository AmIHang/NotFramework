using Not.Core.Model;
using Not.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.Persistence
{
    public interface ISession : IDisposable
    {
        public IEntityBroker Broker { get; }

        public object GetService(Type type);
        public T GetService<T>()
            where T : IService
           => (T)GetService(typeof(T));

       public IEnumerable<object> GetServices(Type type);
       public IEnumerable<T> GetServices<T>()
            where T: class, IService
            => (IEnumerable<T>)GetServices(typeof(T));

        public object GetRequiredService(Type type);
        public T GetRequiredService<T>()
            where T : class, IService
            => (T)GetRequiredService(typeof(T));

        public IQueryable Query(Type type);
        public IQueryable<BO> Query<BO>()
            where BO : BusinessObject;

        public void Commit();

    }
}
