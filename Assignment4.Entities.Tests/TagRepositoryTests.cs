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
    public class TagRepositoryTests : IDisposable
    {
        private readonly KanbanContext _context;
        private readonly TagRepository _repo;
        public TagRepositoryTests()
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
            _repo = new TagRepository(_context);
            _context.SaveChanges();
        }

        [Fact]
        public void Create_Returns_response_created_and_gets_id_3()
        {
            var actual = _repo.Create(new TagCreateDTO { Name = "testtag" });
            Assert.Equal((Response.Created, 3), actual);
        }
        [Fact]
        public void Create_tag_which_already_exist_return_conflict()
        {
            var actual = _repo.Create(new TagCreateDTO { Name = "Important"});
            Assert.Equal((Response.Conflict,0),actual);
        }

        [Fact]
        public void Delete_properly_deletes_tag_in_use_if_force_is_used()
        {
            var response = _repo.Delete(1, true);
            Assert.Equal(Response.Deleted, response);
            Assert.Null(_context.Tags.Find(1));
        }

        [Fact]
        public void Delete_returns_conflict_if_tag_is_in_use_and_force_is_not_used()
        {
            var respone = _repo.Delete(1, false);
            Assert.Equal(Response.Conflict, respone);
        }

        [Fact]
        public void Delete_deletes_tag_not_in_use_if_force_is_not_used()
        {
            _repo.Create(new TagCreateDTO { Name = "testtag" });
            var response = _repo.Delete(3, false);
            Assert.Equal(Response.Deleted, response);
            Assert.Null(_context.Tags.Find(3));
        }

        [Fact]
        public void Delete_returns_NotFound_given_invalid_id()
        {
            var respone = _repo.Delete(-1, true);
            Assert.Equal(Response.NotFound, respone);
        }

        [Fact]
        public void ReadAll_returns_all_Tags()
        {
            var tags = _repo.ReadAll();
            Assert.Collection(tags,
                t => Assert.Equal(new TagDTO(1, "Redundant"), t),
                t => Assert.Equal(new TagDTO(2, "Important"), t)
            );
        }

        [Fact]
        public void Read_returns_null_given_invalid_id()
        {
            var actual = _repo.Read(3);
            Assert.Null(actual);
        }

        [Fact]
        public void Read_returns_tag_given_valid_id()
        {
            var actual = _repo.Read(2);
            var expected = new TagDTO(2, "Important");
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Update_returns_Response_NotFound_for_invalid_tag()
        {
            var tag = new TagUpdateDTO
            {
                Id = 3,
                Name = "Tested"
            };
            var actual = _repo.Update(tag);
            Assert.Equal(Response.NotFound, actual);
        }

        [Fact]
        public void Update_updates_tag()
        {
            var tag = new TagUpdateDTO
            {
                Id = 1,
                Name = "Tested"
            };

            var response = _repo.Update(tag);
            Assert.Equal(Response.Updated, response);

            var tag1 = _repo.Read(1);
            Assert.Equal(1, tag1.Id);
            Assert.Equal("Tested", tag1.Name);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
