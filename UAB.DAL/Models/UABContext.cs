using System;
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
        }

        public virtual DbSet<Chart> Chart { get; set; }
        public virtual DbSet<ChartCptCode> ChartCptCode { get; set; }
        public virtual DbSet<ChartDxCode> ChartDxCode { get; set; }
        public virtual DbSet<ChartQueue> ChartQueue { get; set; }
        public virtual DbSet<ChartVersion> ChartVersion { get; set; }
        public virtual DbSet<Client> Client { get; set; }
        public virtual DbSet<ClinicalCase> ClinicalCase { get; set; }
        public virtual DbSet<CoderQuestion> CoderQuestion { get; set; }
        public virtual DbSet<CustomField> CustomField { get; set; }
        public virtual DbSet<List> List { get; set; }
        public virtual DbSet<Project> Project { get; set; }
        public virtual DbSet<Provider> Provider { get; set; }
        public virtual DbSet<Payor> Payor { get; set; }
        public virtual DbSet<ProviderFeedback> ProviderFeedback { get; set; }
        public virtual DbSet<Status> Status { get; set; }
        public virtual DbSet<WorkItem> WorkItem { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=VD-TSTPC10P-DB;Database=UAB;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chart>(entity =>
            {
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

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.Chart)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FKChart282609");

                entity.HasOne(d => d.ProviderFeedback)
                    .WithMany(p => p.Chart)
                    .HasForeignKey(d => d.ProviderFeedbackId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FKChart202355");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Chart)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FKChart129961");
            });

            modelBuilder.Entity<ChartCptCode>(entity =>
            {
                entity.HasKey(e => e.ChartDataId)
                    .HasName("PK__ChartCpt__21E9F5BBAE9F7595");

                entity.Property(e => e.Cptcode)
                    .IsRequired()
                    .HasColumnName("CPTCode")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Modifier)
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.HasOne(d => d.ClinicalCase)
                    .WithMany(p => p.ChartCptCode)
                    .HasForeignKey(d => d.ClinicalCaseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FKChartCptCo152070");
            });

            modelBuilder.Entity<ChartDxCode>(entity =>
            {
                entity.Property(e => e.DxCode)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.HasOne(d => d.ClinicalCase)
                    .WithMany(p => p.ChartDxCode)
                    .HasForeignKey(d => d.ClinicalCaseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FKChartDxCod9138");
            });

            modelBuilder.Entity<ChartQueue>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ChartVersion>(entity =>
            {
                entity.Property(e => e.VersionDate).HasColumnType("date");

                entity.HasOne(d => d.ChartQueue)
                    .WithMany(p => p.ChartVersion)
                    .HasForeignKey(d => d.ChartQueueId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FKChartVersi409235");

                entity.HasOne(d => d.ClinicalCase)
                    .WithMany(p => p.ChartVersion)
                    .HasForeignKey(d => d.ClinicalCaseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FKChartVersi509041");
            });

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
                    .OnDelete(DeleteBehavior.ClientSetNull)
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

            modelBuilder.Entity<List>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255)
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

            modelBuilder.Entity<Provider>(entity =>
            {
                entity.Property(e => e.ProviderId).HasColumnName("ProviderID");

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
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

            modelBuilder.Entity<WorkItem>(entity =>
            {
                entity.Property(e => e.AssignedDate).HasColumnType("date");

                entity.HasOne(d => d.ClinicalCase)
                    .WithMany(p => p.WorkItem)
                    .HasForeignKey(d => d.ClinicalCaseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FKWorkItem32013");

                entity.HasOne(d => d.ProviderFeedback)
                    .WithMany(p => p.WorkItem)
                    .HasForeignKey(d => d.ProviderFeedbackId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FKWorkItem106667");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.WorkItem)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FKWorkItem225649");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
