using System;
using System.IO;
using Assignment4.Core;
using static Assignment4.Core.State;
using Assignment4.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Assignment4
{
    public class KanbanContextFactory : IDesignTimeDbContextFactory<KanbanContext>
    {
        public KanbanContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets<Program>()
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("Kanban");

            var optionsBuilder = new DbContextOptionsBuilder<KanbanContext>()
                .UseSqlServer(connectionString);

            return new KanbanContext(optionsBuilder.Options);
        }

        public static void Seed(KanbanContext context)
        {
            context.Database.ExecuteSqlRaw("DELETE dbo.Tasks");
            context.Database.ExecuteSqlRaw("DELETE dbo.Tags");
            context.Database.ExecuteSqlRaw("DELETE dbo.Users");

            context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('dbo.Tasks', RESEED, 0)");
            context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('dbo.Tags', RESEED, 0)");
            context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('dbo.Users', RESEED, 0)");

            var task1 = new Task
            {
                Title = "Implement KanbanContext",
                Description = "Implement the code to the KanbanContext",
                State = State.New,
            };

            var task2 = new Task
            {
                Title = "UML Documentation",
                Description = "Draw UML diagrams to document our code",
                State = State.Active,
            };

            var task3 = new Task
            {
                Title = "Test KanbanContext",
                Description = "Test the KanbanContext's methods",
                State = State.Active,
            };

            var tagImportant = new Tag
            {
                Name = "Important",
                Tasks = new[] { task2, task3 }
            };

            var tagRedundant = new Tag
            {
                Name = "Redundant",
                Tasks = new[] { task1 }
            };

            task1.Tags = new[] { tagRedundant };
            task2.Tags = new[] { tagImportant };
            task3.Tags = new[] { tagImportant };

            context.Users.AddRange(
                new User { Name = "user1", Email = "u1@email.com", Tasks = new[] { task1 } },
                new User { Name = "user2", Email = "u2@email.com", Tasks = new[] { task1, task2 } },
                new User { Name = "user3", Email = "u3@email.com", Tasks = new[] { task3 } },
                new User { Name = "user4", Email = "u4@email.com", Tasks = new[] { task1, task2, task3 } }
            );

            context.SaveChanges();
        }
    }
}