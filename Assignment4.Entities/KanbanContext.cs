using Assignment4.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Assignment4.Entities
{
    public class KanbanContext : DbContext
    {
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<User> Users { get; set; }

        public KanbanContext(DbContextOptions<KanbanContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Task>(e =>
            {
                e.HasIndex(e => e.Id).IsUnique();
                e.Property(e => e.State)
                    .HasConversion(new EnumToStringConverter<State>());
            });

            modelBuilder.Entity<Tag>(e =>
            {
                //e.HasKey(e => e.Id);
                //e.Property(e => e.Id).UseIdentityColumn();
                e.HasIndex(e => e.Id).IsUnique();
                e.HasIndex(e => e.Name).IsUnique();
            });

            modelBuilder.Entity<User>(e =>
            {
                e.HasIndex(e => e.Id).IsUnique();
                e.HasIndex(e => e.Email).IsUnique();
            });
        }
    }
}
