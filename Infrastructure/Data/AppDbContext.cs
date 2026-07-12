using Application.Common;
using Domain.Audit;
using Domain.Common;
using Domain.MedCards;
using Domain.MedCenters;
using Domain.Patients;
using Domain.Sessions;
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
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientGroup> PatientGroups => Set<PatientGroup>();
    public DbSet<MedCard> MedCards => Set<MedCard>();
    public DbSet<MedCardClinicalInfo> MedCardClinicalInfos => Set<MedCardClinicalInfo>();
    public DbSet<MedCardDiagnosis> MedCardDiagnoses => Set<MedCardDiagnosis>();
    public DbSet<MedCardInfectionScreening> MedCardInfectionScreenings => Set<MedCardInfectionScreening>();
    public DbSet<MedCardApproval> MedCardApprovals => Set<MedCardApproval>();
    public DbSet<HdSession> HdSessions => Set<HdSession>();
    public DbSet<SessionWorkflowSettings> SessionWorkflowSettings => Set<SessionWorkflowSettings>();
    public DbSet<HdSessionMeasurement> HdSessionMeasurements => Set<HdSessionMeasurement>();
    public DbSet<HdSessionPause> HdSessionPauses => Set<HdSessionPause>();
    public DbSet<MedCenterMachine> MedCenterMachines => Set<MedCenterMachine>();
    
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
            entity.Property(x => x.TimeZoneId)
                .HasMaxLength(100)
                .IsRequired();
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
        modelBuilder.Entity<PatientGroup>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.ConfigureAuditable();
            entity.ConfigureActive();

            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.IsSystem).IsRequired();

            entity.HasIndex(x => x.Code).IsUnique();

            var seedDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

            entity.HasData(
                new PatientGroup
                {
                    Id = PatientGroupIds.New,
                    Code = PatientGroupCodes.New,
                    Name = "New",
                    IsSystem = true,
                    IsActive = true,
                    CreatedAt = seedDate,
                    CreatedBy = 0
                },
                new PatientGroup
                {
                    Id = PatientGroupIds.Fresenius,
                    Code = PatientGroupCodes.Fresenius,
                    Name = "Fresenius",
                    IsSystem = true,
                    IsActive = true,
                    CreatedAt = seedDate,
                    CreatedBy = 0
                },
                new PatientGroup
                {
                    Id = PatientGroupIds.Archive,
                    Code = PatientGroupCodes.Archive,
                    Name = "Archive",
                    IsSystem = true,
                    IsActive = true,
                    CreatedAt = seedDate,
                    CreatedBy = 0
                },
                new PatientGroup
                {
                    Id = PatientGroupIds.Foms,
                    Code = PatientGroupCodes.Foms,
                    Name = "Other",
                    IsSystem = true,
                    IsActive = true,
                    CreatedAt = seedDate,
                    CreatedBy = 0
                });
        });
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.ConfigureAuditable();
            entity.ConfigureSoftDelete();
            entity.ConfigureActive();

            entity.Property(x => x.Inn).HasMaxLength(14).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.MiddleName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.BirthDate).IsRequired();
            entity.Property(x => x.Gender).HasConversion<int>().IsRequired();
            entity.Property(x => x.Address).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Address2).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(50).IsRequired();
            entity.Property(x => x.DistrictId).IsRequired();
            entity.Property(x => x.RegionId).IsRequired();
            entity.Property(x => x.GroupId).IsRequired();
            entity.Property(x => x.SpecialStatus).HasDefaultValue(false).IsRequired();

            entity.HasIndex(x => x.Inn)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            entity.HasIndex(x => x.GroupId);
            entity.HasIndex(x => x.RegionId);
            entity.HasIndex(x => x.DistrictId);

            entity.HasOne(x => x.Group)
                .WithMany(x => x.Patients)
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Patients_Inn_Length", "length(\"Inn\") = 14");
                t.HasCheckConstraint("CK_Patients_Inn_Digits", "\"Inn\" ~ '^[0-9]{14}$'");
                t.HasCheckConstraint("CK_Patients_Gender", "\"Gender\" IN (1, 2)");
            });
        });
        modelBuilder.Entity<MedCard>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.ConfigureTenant();
            entity.ConfigureAuditable();
            entity.ConfigureSoftDelete();

            entity.Property(x => x.CardNumber)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(x => x.OpenedAt)
                .IsRequired();

            entity.HasIndex(x => new { x.TenantId, x.CardNumber })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            entity.HasIndex(x => new { x.TenantId, x.PatientId })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            entity.HasOne(x => x.Patient)
                .WithMany(x => x.MedCards)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<MedCardClinicalInfo>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.ConfigureTenant();
            entity.ConfigureAuditable();

            entity.Property(x => x.BloodGroup).HasMaxLength(20);
            entity.Property(x => x.RhFactor).HasMaxLength(20);

            entity.HasIndex(x => x.MedCardId).IsUnique();
            entity.HasIndex(x => x.TenantId);

            entity.HasOne(x => x.MedCard)
                .WithOne(x => x.ClinicalInfo)
                .HasForeignKey<MedCardClinicalInfo>(x => x.MedCardId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MedCardDiagnosis>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.ConfigureTenant();
            entity.ConfigureAuditable();

            entity.Property(x => x.Type)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(x => x.Text)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(x => x.Code)
                .HasMaxLength(50);

            entity.HasIndex(x => new { x.MedCardId, x.Type, x.SortOrder });
            entity.HasIndex(x => x.TenantId);

            entity.HasOne(x => x.MedCard)
                .WithMany(x => x.Diagnoses)
                .HasForeignKey(x => x.MedCardId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MedCardInfectionScreening>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.ConfigureTenant();
            entity.ConfigureAuditable();

            entity.Property(x => x.Pediculosis).HasConversion<int>().IsRequired();
            entity.Property(x => x.Scabies).HasConversion<int>().IsRequired();
            entity.Property(x => x.Wasserman).HasConversion<int>().IsRequired();
            entity.Property(x => x.Fluorography).HasConversion<int>().IsRequired();
            entity.Property(x => x.AlcoholUse).HasConversion<int>().IsRequired();
            entity.Property(x => x.HepatitisB).HasConversion<int>().IsRequired();
            entity.Property(x => x.HepatitisC).HasConversion<int>().IsRequired();
            entity.Property(x => x.Hiv).HasConversion<int>().IsRequired();

            entity.HasIndex(x => x.MedCardId).IsUnique();
            entity.HasIndex(x => x.TenantId);

            entity.HasOne(x => x.MedCard)
                .WithOne(x => x.InfectionScreening)
                .HasForeignKey<MedCardInfectionScreening>(x => x.MedCardId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MedCardApproval>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.ConfigureTenant();
            entity.ConfigureAuditable();

            entity.Property(x => x.DoctorNameSnapshot).HasMaxLength(200);
            entity.Property(x => x.DirectorNameSnapshot).HasMaxLength(200);

            entity.HasIndex(x => x.MedCardId).IsUnique();
            entity.HasIndex(x => x.TenantId);

            entity.HasOne(x => x.MedCard)
                .WithOne(x => x.Approval)
                .HasForeignKey<MedCardApproval>(x => x.MedCardId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<HdSession>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.ToTable("HdSessions");

            entity.ConfigureTenant();
            entity.ConfigureAuditable();

            entity.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(x => x.IdentifiedAt).IsRequired();
            entity.Property(x => x.StatusChangedAt).IsRequired();
            entity.Property(x => x.StatusReason).HasMaxLength(500);

            entity.HasIndex(x => new { x.TenantId, x.PatientId });
            entity.HasIndex(x => new { x.TenantId, x.MedCardId });
            entity.HasIndex(x => new { x.TenantId, x.MachineId });
            entity.HasIndex(x => new { x.TenantId, x.Status });

            entity.HasOne(x => x.Patient)
                .WithMany(x => x.Sessions)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.MedCard)
                .WithMany(x => x.Sessions)
                .HasForeignKey(x => x.MedCardId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Machine)
                .WithMany(x => x.Sessions)
                .HasForeignKey(x => x.MachineId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.TenantId, x.MachineId })
                .IsUnique()
                .HasDatabaseName("UX_HdSessions_ActiveMachine")
                .HasFilter("\"MachineId\" IS NOT NULL AND \"Status\" IN (2, 3)");
        });

        modelBuilder.Entity<SessionWorkflowSettings>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.ToTable("SessionWorkflowSettings");

            entity.ConfigureTenant();
            entity.ConfigureAuditable();

            entity.HasIndex(x => x.TenantId).IsUnique();

            entity.Property(x => x.IdentificationStartLimitMinutes).IsRequired();
            entity.Property(x => x.AutoFinishActiveMinutes).IsRequired();
            entity.Property(x => x.EndIdentificationLimitMinutes).IsRequired();
            entity.Property(x => x.SendToPayLimitMinutes).IsRequired();
        });
        modelBuilder.Entity<HdSessionMeasurement>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.ToTable("HdSessionMeasurements");

            entity.ConfigureTenant();
            entity.ConfigureAuditable();

            entity.Property(x => x.Point)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(x => x.Sys).HasMaxLength(20);
            entity.Property(x => x.Dia).HasMaxLength(20);
            entity.Property(x => x.Temp).HasMaxLength(20);
            entity.Property(x => x.Ritm).HasMaxLength(20);
            entity.Property(x => x.Note).HasMaxLength(1000);

            entity.HasIndex(x => new { x.HdSessionId, x.Point }).IsUnique();
            entity.HasIndex(x => x.TenantId);

            entity.HasOne(x => x.HdSession)
                .WithMany(x => x.Measurements)
                .HasForeignKey(x => x.HdSessionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HdSessionPause>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.ToTable("HdSessionPauses");

            entity.ConfigureTenant();
            entity.ConfigureAuditable();

            entity.Property(x => x.StartedAt).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(1000);

            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.HdSessionId);

            entity.HasIndex(x => x.HdSessionId)
                .IsUnique()
                .HasFilter("\"EndedAt\" IS NULL");

            entity.HasOne(x => x.HdSession)
                .WithMany(x => x.Pauses)
                .HasForeignKey(x => x.HdSessionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<MedCenterMachine>(entity =>
{
    entity.HasKey(x => x.Id);
    entity.ToTable("MedCenterMachines");

    entity.ConfigureTenant();
    entity.ConfigureAuditable();
    entity.ConfigureSoftDelete();
    entity.ConfigureActive();

    entity.Property(x => x.AcquisitionType)
        .HasConversion<int>()
        .IsRequired();

    entity.Property(x => x.InventoryNumber).HasMaxLength(50).IsRequired();
    entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
    entity.Property(x => x.Model).HasMaxLength(100).IsRequired();
    entity.Property(x => x.SerialNumber).HasMaxLength(100).IsRequired();

    entity.Property(x => x.Manufacturer).HasMaxLength(200).IsRequired();
    entity.Property(x => x.ManufacturingCountry).HasMaxLength(100);
    entity.Property(x => x.ManufactureYear).IsRequired();

    entity.Property(x => x.CertificateHolder).HasMaxLength(200);
    entity.Property(x => x.CertificateHolderCountry).HasMaxLength(100);
    entity.Property(x => x.CertificateNumber).HasMaxLength(100);
    entity.Property(x => x.CertificateCountry).HasMaxLength(100);
    entity.Property(x => x.CertificateIssuedAt).IsRequired();

    entity.Property(x => x.PermitName).HasMaxLength(200);
    entity.Property(x => x.PermitNumber).HasMaxLength(100);
    entity.Property(x => x.PermitSeries).HasMaxLength(100);
    entity.Property(x => x.PermitExpiresAt).IsRequired();

    entity.Property(x => x.DailySessionLimit).IsRequired();
    entity.Property(x => x.BetweenSessionCooldownMinutes).IsRequired();
    entity.Property(x => x.DailyLimitCooldownMinutes).IsRequired();
    entity.Property(x => x.IsApproved).IsRequired();

    entity.HasIndex(x => new { x.TenantId, x.InventoryNumber })
        .IsUnique()
        .HasFilter("\"IsDeleted\" = false");

    entity.HasIndex(x => x.SerialNumber)
        .IsUnique()
        .HasFilter("\"IsDeleted\" = false");

    entity.HasIndex(x => new { x.TenantId, x.IsActive });
    entity.HasIndex(x => new { x.TenantId, x.IsApproved });
    entity.HasIndex(x => x.PermitExpiresAt);
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