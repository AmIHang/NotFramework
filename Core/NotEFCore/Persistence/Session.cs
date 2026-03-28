using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.Extensions.DependencyInjection;
using Not.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.Persistence
{
    public class Session : ISession
    {
        private static readonly AsyncLocal<Session?> _current = new();
        public static Session? Current
        {
            get => _current.Value;
            internal set => _current.Value = value;
        }

        private readonly IServiceScope _scope;
        public readonly DatabaseContext _dbc;
        private readonly IEntityBroker _broker;

        public IEntityBroker Broker
            => _broker;

        internal Session(IServiceScope scope)
        {
            _scope = scope;
            _dbc = _scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            _broker = new EntityBroker(_dbc);
            Current = this;
        }

        public IQueryable Query(Type type)
        {
            if (!type.IsAssignableTo(typeof(BusinessObject)))
                throw new Exception($"Type must be derived of {typeof(BusinessObject).FullName}");

            var method = _dbc.GetType()
                .GetMethod(nameof(DatabaseContext.Set), Type.EmptyTypes)!;

            var generic = method.MakeGenericMethod(type);
            return (IQueryable)generic.Invoke(_dbc, null);
        }

        public IQueryable<BO> Query<BO>()
            where BO : BusinessObject
        {
            return _dbc.Set<BO>();
        }

        public void Commit()
        {
            _dbc.SaveChanges();
        }

        public object GetService(Type type)
            => _scope.ServiceProvider.GetService(type);

        public IEnumerable<object> GetServices(Type type)
            => _scope.ServiceProvider.GetServices(type);
       public object GetRequiredService(Type type)
            => _scope.ServiceProvider.GetRequiredService(type);

        public void Dispose()
        {
            _dbc.Dispose();
            _scope.Dispose();
            Current = null;
        }
    }
}
