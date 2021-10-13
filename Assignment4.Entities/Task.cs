using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Assignment4.Core;
using System;

namespace Assignment4.Entities
{
    public class Task
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        #nullable enable
        public User? AssignedTo { get; set; }

        public string? Description { get; set; }

        public DateTime Created { get; set; }

        [Required]
        public State State { get; set; }

        #nullable disable
        public ICollection<Tag> Tags { get; set; }

        public DateTime StateUpdated { get; set; }
    }
}
