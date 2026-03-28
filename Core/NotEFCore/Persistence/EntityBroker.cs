using Microsoft.EntityFrameworkCore;
using Not.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Not.Core.Persistence
{
    public class EntityBroker : IEntityBroker
    {
        private readonly Dictionary<Type, object> _dbSets = [];
        private readonly DatabaseContext _dbc;

        public EntityBroker(DatabaseContext dbc)
        {
            if (dbc is null)
                throw new ArgumentNullException(nameof(dbc));

            _dbc = dbc;
        }

        public object Create(Type type)
        {
            if(type == null)
               throw new ArgumentNullException(nameof(type));

            if (!type.IsAssignableTo(typeof(BusinessObject)))
                throw new Exception($"Type must be derived of {typeof(BusinessObject).FullName}");

            var constructor = type.GetConstructor(Array.Empty<Type>());
            var businessObejct = constructor.Invoke(null);
            _dbc.Add(businessObejct);
            return businessObejct;
        }

        public void Delete(BusinessObject bo)
        {
            if (bo is null)
                throw new ArgumentNullException(nameof(bo));
            _dbc.Remove(bo);
        }

        public EntityState GetEntityState(BusinessObject bo)
        {
            if (bo is null)
                throw new ArgumentNullException(nameof(bo));

            var efState = _dbc.Entry(bo).State;


            switch(_dbc.Entry(bo).State)
            {
                case Microsoft.EntityFrameworkCore.EntityState.Added:
                    return EntityState.Added;
                case Microsoft.EntityFrameworkCore.EntityState.Unchanged:
                    return EntityState.Current;
                case Microsoft.EntityFrameworkCore.EntityState.Modified:
                    return EntityState.Modified;
                case Microsoft.EntityFrameworkCore.EntityState.Deleted:
                    return EntityState.Deleted;
                case Microsoft.EntityFrameworkCore.EntityState.Detached:
                    throw new Exception("Untracked");
                default:
                    throw new Exception("Unknown state");

            }
        }
    }
}
