using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Models;

namespace RaceDayAPI.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<EventType> EventTypes { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Enrolment> Enrolments { get; set; }

        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .ToTable("Users");

            modelBuilder.Entity<EventType>()
                .ToTable("EventTypes");

            modelBuilder.Entity<Event>()
                .ToTable("Events");

            modelBuilder.Entity<Category>()
                .ToTable("Categories");

            modelBuilder.Entity<Enrolment>()
                .ToTable("Enrolments");

            modelBuilder.Entity<Result>()
                .ToTable("Results");
        }
    }
}