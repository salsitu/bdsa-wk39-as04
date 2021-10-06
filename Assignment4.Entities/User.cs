using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assignment4.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set;}

        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set;}

        public ICollection<Task> Tasks { get; set;}
    }
}
