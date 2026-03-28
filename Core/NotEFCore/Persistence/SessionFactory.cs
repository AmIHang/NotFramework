using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.Persistence
{
    public class SessionFactory : ISessionFactory
    {
        private readonly IServiceScopeFactory _factory;
        public SessionFactory(IServiceScopeFactory factory)
        {
            _factory = factory;
        }

        public ISession Create()
            => new Session(_factory.CreateScope());
    }
}
