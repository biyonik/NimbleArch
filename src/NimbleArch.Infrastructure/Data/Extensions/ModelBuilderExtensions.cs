using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NimbleArch.Core.Entities.Features;
using NimbleArch.Core.MultiTenancy;

namespace NimbleArch.Infrastructure.Data.Extensions;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies global query filters to the model.
    /// </summary>
    public static void ApplyGlobalFilters(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Soft delete filter
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var condition = Expression.Equal(property, Expression.Constant(false));
                var lambda = Expression.Lambda(condition, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }

            // Multi-tenant filter
            if (typeof(IHasTenant).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(IHasTenant.TenantId));
                var tenantId = Expression.Constant(TenantContext.Current?.TenantId);
                var condition = Expression.Equal(property, tenantId);
                var lambda = Expression.Lambda(condition, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}