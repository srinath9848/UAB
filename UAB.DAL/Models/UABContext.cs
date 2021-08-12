using System;
using System.Data.Entity.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace UAB.DAL.Models
{
    public partial class UABContext : DbContext
    {
        public UABContext()
        {
        }

        public UABContext(DbContextOptions<UABContext> options)
            : base(options)
        {
            var objectContext = (this as IObjectContextAdapter).ObjectContext;

            // Sets the command timeout for all the commands
            objectContext.CommandTimeout = 300;
        }

        public virtual DbSet<Client> Client { get; set; }
        public virtual DbSet<ProjectType> ProjectType { get; set; }
        public virtual DbSet<ClinicalCase> ClinicalCase { get; set; }
        public virtual DbSet<CoderQuestion> CoderQuestion { get; set; }
        public virtual DbSet<CptCode> CptCode { get; set; }
        public virtual DbSet<CustomField> CustomField { get; set; }
        public virtual DbSet<DxCode> DxCode { get; set; }
        public virtual DbSet<ErrorType> ErrorType { get; set; }
        public virtual DbSet<List> List { get; set; }
        public virtual DbSet<Payor> Payor { get; set; }
        public virtual DbSet<Project> Project { get; set; }
        public virtual DbSet<ProjectUser> ProjectUser { get; set; }
        public virtual DbSet<Provider> Provider { get; set; }
        public virtual DbSet<ProviderFeedback> ProviderFeedback { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<Status> Status { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<Version> Version { get; set; }
        public virtual DbSet<WorkItem> WorkItem { get; set; }
        public virtual DbSet<WorkItemAudit> WorkItemAudit { get; set; }
        public virtual DbSet<WorkItemProvider> WorkItemProvider { get; set; }
        public virtual DbSet<BlockCategory> BlockCategory { get; set; }
        public virtual DbSet<BlockHistory> BlockHistory { get; set; }
        public virtual DbSet<EMCodeLevel> EMCodeLevel { get; set; }
        public virtual DbSet<BlockResponse> BlockResponse { get; set; }
        public virtual DbSet<Location> Location  { get; set; }
        public virtual DbSet<CptAudit> CptAudit   { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                AppConfiguration app = new AppConfiguration();
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer(app.ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>(entity =>
            {
                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("('1')");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ClinicalCase>(entity =>
            {
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.DateOfService).HasColumnType("date");

                entity.Property(e => e.EncounterNumber)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.PatientFirstName)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.PatientLastName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.PatientMrn)
                    .IsRequired()
                    .HasColumnName("PatientMRN")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.List)
                    .WithMany(p => p.ClinicalCase)
                    .HasForeignKey(d => d.ListId)
                    .HasConstraintName("FKClinicalCa234939");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ClinicalCase)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FKClinicalCa331303");
            });

            modelBuilder.Entity<CoderQuestion>(entity =>
            {
                entity.Property(e => e.Answer)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.AnsweredDate).HasColumnType("date");

                entity.Property(e => e.Question)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.QuestionDate).HasColumnType("date");

                entity.HasOne(d => d.ClinicalCase)
                    .WithMany(p => p.CoderQuestion)
                    .HasForeignKey(d => d.ClinicalCaseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FKCoderQuest192420");
            });

            modelBuilder.Entity<CptCode>(entity =>
            {
                entity.Property(e => e.Cptcode1)
                    .IsRequired()
                    .HasColumnName("CPTCode")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Modifier)
                    .HasMaxLength(25)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CustomField>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.ClinicalCase)
                    .WithMany(p => p.CustomField)
                    .HasForeignKey(d => d.ClinicalCaseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FKCustomFiel283120");
            });

            modelBuilder.Entity<DxCode>(entity =>
            {
                entity.Property(e => e.DxCode1)
                    .IsRequired()
                    .HasColumnName("DxCode")
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ErrorType>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<List>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Payor>(entity =>
            {
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("('1')");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.Project)
                    .HasForeignKey(d => d.ClientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FKProject603379");
            });

            modelBuilder.Entity<ProjectUser>(entity =>
            {
                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("('1')");
            });

            modelBuilder.Entity<Provider>(entity =>
            {
                entity.Property(e => e.ProviderId).HasColumnName("ProviderID");

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ProviderFeedback>(entity =>
            {
                entity.Property(e => e.Feedback)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Status>(entity =>
            {
                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("('1')");
            });

            modelBuilder.Entity<WorkItem>(entity =>
            {
                entity.Property(e => e.AssignedDate).HasColumnType("datetime");

                entity.Property(e => e.NoteTitle)
                    .HasMaxLength(200)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<WorkItemAudit>(entity =>
            {
                entity.Property(e => e.FieldName)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.FieldValue)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.Remark)
                    .IsRequired()
                    .HasMaxLength(2000)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
