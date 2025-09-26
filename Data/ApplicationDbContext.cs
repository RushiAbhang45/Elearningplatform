using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ELearningPlatform.Models;

namespace ELearningPlatform.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Class> Classes { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Content> Contents { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<Notice> Notices { get; set; }
        public DbSet<StudentProgress> StudentProgresses { get; set; }
        public DbSet<StudentClass> StudentClasses { get; set; }
        public DbSet<ParentChild> ParentChildren { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ----------- SQLite adjustments for Identity -----------
            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                // IdentityRole
                builder.Entity<IdentityRole>(b =>
                {
                    b.Property(r => r.Name).HasColumnType("TEXT");
                    b.Property(r => r.NormalizedName).HasColumnType("TEXT");
                    b.Property(r => r.ConcurrencyStamp).HasColumnType("TEXT");
                });

                // IdentityUser
                builder.Entity<ApplicationUser>(b =>
                {
                    b.Property(u => u.UserName).HasColumnType("TEXT");
                    b.Property(u => u.NormalizedUserName).HasColumnType("TEXT");
                    b.Property(u => u.Email).HasColumnType("TEXT");
                    b.Property(u => u.NormalizedEmail).HasColumnType("TEXT");
                    b.Property(u => u.PasswordHash).HasColumnType("TEXT");
                    b.Property(u => u.SecurityStamp).HasColumnType("TEXT");
                    b.Property(u => u.ConcurrencyStamp).HasColumnType("TEXT");
                    b.Property(u => u.PhoneNumber).HasColumnType("TEXT");
                });

                // Other Identity tables
                builder.Entity<IdentityUserLogin<string>>(b =>
                {
                    b.Property(l => l.ProviderKey).HasColumnType("TEXT");
                    b.Property(l => l.LoginProvider).HasColumnType("TEXT");
                });

                builder.Entity<IdentityUserToken<string>>(b =>
                {
                    b.Property(t => t.Value).HasColumnType("TEXT");
                    b.Property(t => t.LoginProvider).HasColumnType("TEXT");
                    b.Property(t => t.Name).HasColumnType("TEXT");
                });

                builder.Entity<IdentityUserClaim<string>>(b =>
                {
                    b.Property(c => c.ClaimType).HasColumnType("TEXT");
                    b.Property(c => c.ClaimValue).HasColumnType("TEXT");
                });

                builder.Entity<IdentityRoleClaim<string>>(b =>
                {
                    b.Property(c => c.ClaimType).HasColumnType("TEXT");
                    b.Property(c => c.ClaimValue).HasColumnType("TEXT");
                });
            }

            // ----------- Your existing model relationships -----------
            builder.Entity<Subject>()
                .HasOne(s => s.Class)
                .WithMany(c => c.Subjects)
                .HasForeignKey(s => s.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Chapter>()
                .HasOne(ch => ch.Subject)
                .WithMany(s => s.Chapters)
                .HasForeignKey(ch => ch.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Content>()
                .HasOne(c => c.Chapter)
                .WithMany(ch => ch.Contents)
                .HasForeignKey(c => c.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Content>()
                .HasOne(c => c.CreatedBy)
                .WithMany()
                .HasForeignKey(c => c.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Quiz>()
                .HasOne(q => q.Chapter)
                .WithMany(ch => ch.Quizzes)
                .HasForeignKey(q => q.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<QuizQuestion>()
                .HasOne(qq => qq.Quiz)
                .WithMany(q => q.Questions)
                .HasForeignKey(qq => qq.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<QuizAttempt>()
                .HasOne(qa => qa.Student)
                .WithMany()
                .HasForeignKey(qa => qa.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<QuizAttempt>()
                .HasOne(qa => qa.Quiz)
                .WithMany(q => q.Attempts)
                .HasForeignKey(qa => qa.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentProgress>()
                .HasOne(sp => sp.Student)
                .WithMany(u => u.StudentProgresses)
                .HasForeignKey(sp => sp.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentProgress>()
                .HasOne(sp => sp.Chapter)
                .WithMany()
                .HasForeignKey(sp => sp.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentClass>()
                .HasOne(sc => sc.Student)
                .WithMany()
                .HasForeignKey(sc => sc.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentClass>()
                .HasOne(sc => sc.Class)
                .WithMany(c => c.StudentClasses)
                .HasForeignKey(sc => sc.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Notice>()
                .HasOne(n => n.CreatedBy)
                .WithMany()
                .HasForeignKey(n => n.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Notice>()
                .HasOne(n => n.Class)
                .WithMany()
                .HasForeignKey(n => n.ClassId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ParentChild>()
                .HasOne(pc => pc.Parent)
                .WithMany(u => u.ParentChildren)
                .HasForeignKey(pc => pc.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ParentChild>()
                .HasOne(pc => pc.Child)
                .WithMany(u => u.ChildParents)
                .HasForeignKey(pc => pc.ChildId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentProgress>()
                .HasIndex(sp => new { sp.StudentId, sp.ChapterId })
                .IsUnique();

            builder.Entity<StudentClass>()
                .HasIndex(sc => new { sc.StudentId, sc.ClassId })
                .IsUnique();

            builder.Entity<ParentChild>()
                .HasIndex(pc => new { pc.ParentId, pc.ChildId })
                .IsUnique();
        }
    }
}
