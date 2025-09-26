using System.ComponentModel.DataAnnotations;

namespace ELearningPlatform.Models
{
    public class Class
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
        public virtual ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
    }
}