using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Web_Form.Models;

namespace Web_Form.Data
{
    public class DataContext : IdentityDbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public virtual DbSet<TblForm> TblForms { get; set; }
        public virtual DbSet<TblKeywordMaster> TblKeywordMasters { get; set; }
        public virtual DbSet<TblQuestion> TblQuestions { get; set; }
        public virtual DbSet<TblQuestionOption> TblQuestionOptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<TblForm>(entity =>
            {
                entity.HasKey(e => e.FormId);

                entity.ToTable("tbl_Forms");

                entity.Property(e => e.FormId)
                    .IsRequired()
                    .HasDefaultValueSql("NEWID()") // SQL function to generate a new GUID
                    .HasColumnName("FormID");
                entity.Property(e => e.BackgroundColor)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasDefaultValue("#ffffff");
                entity.Property(e => e.CreatedOn)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Createdby).HasMaxLength(50);
                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.IsFavourite).HasDefaultValue(false);
                entity.Property(e => e.LastOpened)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.Status).HasDefaultValue(true);
                entity.Property(e => e.Title)
                    .HasMaxLength(50)
                    .HasDefaultValue("Untitled form");
                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<TblKeywordMaster>(entity =>
            {
                entity.HasKey(e => e.KeywordId).HasName("PK_tbl_QuestionMAster");

                entity.ToTable("tbl_KeywordMaster");

                entity.Property(e => e.KeywordId).HasColumnName("KeywordID");
                entity.Property(e => e.KeywordName).IsUnicode(false);
                entity.Property(e => e.KeywordType).IsUnicode(false);
                entity.Property(e => e.Status).HasDefaultValue(true);
            });

            modelBuilder.Entity<TblQuestion>(entity =>
            {
                entity.HasKey(e => e.QuestionId);

                entity.ToTable("tbl_Questions");

                entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
                entity.Property(e => e.CreatedOn)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Createdby).HasMaxLength(50);
                entity.Property(e => e.FormId)
                    .HasColumnName("FormID");
                entity.Property(e => e.IsRequired).HasDefaultValue(false);
                entity.Property(e => e.IsSuffled).HasDefaultValue(false);
                entity.Property(e => e.Serial).HasDefaultValue(1);
                entity.Property(e => e.Status).HasDefaultValue(true);
                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");

                entity.HasOne(d => d.Form).WithMany(p => p.TblQuestions)
                    .HasForeignKey(d => d.FormId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_tbl_Questions_tbl_Forms");

                entity.HasOne(d => d.QuestionTypeNavigation).WithMany(p => p.TblQuestions)
                    .HasForeignKey(d => d.QuestionType)
                    .HasConstraintName("FK_tbl_Questions_tbl_KeywordMaster");
            });

            modelBuilder.Entity<TblQuestionOption>(entity =>
            {
                entity.HasKey(e => e.OptionId);

                entity.ToTable("tbl_QuestionOption");

                entity.Property(e => e.OptionId).HasColumnName("OptionID");
                entity.Property(e => e.OptionText).HasMaxLength(50);
                entity.Property(e => e.QuestionId).HasColumnName("QuestionID");

                entity.HasOne(d => d.Question).WithMany(p => p.TblQuestionOptions)
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_tbl_QuestionOption_tbl_QuestionOption");
            });
        }
    }
}
