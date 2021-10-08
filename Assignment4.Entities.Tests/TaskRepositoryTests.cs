using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Assignment4.Core;
using Xunit;

namespace Assignment4.Entities.Tests
{
    public class TaskRepositoryTests : IDisposable
    {
        private readonly KanbanContext _context;
        private readonly TaskRepository _repo;

        public TaskRepositoryTests()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            var builder = new DbContextOptionsBuilder<KanbanContext>().UseSqlite(connection);
            var context = new KanbanContext(builder.Options);
            context.Database.EnsureCreated();

            // Insert Data
            var task1 = new Task { Title = "Implement KanbanContext", Description = "Implement the code to the KanbanContext", State = State.New };
            var task2 = new Task { Title = "UML Documentation", Description = "Draw UML diagrams to document our code", State = State.Active };
            var task3 = new Task { Title = "Test KanbanContext", Description = "Test the KanbanContext's methods", State = State.Active };

            var tagImportant = new Tag { Name = "Important", Tasks = new List<Task>() { task2, task3 } };
            var tagRedundant = new Tag { Name = "Redundant", Tasks = new List<Task>() { task1 } };

            task1.Tags = new List<Tag>() { tagRedundant };
            task2.Tags = new List<Tag>() { tagImportant };
            task3.Tags = new List<Tag>() { tagImportant };

            context.Users.AddRange(
                new User { Name = "user1", Email = "u1@email.com", Tasks = new List<Task>() { task1 } },
                new User { Name = "user2", Email = "u2@email.com", Tasks = new List<Task>() { task2 } },
                new User { Name = "user3", Email = "u3@email.com", Tasks = new List<Task>() { task3 } },
                new User { Name = "user4", Email = "u4@email.com", Tasks = new List<Task>() { } }
            );
            // -----------

            _context = context;
            _repo = new TaskRepository(_context);
            _context.SaveChanges();
        }

        [Fact]
        public void Create_creates_new_Task_with_generated_id()
        {
            var actual = _repo.Create(new TaskCreateDTO
            {
                Title = "title",
                AssignedToId = 1,
                Description = "testidgen",
                Tags = new List<string>()
            });

            Assert.Equal((Response.Created, 4), actual);
        }

        [Fact]
        public void Create_returns_bad_request_if_user_does_not_exist()
        {
            var actual = _repo.Create(new TaskCreateDTO { AssignedToId = -1 });
            Assert.Equal((Response.BadRequest, 0), actual);
        }


        [Fact]
        public void ReadAll_returns_all_tasks()
        {
            var tasks = _repo.ReadAll();
            
            Assert.Collection(tasks,
                t => Assert.Equal(new TaskDTO(1, "Implement KanbanContext", "user1", new List<String>() { "Redundant" }.ToImmutableList(), State.New), t),
                t => Assert.Equal(new TaskDTO(2, "UML Documentation", "user2", new List<String>() { "Important" }.ToImmutableList(), State.Active), t),
                t => Assert.Equal(new TaskDTO(3, "Test KanbanContext", "user3", new List<String>() { "Important" }.ToImmutableList(), State.Active), t)
            );
        }

        [Fact]
        public void Test_Lists_Equals()
        {
            var l1 = new List<int> { 1, 2, 3 };
            var l2 = new List<int> { 1, 2, 3 };
            
            Assert.True(l1.SequenceEqual(l2));
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
