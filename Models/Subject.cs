using System.ComponentModel.DataAnnotations;

namespace ELearningPlatform.Models
{
    public class Subject
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int ClassId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Class Class { get; set; } = null!;
        public virtual ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
    }
}