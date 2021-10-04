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
            return kanbanContext.Tasks.Select<Task, TaskDTO>(x => new(
                x.Id,
                x.Title,
                x.Description,
                x.AssignedTo.Id,
                x.Tags.Select(y => y.ToString()).ToImmutableList<string>(),
                x.State
            )).ToImmutableList<TaskDTO>();
        }

        public int Create(TaskDTO task)
        {
            var tagList = new List<Tag>();
            tagList = task.Tags.Select(x => new Tag(x)).ToList();
            var taskConvert = new Task(task.Id, task.Title,
                kanbanContext.Users.SingleOrDefault(x => x.Id == task.AssignedToId),
                task.Description, task.State, tagList);
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
            return null;
        }

        public void Update(TaskDTO task)
        {
            //update
            kanbanContext.SaveChanges();
        }
    }
}
