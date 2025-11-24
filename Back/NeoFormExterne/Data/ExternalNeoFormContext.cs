using NeoForm_Externe.Models;
using Microsoft.EntityFrameworkCore;
using static NeoForm_Externe.Models.Dto.UserAuthenticationDto;
using NeoForm_Externe.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace NeoForm_Externe.Data
{
    public class ExternalNeoFormContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IEncryptionService? _encryptionService;

        public ExternalNeoFormContext(DbContextOptions<ExternalNeoFormContext> options)
            : base(options)
        {
            Objects = Set<ObjectModels>();
            Clients = Set<ClientInfo>();
            UserAuthentications = Set<UserAuthentication>();
        }

        public ExternalNeoFormContext(DbContextOptions<ExternalNeoFormContext> options, IEncryptionService encryptionService)
            : base(options)
        {
            _encryptionService = encryptionService;
            Objects = Set<ObjectModels>();
            Clients = Set<ClientInfo>();
            UserAuthentications = Set<UserAuthentication>();
        }

        public DbSet<ObjectModels> Objects { get; set; }
        public DbSet<ClientInfo> Clients { get; set; } // ✅ Add ClientInfo DbSet
        public DbSet<UserAuthentication> UserAuthentications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Important: Call base for Identity

            // Configure Identity tables to use AppNeoFormExt schema
            modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers", "AppNeoFormExt");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().ToTable("AspNetRoles", "AppNeoFormExt");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("AspNetUserRoles", "AppNeoFormExt");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>().ToTable("AspNetUserClaims", "AppNeoFormExt");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>().ToTable("AspNetUserLogins", "AppNeoFormExt");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>().ToTable("AspNetUserTokens", "AppNeoFormExt");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims", "AppNeoFormExt");

            // Object table configuration
            modelBuilder.Entity<ObjectModels>(entity =>
            {
                entity.ToTable("Object", "AppNeoFormExt");

                entity.HasKey(e => e.Id).HasName("PK_Object");

                entity.Property(e => e.Id).HasColumnName("_id");

                entity.Property(e => e.Application)
                      .HasMaxLength(100)
                      .HasComputedColumnSql("(left(json_value([ObjectJson],'$.application'),(100)))", false)
                      .HasColumnName("application");

                entity.Property(e => e.Guid)
                      .HasMaxLength(100)
                      .HasComputedColumnSql("(left(json_value([ObjectJson],'$.guid'),(100)))", false)
                      .HasColumnName("guid");

                entity.Property(e => e.ObjectName)
                      .HasMaxLength(100)
                      .HasComputedColumnSql("(left(json_value([ObjectJson],'$.objectName'),(100)))", false)
                      .HasColumnName("objectName");

                entity.Property(e => e.ObjectType)
                      .HasMaxLength(100)
                      .HasComputedColumnSql("(left(json_value([ObjectJson],'$.objectType'),(100)))", false)
                      .HasColumnName("objectType");

                entity.Property(e => e.IsEncrypted)
                      .HasComputedColumnSql("CASE WHEN JSON_VALUE([ObjectJson], '$.isEncrypted') = 'true' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END", false)
                      .HasColumnName("isEncrypted");
            });


            modelBuilder.Entity<ClientInfo>(entity =>
            {
                entity.ToTable("Clients", "AppNeoFormExt");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.ClientId)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.BaseUrl)
                      .IsRequired()
                      .HasMaxLength(300);

                // Automatically encrypt/decrypt ApiKey when saving/reading from database
                if (_encryptionService != null)
                {
                    entity.Property(e => e.ApiKey)
                          .HasConversion(new EncryptedStringConverter(_encryptionService))
                          .HasMaxLength(500); // Encrypted values are longer
                }
                else
                {
                    entity.Property(e => e.ApiKey)
                          .HasMaxLength(500);
                }
            });

            modelBuilder.Entity<UserAuthentication>(entity =>
            {
                entity.ToTable("UserAuthentications", "AppNeoFormExt");
                entity.HasIndex(e => e.Guid);
                entity.HasIndex(e => new { e.Guid, e.IsActive });
            });
        }
    }
}
