using System.Collections.Generic;
using System.Collections.Immutable;
using Assignment4.Core;
using System.Linq;
using System;

namespace Assignment4.Entities
{
    public class TaskRepository : ITaskRepository
    {
        private KanbanContext _kanbanContext;

        public TaskRepository(KanbanContext kanbanContext)
        {
            _kanbanContext = kanbanContext;
        }

        public (Response Response, int TaskId) Create(TaskCreateDTO task)
        {
            _kanbanContext.Tasks.Add(
                new Task
                {
                    Title = task.Title,
                    AssignedTo = _kanbanContext.Users.FirstOrDefault(x => x.Id == task.AssignedToId),
                    Description = task.Description,
                    Created = DateTime.Now,
                    State = State.New,
                    Tags = task.Tags.Select(x => new Tag
                    {
                        Id = _kanbanContext.Tags.FirstOrDefault(y => y.Name == x).Id,
                        Name = x,
                        Tasks = _kanbanContext.Tags.FirstOrDefault(y => y.Name == x).Tasks
                    }).ToList(),
                    StateUpdated = DateTime.Now
                }
            );

            _kanbanContext.SaveChanges();
            return (Response.Created, _kanbanContext.Tasks.Last().Id);
        }

        public Response Delete(int taskId)
        {
            var task = _kanbanContext.Tasks.FirstOrDefault(x => x.Id == taskId);
            if (task == null) return Response.NotFound;
            if (task.State == State.Removed || task.State == State.Closed || task.State == State.Removed) return Response.Conflict;
            if (task.State == State.Active) task.State = State.Removed;

            if (task.State == State.New)
            {
                _kanbanContext.Tasks.Remove(task);
            }

            _kanbanContext.SaveChanges();

            return Response.Deleted;
        }

        public TaskDetailsDTO Read(int taskId)
        {
            var task = _kanbanContext.Tasks.FirstOrDefault(x => x.Id.Equals(taskId));
            return new TaskDetailsDTO(task.Id, task.Title, task.Description, task.Created, task.AssignedTo.Name, (IReadOnlyCollection<string>)task.Tags.Select(x => x.Name), task.State, task.StateUpdated);
        }

        public IReadOnlyCollection<TaskDTO> ReadAll()
        {
            return _kanbanContext.Tasks.Select<Task, TaskDTO>(x => new TaskDTO(
                x.Id,
                x.Title,
                x.AssignedTo.Name,
                x.Tags.Select(y => y.Name).ToImmutableList<string>(),
                x.State
            )).ToImmutableList<TaskDTO>();
        }

        public IReadOnlyCollection<TaskDTO> ReadAllByState(State state)
        {
            return _kanbanContext.Tasks.Select<Task, TaskDTO>(x => new TaskDTO(
                x.Id,
                x.Title,
                x.AssignedTo.Name,
                x.Tags.Select(y => y.Name).ToImmutableList<string>(),
                x.State
            )).Where(x => x.State == state).ToImmutableList<TaskDTO>();
        }

        public IReadOnlyCollection<TaskDTO> ReadAllByTag(string tag)
        {
            return _kanbanContext.Tasks.Select<Task, TaskDTO>(x => new TaskDTO(
                x.Id,
                x.Title,
                x.AssignedTo.Name,
                x.Tags.Select(y => y.Name).ToImmutableList<string>(),
                x.State
            )).Where(x => x.Tags.Contains(tag)).ToImmutableList<TaskDTO>();
        }

        public IReadOnlyCollection<TaskDTO> ReadAllByUser(int userId)
        {
            return _kanbanContext.Tasks.Where(x => x.AssignedTo.Id == userId).Select<Task, TaskDTO>(x => new TaskDTO(
                x.Id,
                x.Title,
                x.AssignedTo.Name,
                x.Tags.Select(y => y.Name).ToImmutableList<string>(),
                x.State
            )).ToImmutableList<TaskDTO>();
        }

        public IReadOnlyCollection<TaskDTO> ReadAllRemoved()
        {
            return _kanbanContext.Tasks.Where(x => x.State == State.Removed).Select<Task, TaskDTO>(x => new TaskDTO(
                x.Id,
                x.Title,
                x.AssignedTo.Name,
                x.Tags.Select(y => y.Name).ToImmutableList<string>(),
                x.State
            )).ToImmutableList<TaskDTO>();
        }

        public Response Update(TaskUpdateDTO task)
        {
            var t = _kanbanContext.Tasks.FirstOrDefault(x => x.Id == task.Id);
            
            if (t == null) return Response.NotFound;

            t.Title = task.Title;
            t.AssignedTo = _kanbanContext.Users.FirstOrDefault(x => x.Id == task.AssignedToId);
            t.Description = task.Description;
            if (t.State != task.State)
            {
                t.State = task.State;
                t.StateUpdated = DateTime.Now;
            }
            t.Tags = GetTags(task.Tags).ToList();

            _kanbanContext.SaveChanges();

            return Response.Updated;
        }

        private IEnumerable<Tag> GetTags(IEnumerable<string> tags)
        {
            var existing = _kanbanContext.Tags.Where(x => tags.Contains(x.Name)).ToDictionary(x => x.Name);

            foreach(var tag in tags)
            {
                yield return existing.TryGetValue(tag, out var x) ? x : new Tag { Name = tag };
            }
        }
    }
}
