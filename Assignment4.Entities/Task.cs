using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Assignment4.Core;

namespace Assignment4.Entities
{
    public class Task
    {
        public Task(int id, string title, User? assignedTo, string? description, State state, IReadOnlyCollection<Tag> tags)
        {
            Id = id;
            Title = title;
            AssignedTo = assignedTo;
            Description = description;
            State = state;
            Tags = (ICollection<Tag>)tags;
        }
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public User? AssignedTo { get; set; }

        public string? Description { get; set; }

        [Required]
        public State State { get; set; }

        public ICollection<Tag> Tags { get; set; }
    }
}
