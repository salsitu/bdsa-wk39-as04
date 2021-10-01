using System;
using System.IO;
using Assignment4;
using Assignment4.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Lecture04
{
    public class ComicsContextFactory : IDesignTimeDbContextFactory<KanbanContext>
    {
        public KanbanContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets<Program>()
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("Comics");

            var optionsBuilder = new DbContextOptionsBuilder<KanbanContext>()
                .UseSqlServer(connectionString);

            return new KanbanContext(optionsBuilder.Options);
        }

        public static void Seed(KanbanContext context)
        {
            context.Database.ExecuteSqlRaw("DELETE dbo.CharacterPower");
            context.Database.ExecuteSqlRaw("DELETE dbo.Characters");
            context.Database.ExecuteSqlRaw("DELETE dbo.Powers");
            context.Database.ExecuteSqlRaw("DELETE dbo.Cities");
            context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('dbo.Powers', RESEED, 0)");
            context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('dbo.Cities', RESEED, 0)");
            context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('dbo.Characters', RESEED, 0)");

            context.SaveChanges();
        }
    }
}