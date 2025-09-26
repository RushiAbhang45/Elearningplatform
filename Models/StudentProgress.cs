namespace ELearningPlatform.Models
{
    public class StudentProgress
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public int ChapterId { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ApplicationUser Student { get; set; } = null!;
        public virtual Chapter Chapter { get; set; } = null!;
    }
}