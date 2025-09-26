namespace ELearningPlatform.Models
{
    public class QuizAttempt
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public int QuizId { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ApplicationUser Student { get; set; } = null!;
        public virtual Quiz Quiz { get; set; } = null!;
    }
}