using System.ComponentModel.DataAnnotations;

namespace ELearningPlatform.Models
{
    public class Quiz
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int ChapterId { get; set; }
        public int TimeLimit { get; set; } = 30; // minutes
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Chapter Chapter { get; set; } = null!;
        public virtual ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
        public virtual ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
    }
}