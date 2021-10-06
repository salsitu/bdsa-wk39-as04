using System.Collections.Generic;
using System.Collections.Immutable;
using Assignment4.Core;
using System.Linq;
using System.Data.SqlClient;

namespace Assignment4.Entities
{
    public class TaskRepository : ITaskRepository
    {
        private KanbanContext kanbanContext;

        public IReadOnlyCollection<TaskDTO> All()
        {
            return kanbanContext.Tasks.Select<Task, TaskDTO>(x => new TaskDTO
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                AssignedToId = x.AssignedTo.Id,
                Tags = x.Tags.Select(y => y.ToString()).ToImmutableList<string>(),
                State = x.State
            }).ToImmutableList<TaskDTO>();
        }

        public int Create(TaskDTO task)
        {
            var tagList = new List<Tag>();
            tagList = task.Tags.Select(x => new Tag
            {
                Id = kanbanContext.Tags.SingleOrDefault(y => y.Name == x).Id,
                Name = x,
                Tasks = kanbanContext.Tags.SingleOrDefault(y => y.Name == x).Tasks
            }).ToList();
            var taskConvert = new Task
            {
                Id = task.Id,
                Title = task.Title,
                AssignedTo = kanbanContext.Users.SingleOrDefault(x => x.Id == task.AssignedToId),
                Description = task.Description,
                State = task.State,
                Tags = tagList
            };
            kanbanContext.Tasks.Add(taskConvert);
            kanbanContext.SaveChanges();
            return task.Id;
        }

        public void Delete(int taskId)
        {
            var task = kanbanContext.Tasks.SingleOrDefault(x => x.Id.Equals(taskId));
            kanbanContext.Tasks.Remove(task);
            kanbanContext.SaveChanges();
        }

        public void Dispose()
        {
            kanbanContext.Dispose();
        }

        public TaskDetailsDTO FindById(int id)
        {
            var task = kanbanContext.Tasks.SingleOrDefault(x => x.Id == id);
            var user = kanbanContext.Users.SingleOrDefault(x => x.Tasks.Contains(task));
            return new TaskDetailsDTO
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                AssignedToId = user.Id,
                AssignedToName = user.Name,
                AssignedToEmail = user.Email,
                Tags = task.Tags.Select(x => x.Name),
                State = task.State
            };
        }

        public void Update(TaskDTO task)
        {
            var t = kanbanContext.Tasks.SingleOrDefault(x => task.Id == x.Id);
            if (t != null) Delete(t.Id);
            Create(task);
        }
    }
}
