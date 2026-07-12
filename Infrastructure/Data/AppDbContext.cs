using Application.Common;
using Domain.Audit;
using Domain.Common;
using Domain.Tenants;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    private const long SystemUserId = 0;
    private readonly IRequestContext? _requestContext;
    public AppDbContext(DbContextOptions<AppDbContext> options,IRequestContext? requestContext = null) : base(options)
    {
        _requestContext = requestContext;
    }
    
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ActionLog> ActionLogs => Set<ActionLog>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<ManagerRegionAccess> ManagerRegionAccesses => Set<ManagerRegionAccess>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.HasOne(e => e.User)
                .WithMany(e => e.UserRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Role)
                .WithMany(e => e.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.RoleId);
        });
        
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(x => new { x.RoleId, x.PermissionId });

            entity.HasOne(x => x.Role)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Permission)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PermissionId);
        });
        
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).HasMaxLength(500).IsRequired();
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.ExpiresAt);

            entity.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ActionLog>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UsernameSnapshot).HasMaxLength(100);
            entity.Property(x => x.Action).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Module).HasMaxLength(100).IsRequired();
            entity.Property(x => x.EntityName).HasMaxLength(100);
            entity.Property(x => x.EntityId).HasMaxLength(100);
            entity.Property(x => x.HttpMethod).HasMaxLength(20);
            entity.Property(x => x.Path).HasMaxLength(500);
            entity.Property(x => x.IpAddress).HasMaxLength(100);
            entity.Property(x => x.UserAgent).HasMaxLength(500);
            entity.Property(x => x.FailureReason).HasMaxLength(500);
            entity.Property(x => x.CorrelationId).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.Action);
            entity.HasIndex(x => x.CorrelationId);
        });
        
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(100);
            entity.Property(x => x.Code).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Locale).HasMaxLength(20).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.RegionId).HasMaxLength(100);
            entity.HasIndex(x => x.RegionId);
            entity.HasOne(x => x.Region)
                .WithMany(x => x.Tenants)
                .HasForeignKey(x => x.RegionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TenantUser>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.UserId }).IsUnique();

            entity.HasOne(x => x.Tenant)
                .WithMany(x => x.TenantUsers)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.User)
                .WithMany(x => x.TenantUsers)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<Region>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(100);
            entity.Property(x => x.Code).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
        });
        
        modelBuilder.Entity<ManagerRegionAccess>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RegionId).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.RegionId);
            entity.HasIndex(x => new { x.UserId, x.RegionId }).IsUnique();

            entity.HasOne(x => x.User)
                .WithMany(x => x.ManagerRegionAccesses)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Region)
                .WithMany(x => x.ManagerRegionAccesses)
                .HasForeignKey(x => x.RegionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
    public override int SaveChanges()
    {
        ApplyAuditFields();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditFields();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditFields()
    {
        ChangeTracker.DetectChanges();

        var now = DateTimeOffset.UtcNow;
        var userId = _requestContext?.UserId ?? SystemUserId;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = now;
                }

                if (entry.Entity.CreatedBy == default)
                {
                    entry.Entity.CreatedBy = userId;
                }
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = userId;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ISoftDeletableEntity>())
        {
            if (entry.State == EntityState.Modified &&
                entry.Entity.IsDeleted &&
                entry.Entity.DeletedAt is null)
            {
                entry.Entity.DeletedAt = now;
            }
        }
    }
}