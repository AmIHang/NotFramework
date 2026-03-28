using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Not.Core.EF.Persistence.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.Persistence
{
    public class DatabaseContext : DbContext
    {
        private bool _trackerSubscribed = false;

        public DatabaseContext(DbContextOptions options)
            : base(options)
        {
            // NOTE: Do NOT access ChangeTracker here.
            // Accessing ChangeTracker → IStateManager → IModel triggers model building
            // before ModelDefinition._includedModels is populated, resulting in an empty model.
        }

        // ── Lazy tracker subscription ─────────────────────────────────────────

        /// <summary>
        /// Subscribes to ChangeTracker.Tracked the first time it is safe to do so
        /// (after the model has been built and the full constructor chain completed).
        /// Called from Set&lt;T&gt;() and SaveChanges() which are always invoked post-construction.
        /// </summary>
        private void EnsureTrackerSubscribed()
        {
            if (_trackerSubscribed) return;
            _trackerSubscribed = true;
            ChangeTracker.Tracked += OnEntityTracked;
        }

        /// <summary>
        /// After an entity is loaded from the database, copy shadow property values
        /// into the LocalizedString CLR properties.
        /// </summary>
        private void OnEntityTracked(object? sender, EntityTrackedEventArgs e)
        {
            if (e.FromQuery)
                LocalizedStringSync.SyncFromShadow(e.Entry);
        }

        // ── DbSet access ──────────────────────────────────────────────────────

        public override DbSet<TEntity> Set<TEntity>()
        {
            EnsureTrackerSubscribed();
            return base.Set<TEntity>();
        }

        public override DbSet<TEntity> Set<TEntity>(string name)
        {
            EnsureTrackerSubscribed();
            return base.Set<TEntity>(name);
        }

        // ── SaveChanges overrides ─────────────────────────────────────────────

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            EnsureTrackerSubscribed();
            SyncLocalizedStringsToShadow();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            EnsureTrackerSubscribed();
            SyncLocalizedStringsToShadow();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void SyncLocalizedStringsToShadow()
        {
            foreach (var entry in ChangeTracker.Entries()
                .Where(e => e.State is Microsoft.EntityFrameworkCore.EntityState.Added
                                    or Microsoft.EntityFrameworkCore.EntityState.Modified))
            {
                LocalizedStringSync.SyncToShadow(entry);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
