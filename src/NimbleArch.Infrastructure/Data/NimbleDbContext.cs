using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NimbleArch.Core.Common.ObjectPool;
using NimbleArch.Core.Entities;
using NimbleArch.Core.Entities.Base;
using NimbleArch.Core.Entities.Factory;
using NimbleArch.Core.Entities.Features;
using NimbleArch.Core.MultiTenancy;
using NimbleArch.Infrastructure.Data.Exceptions;
using NimbleArch.Infrastructure.Data.Extensions;
using EntityState = Microsoft.EntityFrameworkCore.EntityState;

namespace NimbleArch.Infrastructure.Data;

/// <summary>
/// High-performance database context with optimized change tracking.
/// </summary>
/// <remarks>
/// EN: Provides an optimized DbContext implementation with support for
/// compile-time query generation and efficient change tracking.
///
/// TR: Derleme zamanında sorgu oluşturma ve verimli değişiklik takibi
/// desteği ile optimize edilmiş DbContext implementasyonu sağlar.
/// </remarks>
public class NimbleDbContext : DbContext
{
    private readonly bool _enableAutoDetectChanges;
    private readonly QueryTrackingBehavior _queryTrackingBehavior;
    private readonly IEntityStateManager<EntityBase<Guid>, Guid> _stateManager;
    private readonly ILogger<NimbleDbContext> _logger;

    private readonly IObjectPool<IEntityFactory<EntityBase<Guid>, Guid>> _entityFactoryPool;

    public NimbleDbContext(
        DbContextOptions<NimbleDbContext> options,
        ILogger<NimbleDbContext> logger,
        IEntityStateManager<EntityBase<Guid>, Guid> stateManager,
        IObjectPool<IEntityFactory<EntityBase<Guid>, Guid>> entityFactoryPool,
        bool enableAutoDetectChanges = false,
        QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.NoTracking)
        : base(options)
    {
        _logger = logger;
        _stateManager = stateManager;
        _entityFactoryPool = entityFactoryPool;
        _enableAutoDetectChanges = enableAutoDetectChanges;
        _queryTrackingBehavior = queryTrackingBehavior;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Global query filters
        modelBuilder.ApplyGlobalFilters();
        
        // Entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NimbleDbContext).Assembly);

        // Snapshotting configuration
        ConfigureSnapshots(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private void ConfigureSnapshots(ModelBuilder modelBuilder)
    {
        var types = modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(ISnapshotable<>).IsAssignableFrom(t.ClrType));

        foreach (var type in types)
        {
            var snapshotType = typeof(EntitySnapshot<>).MakeGenericType(type.ClrType);
            modelBuilder.Entity(snapshotType).ToTable($"{type.ClrType.Name}Snapshots");
        }
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        try
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified)
                .ToList();

            foreach (var entry in entries)
            {
                var entity = entry.Entity as EntityBase<Guid>;
                if (entity == null) continue;

                // Handle domain events
                var events = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();

                switch (entity)
                {
                    // Check if snapshot needed
                    case ISnapshotable<EntitySnapshot<Guid>> snapshotable when 
                        snapshotable.NeedsSnapshot(events.Count):
                    {
                        var snapshot = snapshotable.CreateSnapshot();
                        Set<EntitySnapshot<Guid>>().Add(snapshot);
                        break;
                    }
                    // Handle audit fields
                    case IAuditable auditable:
                    {
                        if (entry.State == EntityState.Added)
                        {
                            auditable.CreatedAt = DateTime.UtcNow;
                            auditable.CreatedBy = TenantContext.Current?.Name ?? "System";
                        }
                        auditable.UpdatedAt = DateTime.UtcNow;
                        auditable.UpdatedBy = TenantContext.Current?.Name ?? "System";
                        break;
                    }
                }
            }

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error saving changes to database");
            throw new DataAccessException("Failed to save changes", ex);
        }
    }
}