using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Web_Form.Models;

namespace Web_Form.Data
{
    public class DbContext : IdentityDbContext<ApplicationUser>
    {
        public DbContext(DbContextOptions<DbContext> options) : base(options)
        {

        }
        public virtual DbSet<TblForm> TblForms { get; set; }

        public virtual DbSet<TblKeywordMaster> TblKeywordMasters { get; set; }

        public virtual DbSet<TblQuestion> TblQuestions { get; set; }

        public virtual DbSet<TblQuestionOption> TblQuestionOptions { get; set; }
        public virtual DbSet<TblResponse> TblResponses { get; set; }
        public virtual DbSet<TblComment> TblComments { get; set; }
        public virtual DbSet<TblLike> TblLikes { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=MehediForm;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers");
            modelBuilder.Entity<IdentityRole>().ToTable("AspNetRoles");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens");
            modelBuilder.Entity<TblKeywordMaster>(entity =>
            {
                entity.HasKey(e => e.KeywordId).HasName("PK_tbl_QuestionMAster");

                entity.ToTable("tbl_KeywordMaster");

                entity.Property(e => e.KeywordId).HasColumnName("KeywordID");
                entity.Property(e => e.KeywordName).IsUnicode(false);
                entity.Property(e => e.KeywordType).IsUnicode(false);
                entity.Property(e => e.Status).HasDefaultValue(true);
            });
           

            modelBuilder.Entity<TblForm>(entity =>
            {
                entity.HasKey(e => e.FormId);

                entity.ToTable("tbl_Forms");

                entity.Property(e => e.FormId).HasColumnName("FormID");
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
                entity.Property(e => e.Likes).HasDefaultValue(0);
                entity.Property(e => e.Title)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValue("Untitled form");
                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });
            modelBuilder.Entity<TblLike>(entity =>
            {
                entity.ToTable("tbl_Likes");

                entity.HasOne(d => d.Form).WithMany(p => p.TblLikes)
                    .HasForeignKey(d => d.FormId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_tbl_Likes_tbl_Forms");
                
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
                entity.Property(e => e.FormId).HasColumnName("FormID");
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

            modelBuilder.Entity<TblResponse>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("tbl_Response");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.SubmissionDate).HasColumnType("datetime");
                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.HasOne(d => d.Form).WithMany()
                    .HasForeignKey(d => d.FormId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_tbl_Response_tbl_Forms");

                entity.HasOne(d => d.Option).WithMany()
                    .HasForeignKey(d => d.OptionId)
                    .HasConstraintName("FK_tbl_Response_tbl_QuestionOption");

                entity.HasOne(d => d.Question).WithMany()
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_tbl_Response_tbl_Questions");


            });
            modelBuilder.Entity<TblComment>(entity =>
{
    entity.HasKey(e => e.Id); // Define 'Id' as the primary key

    entity.ToTable("tbl_Comment");

    entity.Property(e => e.Id)
        .ValueGeneratedOnAdd(); // Auto-generate the primary key value

    entity.Property(e => e.UserId)
        .HasMaxLength(450);

    entity.HasOne(d => d.Form)
        .WithMany(f => f.TblComments)
        .HasForeignKey(d => d.FormId)
        .OnDelete(DeleteBehavior.ClientSetNull)
        .HasConstraintName("FK_tbl_Comment_tbl_Forms");
});

        }
    }
}

