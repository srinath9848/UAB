using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace UAB.DAL.LoginDTO
{
    public partial class IdentityServerContext : DbContext
    {
        public IdentityServerContext()
        {
        }

        public IdentityServerContext(DbContextOptions<IdentityServerContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Attempts> Attempts { get; set; }
        public virtual DbSet<EmailVerifications> EmailVerifications { get; set; }
        public virtual DbSet<HistoricPasswords> HistoricPasswords { get; set; }
        public virtual DbSet<MigrationHistory> MigrationHistory { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=VD-TSTPC10P-DB;Database=IdentityServer;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Attempts>(entity =>
            {
                entity.Property(e => e.AttemptsId).HasColumnName("AttemptsID");

                entity.Property(e => e.Action)
                    .HasMaxLength(8000)
                    .IsUnicode(false);

                entity.Property(e => e.Email)
                    .HasMaxLength(8000)
                    .IsUnicode(false);

                entity.Property(e => e.Result)
                    .HasMaxLength(8000)
                    .IsUnicode(false);

                entity.Property(e => e.UserCode)
                    .HasMaxLength(8000)
                    .IsUnicode(false);

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Attempts)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Attempts_Users");
            });

            modelBuilder.Entity<EmailVerifications>(entity =>
            {
                entity.Property(e => e.EmailVerificationsId).HasColumnName("EmailVerificationsID");

                entity.Property(e => e.Code)
                    .HasMaxLength(8000)
                    .IsUnicode(false);

                entity.Property(e => e.UserCode)
                    .HasMaxLength(8000)
                    .IsUnicode(false);

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.EmailVerifications)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_EmailVerifications_Users");
            });

            modelBuilder.Entity<HistoricPasswords>(entity =>
            {
                entity.Property(e => e.HistoricPasswordsId).HasColumnName("HistoricPasswordsID");

                entity.Property(e => e.Data)
                    .HasMaxLength(8000)
                    .IsUnicode(false);

                entity.Property(e => e.UserCode)
                    .HasMaxLength(8000)
                    .IsUnicode(false);

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.HistoricPasswords)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_HistoricPasswords_Users");
            });

            modelBuilder.Entity<MigrationHistory>(entity =>
            {
                entity.HasKey(e => new { e.MigrationId, e.ContextKey })
                    .HasName("PK_dbo.__MigrationHistory");

                entity.ToTable("__MigrationHistory");

                entity.Property(e => e.MigrationId).HasMaxLength(150);

                entity.Property(e => e.ContextKey).HasMaxLength(300);

                entity.Property(e => e.Model).IsRequired();

                entity.Property(e => e.ProductVersion)
                    .IsRequired()
                    .HasMaxLength(32);
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.Property(e => e.UsersId).HasColumnName("UsersID");

                entity.Property(e => e.Code)
                    .HasMaxLength(8000)
                    .IsUnicode(false);

                entity.Property(e => e.Email)
                    .HasMaxLength(8000)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName)
                    .HasMaxLength(8000)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .HasMaxLength(8000)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .HasMaxLength(8000)
                    .IsUnicode(false);

                entity.Property(e => e.Phone)
                    .HasMaxLength(8000)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
