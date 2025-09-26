using System.ComponentModel.DataAnnotations;

namespace ELearningPlatform.Models
{
    public class Chapter
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int SubjectId { get; set; }
        public int OrderIndex { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Subject Subject { get; set; } = null!;
        public virtual ICollection<Content> Contents { get; set; } = new List<Content>();
        public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    }
}