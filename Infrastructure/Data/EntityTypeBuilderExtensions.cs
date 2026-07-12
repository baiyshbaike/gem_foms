using Domain.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data;

public static class EntityTypeBuilderExtensions
{
    public static void ConfigureTenant<TEntity>(this EntityTypeBuilder<TEntity> entity)
        where TEntity : class, ITenantEntity
    {
        entity.Property(x => x.TenantId).HasMaxLength(100).IsRequired();
        entity.HasIndex(x => x.TenantId);
    }

    public static void ConfigureAuditable<TEntity>(this EntityTypeBuilder<TEntity> entity)
        where TEntity : class, IAuditableEntity
    {
        entity.Property(x => x.CreatedAt).IsRequired();
        entity.Property(x => x.CreatedBy).IsRequired();
        entity.HasIndex(x => x.CreatedAt);
    }

    public static void ConfigureSoftDelete<TEntity>(this EntityTypeBuilder<TEntity> entity)
        where TEntity : class, ISoftDeletableEntity
    {
        entity.Property(x => x.IsDeleted).IsRequired();
        entity.HasIndex(x => x.IsDeleted);
    }

    public static void ConfigureActive<TEntity>(this EntityTypeBuilder<TEntity> entity)
        where TEntity : class, IActiveEntity
    {
        entity.Property(x => x.IsActive).IsRequired();
        entity.HasIndex(x => x.IsActive);
    }
}