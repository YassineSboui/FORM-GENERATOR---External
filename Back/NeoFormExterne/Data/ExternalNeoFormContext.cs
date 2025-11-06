using NeoForm_Externe.Models;
using Microsoft.EntityFrameworkCore;
using static NeoForm_Externe.Models.Dto.UserAuthenticationDto;
using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Data.Converters;

namespace NeoForm_Externe.Data
{
    public class ExternalNeoFormContext : DbContext
    {
        private readonly IEncryptionService? _encryptionService;

        public ExternalNeoFormContext(DbContextOptions<ExternalNeoFormContext> options)
            : base(options) { }

        public ExternalNeoFormContext(DbContextOptions<ExternalNeoFormContext> options, IEncryptionService encryptionService)
            : base(options)
        {
            _encryptionService = encryptionService;
        }

        public DbSet<ObjectModels> Objects { get; set; }
        public DbSet<ClientInfo> Clients { get; set; } // ✅ Add ClientInfo DbSet
        public DbSet<UserAuthentication> UserAuthentications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Object table configuration
            modelBuilder.Entity<ObjectModels>(entity =>
            {
                entity.ToTable("Object", "AppNeoForm");

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
                entity.ToTable("Clients", "AppNeoForm");

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
                entity.HasIndex(e => e.Guid);
                entity.HasIndex(e => new { e.Guid, e.IsActive });
            });
        }
    }
}
