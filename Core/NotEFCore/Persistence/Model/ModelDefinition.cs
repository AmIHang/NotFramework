using Microsoft.EntityFrameworkCore;
using Not.Core.Model.Metadata;
using Not.Core.Persistence;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.EF.Persistence.Model
{
    public abstract class ModelDefinition : DatabaseContext
    {
        public abstract Guid Guid { get; }
        public abstract string Name { get; }
        public abstract string Version { get; }

        private IEnumerable<ClassInfo>? _classInfos = null;
        public IEnumerable<ClassInfo> ClassInfos
        {
            get
            {
                if(_classInfos == null)
                    _classInfos = _includedModels.SelectMany(x => x.Value.GetClassInfos());
                return [.. _classInfos];
            }
        }


        private Dictionary<Guid, ModelDefinition> _includedModels = new();

        public ModelDefinition(DbContextOptions options)
            : base(options) 
        {
            _includedModels.Add(Guid, this);
        }



        public void Include(ModelDefinition dependency)
        {
            if (_includedModels.ContainsKey(dependency.Guid))
                throw new Exception("Module already added");

            _includedModels[dependency.Guid] = dependency;
        }

        protected abstract IEnumerable<ClassInfo> GetClassInfos();

        protected virtual Dictionary<string, IEntityTableMappingStrategy> GetEntityMappingStrategies()
        {
            var tph = new TphEntityTableMappingStrategy();
            var tpt = new TptEntityTablingMappingStrategy();
            return new Dictionary<string, IEntityTableMappingStrategy>()
            {
                { tph.ShortName, tph },
                { tpt.ShortName, tpt },
            };
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var mappingStrategies = GetEntityMappingStrategies();

            foreach(var ci in ClassInfos)
            {
                if (!mappingStrategies.TryGetValue(ci.TableMappingStrategy, out var strat))
                    throw new Exception("Invalid strat");
                strat.Register(modelBuilder, ci);
            }
        }
    }
}
