using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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
            
            // -----------

            _context = context;
            _repo = new TaskRepository(_context);

        }

        [Fact]
        public void Create_creates_new_Task_with_generated_id()
        {
            
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
