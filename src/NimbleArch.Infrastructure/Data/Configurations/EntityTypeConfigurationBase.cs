using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NimbleArch.Core.Entities;
using NimbleArch.Core.Entities.Features;

namespace NimbleArch.Infrastructure.Data.Configurations;

/// <summary>
/// Base configuration class for entities with performance optimizations.
/// </summary>
public abstract class EntityTypeConfigurationBase<TEntity, TKey> : IEntityTypeConfiguration<TEntity>
    where TEntity : EntityBase<TKey>
    where TKey : struct
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // Base configuration
        builder.HasKey(e => e.Id);
        
        // Version tracking
        builder.Property(e => e.Version)
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        // State tracking
        builder.Property("State")
            .HasConversion<string>()
            .HasMaxLength(50);

        // Modified properties tracking
        builder.Property<string>("_modifiedPropertiesJson")
            .HasColumnName("ModifiedProperties")
            .HasMaxLength(4000);

        // Tenant handling
        if (typeof(IHasTenant).IsAssignableFrom(typeof(TEntity)))
        {
            builder.Property<string>("TenantId")
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex("TenantId")
                .HasFilter(null);
        }

        // Soft delete handling
        if (typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
        {
            builder.Property<bool>("IsDeleted")
                .HasDefaultValue(false);

            builder.Property<DateTime?>("DeletedAt");
            builder.Property<string>("DeletedBy")
                .HasMaxLength(100);

            builder.HasIndex("IsDeleted");
        }

        // Audit fields
        if (typeof(IAuditable).IsAssignableFrom(typeof(TEntity)))
        {
            builder.Property<DateTime>("CreatedAt")
                .IsRequired();
            
            builder.Property<string>("CreatedBy")
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property<DateTime?>("UpdatedAt");
            
            builder.Property<string>("UpdatedBy")
                .HasMaxLength(100);

            builder.HasIndex("CreatedAt");
        }

        // Domain events handling
        builder.Ignore(e => e.DomainEvents);

        // Performance optimizations
        ConfigureIndexes(builder);
        ConfigureNavigations(builder);
    }

    /// <summary>
    /// Configures performance-optimized indexes.
    /// </summary>
    protected virtual void ConfigureIndexes(EntityTypeBuilder<TEntity> builder)
    {
        // Override in derived classes for entity-specific indexes
    }

    /// <summary>
    /// Configures navigation properties with loading strategies.
    /// </summary>
    protected virtual void ConfigureNavigations(EntityTypeBuilder<TEntity> builder)
    {
        // Override in derived classes for entity-specific navigation configurations
    }
}
