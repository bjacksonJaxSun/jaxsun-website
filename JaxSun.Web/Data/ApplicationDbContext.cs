using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using JaxSun.Web.Models;

namespace JaxSun.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Time Tracking DbSets
        public DbSet<TimeEntry> TimeEntries { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<TimeCategory> TimeCategories { get; set; }
        public DbSet<IdeaSubmissionModel> IdeaSubmissions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure TimeEntry relationships
            builder.Entity<TimeEntry>()
                .HasOne(te => te.User)
                .WithMany()
                .HasForeignKey(te => te.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TimeEntry>()
                .HasOne(te => te.Project)
                .WithMany(p => p.TimeEntries)
                .HasForeignKey(te => te.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TimeEntry>()
                .HasOne(te => te.Category)
                .WithMany(tc => tc.TimeEntries)
                .HasForeignKey(te => te.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Project relationships
            builder.Entity<Project>()
                .HasOne(p => p.CreatedBy)
                .WithMany()
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Project>()
                .HasOne(p => p.IdeaSubmission)
                .WithMany()
                .HasForeignKey(p => p.IdeaSubmissionId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure decimal precision
            builder.Entity<Project>()
                .Property(p => p.EstimatedHours)
                .HasPrecision(8, 2);

            builder.Entity<Project>()
                .Property(p => p.Budget)
                .HasPrecision(10, 2);

            // Configure indexes for performance
            builder.Entity<TimeEntry>()
                .HasIndex(te => te.UserId);

            builder.Entity<TimeEntry>()
                .HasIndex(te => te.ProjectId);

            builder.Entity<TimeEntry>()
                .HasIndex(te => te.StartTime);

            builder.Entity<Project>()
                .HasIndex(p => p.Status);

            builder.Entity<Project>()
                .HasIndex(p => p.CreatedById);

            // Configure IdeaSubmissionModel for EF Core
            builder.Entity<IdeaSubmissionModel>()
                .HasKey(i => i.Id);

            builder.Entity<IdeaSubmissionModel>()
                .Property(i => i.Id)
                .ValueGeneratedOnAdd();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=JaxSun.db");
            }
        }
    }
}