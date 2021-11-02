using Microsoft.EntityFrameworkCore;

using System.Diagnostics.CodeAnalysis;

namespace OKRFeedbackService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class FeedbackDbContext : DataContext
    {
        public FeedbackDbContext()
        {
        }

        public FeedbackDbContext(DbContextOptions<FeedbackDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Comment> Comment { get; set; }
        public virtual DbSet<ErrorLog> ErrorLog { get; set; }
        public virtual DbSet<FeedbackDetail> FeedbackDetail { get; set; }
        public virtual DbSet<FeedbackOnTypeMaster> FeedbackOnTypeMaster { get; set; }
        public virtual DbSet<FeedbackRequest> FeedbackRequest { get; set; }
        public virtual DbSet<OneToOneDetail> OneToOneDetail { get; set; }
        public virtual DbSet<RaisedTypeMaster> RaisedTypeMaster { get; set; }
        public virtual DbSet<RequestMaster> RequestMaster { get; set; }
        public virtual DbSet<StatusMaster> StatusMaster { get; set; }
        public virtual DbSet<CriteriaFeedbackMapping> CriteriaFeedbackMapping { get; set; }
        public virtual DbSet<CriteriaMaster> CriteriaMaster { get; set; }
        public virtual DbSet<CriteriaTypeMaster> CriteriaTypeMaster { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=52.21.77.184;Database=Feedback_QA;User Id=okr-admin;Password=abcd@1234;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.CommentId)
                    .HasName("PK__Comment__C3B4DFCAD28C3BDB");

                entity.Property(e => e.Comments)
                    .IsRequired()
                    .HasColumnType("text");

                entity.Property(e => e.CreatedOn)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");

                entity.HasOne(d => d.FeedbackDetail)
                    .WithMany(p => p.Comment)
                    .HasForeignKey(d => d.FeedbackDetailId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Comment__Feedbac__04E4BC85");
            });

            modelBuilder.Entity<ErrorLog>(entity =>
            {
                entity.HasKey(e => e.LogId)
                    .HasName("PK__ErrorLog__5E548648BB010C2F");

                entity.Property(e => e.CreatedOn)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ErrorDetail).IsRequired();

                entity.Property(e => e.FunctionName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.PageName)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<FeedbackDetail>(entity =>
            {
                entity.HasKey(e => e.FeedbackDetailId)
                    .HasName("PK__Feedback__81AD9A7A7355E801");

                entity.Property(e => e.CreatedOn)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsOneToOneRequested)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.SharedRemark).HasColumnType("text");

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");

                entity.HasOne(d => d.FeedbackOnType)
                    .WithMany(p => p.FeedbackDetail)
                    .HasForeignKey(d => d.FeedbackOnTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__FeedbackD__Feedb__06CD04F7");

                entity.HasOne(d => d.FeedbackRequest)
                    .WithMany(p => p.FeedbackDetail)
                    .HasForeignKey(d => d.FeedbackRequestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__FeedbackD__Feedb__05D8E0BE");

                entity.Property(e => e.CriteriaTypeId)
                  .HasColumnType("int")
                  .HasDefaultValueSql("((0))");
            });

            modelBuilder.Entity<FeedbackOnTypeMaster>(entity =>
            {
                entity.HasKey(e => e.FeedbackOnTypeId)
                    .HasName("PK__Feedback__84E6FCBA34C91475");

                entity.Property(e => e.FeedbackOnTypeId).ValueGeneratedNever();

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<FeedbackRequest>(entity =>
            {
                entity.HasKey(e => e.FeedbackRequestId)
                    .HasName("PK__Feedback__DC8AF114EF12673F");

                entity.Property(e => e.CreatedOn)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.RequestRemark).HasColumnType("text");

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");

                entity.HasOne(d => d.FeedbackOnType)
                    .WithMany(p => p.FeedbackRequest)
                    .HasForeignKey(d => d.FeedbackOnTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__FeedbackR__Feedb__07C12930");

                entity.HasOne(d => d.RaisedType)
                    .WithMany(p => p.FeedbackRequest)
                    .HasForeignKey(d => d.RaisedTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__FeedbackR__Raise__08B54D69");

                entity.HasOne(d => d.StatusNavigation)
                    .WithMany(p => p.FeedbackRequest)
                    .HasForeignKey(d => d.Status)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__FeedbackR__Statu__09A971A2");
            });

            modelBuilder.Entity<OneToOneDetail>(entity =>
            {
                entity.HasKey(e => e.OneToOneDetailId)
                   .HasName("PK__OneToOne__88C2BEDF144DC924");

                entity.Property(e => e.CreatedOn)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.OneToOneRemark)
                    .IsRequired()
                    .HasColumnType("text");

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");

                entity.HasOne(d => d.RequestTypeNavigation)
                    .WithMany(p => p.OneToOneDetail)
                    .HasForeignKey(d => d.RequestType)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__OneToOneD__Reque__151B244E");

                entity.Property(e => e.Status)
                   .HasColumnType("int")
                   .HasDefaultValueSql("((2))");
            });

            modelBuilder.Entity<RaisedTypeMaster>(entity =>
            {
                entity.HasKey(e => e.RaisedTypeId)
                    .HasName("PK__RaisedTy__75AB84034AD22934");

                entity.Property(e => e.RaisedTypeId).ValueGeneratedNever();

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<RequestMaster>(entity =>
            {
                entity.HasKey(e => e.RequestId)
                    .HasName("requestMaster_pk");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.RequestName).HasMaxLength(250);
            });

            modelBuilder.Entity<CriteriaFeedbackMapping>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Score).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");

                entity.HasOne(d => d.CriteriaMaster)
                    .WithMany(p => p.CriteriaFeedbackMapping)
                    .HasForeignKey(d => d.CriteriaMasterId)
                     .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__CriteriaF__Crite__5070F446");

                entity.HasOne(d => d.FeedbackDetail)
                    .WithMany(p => p.CriteriaFeedbackMapping)
                    .HasForeignKey(d => d.FeedbackDetailId)
                     .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__CriteriaF__Feedb__4F7CD00D");
            });

            modelBuilder.Entity<CriteriaMaster>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.CriteriaName).HasMaxLength(30);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");

                entity.HasOne(d => d.CriteriaType)
                    .WithMany(p => p.CriteriaMaster)
                    .HasForeignKey(d => d.CriteriaTypeId)
                    .HasConstraintName("FK__CriteriaM__Crite__4CA06362");
            });

            modelBuilder.Entity<CriteriaTypeMaster>(entity =>
            {
                entity.HasKey(e => e.CriteriaTypeId)
                    .HasName("PK__Criteria__824897A0816C5ECC");

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.CriteriaTypeName).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<StatusMaster>(entity =>
            {
                entity.HasKey(e => e.StatusId)
                    .HasName("PK__StatusMa__C8EE2063E6436141");

                entity.Property(e => e.StatusId).ValueGeneratedNever();

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
