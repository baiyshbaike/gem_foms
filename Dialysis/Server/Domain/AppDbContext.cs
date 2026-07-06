using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Dialysis.Shared.Models;
using System.Reflection.Metadata;
using System.Xml.Linq;
using System.Reflection.Emit;
using Dialysis.Shared.Dto;
using Dialysis.Shared.Models.Files;
using Dialysis.Shared.Models.Tunduk;

namespace Dialysis.Server.Domain;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

 
    public DbSet<User> User { get; set; }
    public DbSet<SaveFile> SaveFile { get; set; }
    public DbSet<Complaint> Complaint { get; set; }
    public DbSet<MedicineType> MedicineType { get; set; }
    public DbSet<ProtocolFile> ProtocolFile { get; set; }
    public DbSet<Role> Role { get; set; }
    public DbSet<UserRole> UserRole { get; set; }
    public DbSet<UserProfile> UserProfile { get; set; }
    public DbSet<UserProfileRole> UserProfileRole { get; set; }
    public DbSet<Analysis> Analysise { get; set; }
    public DbSet<CodeMKB> CodeMKBs { get; set; }
    public DbSet<DialyzerType> DialyzerType { get; set; }
    public DbSet<AnalysisResult> AnalysisResult { get; set; }
    public DbSet<AnalysisResultGroup> AnalysisResultGroup { get; set; }
    public DbSet<Epicrisis> Epicrisis { get; set; }
    public DbSet<FirstAnalysis> FirstAnalysis { get; set; }
    public DbSet<FirstAnalysisResult> FirstAnalysisResult { get; set; }
    public DbSet<FirstCardiovascular> FirstCardiovascular { get; set; }
    public DbSet<FirstConfectionery> FirstConfectionery { get; set; }
    public DbSet<FirstEndocrine> FirstEndocrine { get; set; }
    public DbSet<FirstInspection> FirstInspection { get; set; }
    public DbSet<FirstNeuro> FirstNeuro { get; set; }
    public DbSet<FirstRespiratory> FirstRespiratory { get; set; }
    public DbSet<FirstUrogenital> FirstUrogenital { get; set; }
    public DbSet<HDSession> HDSession { get; set; }
    public DbSet<HDSessionHour> HDSessionHour { get; set; }
    public DbSet<HDSessionPause> HDSessionPause { get; set; }
    public DbSet<MedCard> MedCard { get; set; }
    public DbSet<Patient> Patient { get; set; }
    public DbSet<PatientGroup> PatientGroup { get; set; }
    public DbSet<PatientGroupPerson> PatientGroupAct { get; set; }
    public DbSet<MedCenter> MedCenter { get; set; }
    public DbSet<MedCenterMachine> MedCenterMachine { get; set; }
    public DbSet<MedCenterUser> MedCenterUser { get; set; }
    public DbSet<District> District { get; set; }
    public DbSet<Region> Region { get; set; }
    public DbSet<Status> Status { get; set; }
    public DbSet<ActivePrice> ActivePrice { get; set; }
    public DbSet<GlobalStatus> GlobalStatus { get; set; }
    public DbSet<PatientStatus> PatientStatus { get; set; }
    public DbSet<PatientGroupTitle> PatientGroupTitle { get; set; }
    public DbSet<MedCenterFiles> MedCenterFiles { get; set; }

    public DbSet<GroupLPU> GroupLPU { get; set; }
    public DbSet<GroupPersonTitle> GroupPersonTitle { get; set; }
    public DbSet<GroupReason> GroupReason { get; set; }
    public DbSet<PatientHistory> PatientHistory { get; set; }

    public DbSet<Indicator> Indicator { get; set; }
    public DbSet<IndicatorReference> IndicatorReference { get; set; }
    public DbSet<IndicatorRow> IndicatorRow { get; set; }
    public DbSet<WritingOut> WritingOut { get; set; }

    public DbSet<QualityExam1> QualityExam1 { get; set; }
    public DbSet<QualityExam2> QualityExam2 { get; set; }
    public DbSet<QualityExam2Patient> QualityExam2Patient { get; set; }
    public DbSet<QualityExam3> QualityExam3 { get; set; }
    public DbSet<QualityExam3Row> QualityExam3Row { get; set; }
    public DbSet<MedCenterEmployee> MedCenterEmployee { get; set; }
    public DbSet<MedCenterPatientFile> MedCenterPatientFiles { get; set; }
    public DbSet<IdentifyTunduk> IdentifyTunduks { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<HttpLog> HttpLogs { get; set; }
    public DbSet<ActionLog> ActionLogs { get; set; }
    public DbSet<LogSettings> LogSettings { get; set; }

    protected void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<CodeMKB>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<MedCenterEmployee>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<ProtocolFile>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });
        builder.Entity<Complaint>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<SaveFile>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });
        builder.Entity<DialyzerType>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<MedicineType>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<User>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        builder.Entity<Role>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<UserRole>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();           
        });
        builder.Entity<UserProfile>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<UserProfileRole>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

        });

        builder.Entity<Analysis>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<AnalysisResult>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<AnalysisResultGroup>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        }); 
        builder.Entity<Epicrisis>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<FirstAnalysis>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<FirstAnalysisResult>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<FirstCardiovascular>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<FirstConfectionery>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<FirstEndocrine>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<FirstInspection>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<FirstNeuro>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<FirstRespiratory>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<FirstUrogenital>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<HDSession>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<HDSessionHour>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();           
        });
        builder.Entity<HDSessionPause>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();            
        });
        builder.Entity<MedCard>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        builder.Entity<MedCenter>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<MedCenterMachine>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<MedCenterUser>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });


        builder.Entity<Patient>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.SpecialStatus).HasDefaultValue(false);
        });
        builder.Entity<PatientGroup>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            
        });
        builder.Entity<PatientGroupPerson>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        builder.Entity<ActivePrice>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<GlobalStatus>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<Status>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        builder.Entity<PatientStatus>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });      

        builder.Entity<District>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
       
        builder.Entity<Region>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        builder.Entity<PatientGroupTitle>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();            
        });
        
        builder.Entity<MedCenterFiles>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();            
        });

        builder.Entity<PatientSessionsRepDto>().HasNoKey();

        builder.Entity<GroupLPU>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });
        builder.Entity<GroupPersonTitle>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });
        builder.Entity<GroupReason>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        builder.Entity<PatientHistory>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

        });

        builder.Entity<Indicator>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

        });
        builder.Entity<IndicatorReference>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

        });
        builder.Entity<IndicatorRow>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

        });

        builder.Entity<WritingOut>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

        });

        builder.Entity<QualityExam1>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });
        builder.Entity<QualityExam2>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });
        builder.Entity<QualityExam2Patient>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });
        builder.Entity<QualityExam3>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });
        builder.Entity<QualityExam3Row>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        builder.Entity<MedCenterPatientFile>()
            .HasOne(pf => pf.Patient)
            .WithMany()
            .HasForeignKey(pf => pf.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<MedCenterPatientFile>()
            .HasOne(pf => pf.UploadedByUser)
            .WithMany()
            .HasForeignKey(pf => pf.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MedCenterPatientFile>()
            .HasOne(pf => pf.MedCenter)
            .WithMany()
            .HasForeignKey(pf => pf.MedCenterId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<MedCenterPatientFile>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });
        builder.Entity<IdentifyTunduk>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });
        builder.Entity<UserSession>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Jti).IsRequired();
            entity.HasIndex(e => e.Jti);
            entity.HasIndex(e => new { e.UserId, e.IsActive });
        });
    }
}


