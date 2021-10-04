using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assignment4.Entities
{
    public class Tag
    {
        public Tag(string name)
        {
            Name = name;
        }

        public int Id { get; set; }

        [Required]
        [Key]
        [StringLength(50)]
        public string Name { get; set; }

        public ICollection<Task> Tasks { get; set; }
    }
}
