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
            var task1 = new Task { Title = "Implement KanbanContext", Description = "Implement the code to the KanbanContext", Created = DateTime.Now, State = State.New, StateUpdated = DateTime.Now };
            var task2 = new Task { Title = "UML Documentation", Description = "Draw UML diagrams to document our code", Created = DateTime.Now, State = State.Active, StateUpdated = DateTime.Now };
            var task3 = new Task { Title = "Test KanbanContext", Description = "Test the KanbanContext's methods", Created = DateTime.Now, State = State.Removed, StateUpdated = DateTime.Now };

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
                AssignedToId = 2,
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
                t => Assert.Equal(new TaskDTO(3, "Test KanbanContext", "user3", new List<String>() { "Important" }.ToImmutableList(), State.Removed), t)
            );
        }
        [Fact]
        public void ReadAllByState_Given_Active_State_returns_all_tasks_With_Active_State()
        {
            var tasks = _repo.ReadAllByState(State.Active);

            Assert.Collection(tasks,
                t => Assert.Equal(new TaskDTO(2, "UML Documentation", "user2", new List<String>() { "Important" }.ToImmutableList(), State.Active), t)
            );
        }

        [Fact]
        public void ReadAllByTag_Given_Important_Tag_returns_all_tasks_With_Important_tag()
        {
            var tasks = _repo.ReadAllByTag("Important");

            Assert.Collection(tasks,
                t => Assert.Equal(new TaskDTO(2, "UML Documentation", "user2", new List<String>() { "Important" }.ToImmutableList(), State.Active), t),
                t => Assert.Equal(new TaskDTO(3, "Test KanbanContext", "user3", new List<String>() { "Important" }.ToImmutableList(), State.Removed), t)
            );
        }

        [Fact]
        public void ReadAllByUser_Given_UserId_1_returns_all_tasks_With_UserId_1()
        {
            var tasks = _repo.ReadAllByUser(1);

            Assert.Collection(tasks,
                t => Assert.Equal(new TaskDTO(1, "Implement KanbanContext", "user1", new List<String>() { "Redundant" }.ToImmutableList(), State.New), t)
            );
        }

        [Fact]
        public void Delete_of_Task_with_State_New_removes_it_from_database()
        {
            var repository = new TaskRepository(_context);
            var response = repository.Delete(1);
            Assert.Equal(Response.Deleted, response);
            Assert.Null(_context.Tasks.Find(1));
        }

        [Fact]
        public void Delete_of_existing_Task_with_State_Active_sets_its_State_to_Removed()
        {
            var repository = new TaskRepository(_context);
            var response = repository.Delete(2);
            var actual = _context.Tasks.Find(2).State;
            Assert.Equal(Response.Deleted, response);
            Assert.Equal(State.Removed, actual);
        }

        [Fact]
        public void Delete_of_Task_with_State_different_from_Active_or_New_changes_nothing_and_returns_Reponse_Conflict()
        {
            var repository = new TaskRepository(_context);
            var response = repository.Delete(3);
            var actual = _context.Tasks.Find(3).State;
            Assert.Equal(Response.Conflict, response);
            Assert.Equal(State.Removed, actual);
        }

        [Fact]
        public void Delete_of_non_existing_Task_returns_Response_NotFound()
        {
            var repository = new TaskRepository(_context);
            var actual = repository.Delete(4);
            Assert.Equal(Response.NotFound, actual);
        }

        [Fact]
        public void Delete_of_Task_with_State_Removed_Closed_or_Resolved_returns_Response_Conflict()
        {
            var repository = new TaskRepository(_context);
            var actual = repository.Delete(3);
            Assert.Equal(Response.Conflict, actual);
        }

        [Fact]
        public void Update_updates_existing_task()
        {
            var repository = new TaskRepository(_context);

            var task = new TaskUpdateDTO
            {
                Id = 1,
                Title = "Implement TaskRepository",
                AssignedToId = 4,
                Description = "Implement the code to the TaskRepository",
                Tags = new List<string> { "Important" }.ToImmutableList(),
                State = State.Active
            };

            var response = repository.Update(task);
            Assert.Equal(Response.Updated, response);

            var task1 = repository.Read(1);
            Assert.Equal("Implement TaskRepository", task1.Title);
            Assert.Equal(4, _context.Users.Find(4).Id);
            Assert.Equal("Implement the code to the TaskRepository", task1.Description);
            Assert.True(task1.Tags.SequenceEqual(new List<string>() { "Important" }.ToImmutableList()));
            Assert.Equal(State.Active, task1.State);
        }

        [Fact]
        public void Update_given_non_existing_id_returns_Response_NotFound()
        {
            var repository = new TaskRepository(_context);
            var task = new TaskUpdateDTO
            {
                Id = 45,
                Title = "Implement TaskRepository",
                AssignedToId = 4,
                Description = "Implement the code to the TaskRepository",
                Tags = new List<string>() { "Important" }.ToImmutableList(),
                State = State.Active
            };

            var actual = repository.Update(task);
            Assert.Equal(Response.NotFound, actual);
        }

        [Fact]
        public void Update_of_Task_State_updates_StateUpdated()
        {
            var repository = new TaskRepository(_context);
            var task = new TaskUpdateDTO
            {
                Id = 1,
                Title = "Implement KanbanContext",
                AssignedToId = 1,
                Description = "Implement the code to the TaskRepository",
                Tags = new List<string>() { }.ToImmutableList(),
                State = State.Active
            };

            var response = repository.Update(task);
            var expected = DateTime.UtcNow;
            Assert.Equal(Response.Updated, response);

            var actual = repository.Read(1).StateUpdated;
            Assert.Equal(expected, actual, precision: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void Update_of_Task_assignedTo_with_non_existing_id_returns_Response_BadRequest()
        {
            var repository = new TaskRepository(_context);
            var task = new TaskUpdateDTO
            {
                Id = 1,
                Title = "Implement KanbanContext",
                AssignedToId = 5,
                Description = "Implement the code to the TaskRepository",
                Tags = new List<string> { "Important" }.ToImmutableList(),
                State = State.New
            };

            var actual = repository.Update(task);
            Assert.Equal(Response.BadRequest, actual);
        }

        [Fact]
        public void Read_Task_Return_TaskDetailsDTO()
        {
            var actual = _repo.Read(1);
            var expected = new TaskDetailsDTO(1, "Implement KanbanContext", "Implement the code to the KanbanContext", actual.Created, "user1", actual.Tags, State.New, actual.StateUpdated);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Read_Task_Return_Null_When_Task_Does_Not_Exist()
        {
            var actual = _repo.Read(-1);
            Assert.Null(actual);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
